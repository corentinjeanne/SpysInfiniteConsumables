using System.Collections;
using System.Collections.Generic;
using Terraria;
using SPIC.ConsumableGroup;

namespace SPIC;

public enum FilterFlags {
    Default   = NonGlobal | Enabled,
    NonGlobal = 0b0001,
    Global    = 0b0010,
    Enabled   = 0b0100,
    Disabled  = 0b1000,
}


public static class InfinityManager {

    internal static int LCM { get; private set; } = 1;

    public static void Register<TImplementation>(ItemGroup<TImplementation> group) where TImplementation : ItemGroup<TImplementation>, IConsumableGroup, new() => Register(group, false);
    public static void RegisterAsGlobal<TImplementation, TConsumable>(ConsumableGroup<TImplementation, TConsumable> group) where TImplementation : ConsumableGroup<TImplementation, TConsumable>, IConsumableGroup, new() => Register(group, true);
    internal static void Register<TImplementation, TConsumable>(ConsumableGroup<TImplementation, TConsumable> group, bool global) where TImplementation : ConsumableGroup<TImplementation, TConsumable>, IConsumableGroup, new() {
        if (group.UID != 0) throw new System.ArgumentException("This type has already been registered", nameof(group));
        int id = group.UID = global ? s_nextGlobalID-- : s_nextTypeID++;
        
        s_groups[id] = group;
        s_caches[id] = new();
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        LCM = s_groups.Count * LCM / GCD(s_groups.Count, LCM);
    }

    public static IConsumableGroup ConsumableGroup(int id) => s_groups[id];
    public static IConsumableGroup<TConsumable> ConsumableGroup<TConsumable>(int id) => ConsumableGroup(id) as IConsumableGroup<TConsumable>;
    public static IConsumableGroup ConsumableGroup(string mod, string Name) => s_groups.FindValue(kvp => kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);

    public static bool IsEnabled(IConsumableGroup group) => group is not IToggleable toogleable || toogleable.Enabled;
    public static bool IsEnabled(int id) => IsEnabled(ConsumableGroup(id));
    public static bool IsUsed(this Item item, int group) => item.UsedConsumableTypes().Find(t => t.UID == group) != null;

    public static IEnumerable<IConsumableGroup> ConsumableGroups(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) => ConsumableGroups<IConsumableGroup>(filters, noOrdering);
    public static IEnumerable<TConsumableGroup> ConsumableGroups<TConsumableGroup>(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) where TConsumableGroup: IConsumableGroup {
        bool enabled = filters.HasFlag(FilterFlags.Enabled);
        bool disabled = filters.HasFlag(FilterFlags.Disabled);

        bool MatchsFlags(int id, IConsumableGroup t) => t is TConsumableGroup && ((enabled && disabled) || (enabled == IsEnabled(id)));

        if (filters.HasFlag(FilterFlags.NonGlobal)) {
            if (!noOrdering) {
                foreach (DictionaryEntry entry in Requirements.EnabledTypes) {
                    IConsumableGroup type = ((Configs.ConsumableTypeDefinition)entry.Key).ConsumableType;
                    if (MatchsFlags(type.UID, type)) yield return (TConsumableGroup)type;
                }
            } else {
                foreach ((int id, IConsumableGroup type) in s_groups) {
                    if (id > 0 && MatchsFlags(id, type)) yield return (TConsumableGroup)type;
                }
            }
        }
        if (filters.HasFlag(FilterFlags.Global)) {
            foreach ((int id, IConsumableGroup type) in s_groups) {
                if (id < 0 && MatchsFlags(id, type)) yield return (TConsumableGroup)type;
            }
        }
    }

    public static IReadOnlyList<IStandardGroup> UsedConsumableTypes(this Item item){
        if(s_usedTypes.TryGetValue(item.type, out IReadOnlyList<IStandardGroup> types)) return types;
        List<IStandardGroup> used = new();
        foreach (IStandardGroup consumableType in ConsumableGroups()) {
            if(item.GetRequirement(consumableType.UID) is NoRequirement) continue;
            used.Add(consumableType);
            if (Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
        }
        return s_usedTypes[item.type] = used;
    }

    public static Category GetCategory(this Item item, int groupID){
        ICategory group = ConsumableGroup(groupID) as ICategory;
        return s_caches[groupID].GetOrAdd(CacheType.Category, group.CacheID(item), () => group.GetCategory(item));
    }
    public static Category GetCategory<TConsumable>(TConsumable consumable, int groupID){
        ICategory<TConsumable> group = ConsumableGroup(groupID) as ICategory<TConsumable>;
        return s_caches[groupID].GetOrAdd(CacheType.Category, group.CacheID(consumable), () => group.GetCategory(consumable));
    }
    public static TCategory GetCategory<TCategory>(this Item item, int groupID) where TCategory : System.Enum => (TCategory)GetCategory(item, groupID);
    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, int groupID) where TCategory : System.Enum => (TCategory)GetCategory(consumable, groupID);

    public static bool HasCategoryOverride(int cacheID, int consumableID, out Category category) {
        Item a = new();
        var cat = GetCategory<Item, VanillaConsumableTypes.AmmoCategory>(a, VanillaConsumableTypes.Ammo.ID);
        IConsumableGroup inf = ConsumableGroup(consumableID);
        category = Category.None;
        return inf is IDetectable && CategoryDetection.HasDetectedCategory(cacheID, consumableID, out category);
    }

    public static IRequirement GetRequirement(this Item item, int groupID) {
        IConsumableGroup group = ConsumableGroup(groupID);
        return s_caches[groupID].GetOrAdd(CacheType.Requirement, group.CacheID(item), () => group.GetRequirement(item) ?? new NoRequirement());
    }
    public static IRequirement GetRequirement<TConsumable>(TConsumable consumable, int groupID) {
        IConsumableGroup<TConsumable> group = ConsumableGroup(groupID) as IConsumableGroup<TConsumable>;
        return s_caches[groupID].GetOrAdd(CacheType.Requirement, group.CacheID(consumable), () => group.GetRequirement(consumable) ?? new NoRequirement());
    }

    public static bool HasRequirementOverride(int cacheID, int consumableID, out IRequirement requirement) {
        requirement = new NoRequirement();
        return false;
    }

    public static Infinity GetInfinity(this Player player, Item item, int groupID) {
        IConsumableGroup group = ConsumableGroup(groupID);
        Infinity InfGetter() => GetInfinity(item, group.CountConsumables(player, item), groupID);
        return UseCache(player) ? s_caches[groupID].GetOrAdd(CacheType.Infinity, group.CacheID(item), InfGetter) : InfGetter();
    }
    public static Infinity GetInfinity<TConsumable>(this Player player, TConsumable consumable, int groupID) {
        IConsumableGroup<TConsumable> group = ConsumableGroup(groupID) as IConsumableGroup<TConsumable>;
        Infinity InfGetter() => GetInfinity(consumable, group.CountConsumables(player, consumable), groupID);
        return UseCache(player) ? s_caches[groupID].GetOrAdd(CacheType.Infinity, group.CacheID(consumable), InfGetter) : InfGetter();
    }
    
    // TODO implement
    // TODO remove duplicate code
    public static Infinity GetInfinity(this Item item, long count, int groupID) {
        IRequirement req = GetRequirement(item, groupID);
        bool b = HasInfinite<Item>(null, item, 5, groupID);
        return req.Infinity(item, new(count, item.maxStack));
    } 
    public static Infinity GetInfinity<TConsumable>(TConsumable consumable, long count, int groupID) {
        IRequirement req = GetRequirement(consumable, groupID);
        Item item = ConsumableGroup<TConsumable>(groupID).ToItem(consumable);
        return req.Infinity(item, new(count, item.maxStack));
    }

    // TODO remove duplicate code
    public static bool HasInfinite(this Player player, Item item, long consumablesConsumed, int groupID) {
        return IsEnabled(groupID) && (groupID < 0 || item.IsUsed(groupID)) ?
            new ItemCount(consumablesConsumed, item.maxStack) <= player.GetInfinity(item, groupID).Value : player.HasInfinite(item, consumablesConsumed, VanillaConsumableTypes.Mixed.ID);
    }

    // TODO calls for HasInfinite<Item>
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumablesConsumed, int groupID) {
        return IsEnabled(groupID) ?
            new ItemCount(consumablesConsumed, ConsumableGroup<TConsumable>(groupID).ToItem(consumable).maxStack) <= player.GetInfinity(consumable, groupID).Value : player.HasInfinite(consumable, consumablesConsumed, VanillaConsumableTypes.Mixed.ID);
    }


    private static int s_nextTypeID = 1;
    private static int s_nextGlobalID = -1;
    private static readonly Dictionary<int, IConsumableGroup> s_groups = new();

    private static bool UseCache(Player player) => player == Main.LocalPlayer;
    public static void ClearCache() {
        foreach ((int _, ConsumableCache cache) in s_caches) cache.ClearAll();
        s_usedTypes.Clear();
        s_usedAmmoTypes.Clear();
    }
    public static void ClearCache(Item item) {
        foreach ((int id, ConsumableCache cache) in s_caches) cache.ClearType(ConsumableGroup(id).CacheID(item));
        s_usedTypes.Remove(item.type);
        s_usedAmmoTypes.Clear();
    }

    private static readonly Dictionary<int, ConsumableCache> s_caches = new();

    private static readonly Dictionary<int, IReadOnlyList<IStandardGroup>> s_usedTypes = new();
    private static readonly Dictionary<int, IReadOnlyList<(IConsumableGroup, Item ammo)>> s_usedAmmoTypes = new();

    private static Configs.RequirementSettings Requirements => Configs.RequirementSettings.Instance;
    private static Configs.CategoryDetection CategoryDetection => Configs.CategoryDetection.Instance;

    public static Configs.ConsumableTypeDefinition ToDefinition(this IConsumableGroup type) => new(type.Mod, type.Name);
}

internal enum CacheType {
    Category,
    Requirement,
    Infinity
}

internal sealed class ConsumableCache {

    private readonly Dictionary<int, Category> _categories = new();
    private readonly Dictionary<int, IRequirement> _requirements = new();
    private readonly Dictionary<int, Infinity> _infinities = new();

    // TODO category and requirement overrides
    public T GetOrAdd<T>(CacheType type, int id, System.Func<T> getter){
        IDictionary cache = type switch {
            CacheType.Category => _categories,
            CacheType.Requirement => _requirements,
            CacheType.Infinity => _infinities,
            _ => throw new System.ArgumentException("Invalid CacheType", nameof(type))
        };
        if(Utility.TryGet(cache, id, out object value)) return (T)value;
        return (T)(cache[id] = getter());
    }

    public void ClearAll() {
        _categories.Clear();
        _requirements.Clear();
        _infinities.Clear();
    }
    public void ClearType(int type) {
        _categories.Remove(type);
        _requirements.Remove(type);
        _infinities.Remove(type);
    }
}
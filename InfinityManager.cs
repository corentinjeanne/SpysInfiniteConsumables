using System.Collections;
using System.Collections.Generic;
using Terraria;
using SPIC.ConsumableGroup;
using System.Diagnostics.CodeAnalysis;

namespace SPIC;

public enum FilterFlags {
    Default   = NonGlobal | Enabled,
    NonGlobal = 0b0001,
    Global    = 0b0010,
    Enabled   = 0b0100,
    Disabled  = 0b1000,
}


public static class InfinityManager {

    public static void Register<TImplementation>(ItemGroup<TImplementation> group) where TImplementation : ItemGroup<TImplementation> => Register(group, false);
    public static void RegisterAsGlobal<TImplementation, TConsumable>(ConsumableGroup<TImplementation, TConsumable> group) where TImplementation : ConsumableGroup<TImplementation, TConsumable> where TConsumable : notnull => Register(group, true);
    internal static void Register<TImplementation, TConsumable>(ConsumableGroup<TImplementation, TConsumable> group, bool global) where TImplementation : ConsumableGroup<TImplementation, TConsumable> where TConsumable : notnull {
        if (group.UID != 0) throw new System.ArgumentException("This type has already been registered", nameof(group));
        int id = group.UID = global ? s_nextGlobalID-- : s_nextTypeID++;
        
        s_groups[id] = group;
        s_caches[id] = new();
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        LCM = s_groups.Count * LCM / GCD(s_groups.Count, LCM);
    }

    public static IConsumableGroup ConsumableGroup(int id) => s_groups[id];
    public static IConsumableGroup<TConsumable> ConsumableGroup<TConsumable>(int id) where TConsumable : notnull => (IConsumableGroup<TConsumable>)ConsumableGroup(id);
    public static IConsumableGroup? ConsumableGroup(string mod, string Name) => s_groups.FindValue(kvp => kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);

    public static bool IsEnabled(IConsumableGroup group) => group is not IToggleable toogleable || toogleable.Enabled;
    public static bool IsEnabled(int groupID) => IsEnabled(ConsumableGroup(groupID));
    public static bool IsUsed(this Item item, int groupID) => item.UsedConsumableGroups().Find(t => t.UID == groupID) != null;

    public static IEnumerable<IConsumableGroup> ConsumableGroups(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) => ConsumableGroups<IConsumableGroup>(filters, noOrdering);
    public static IEnumerable<TConsumableGroup> ConsumableGroups<TConsumableGroup>(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) where TConsumableGroup: IConsumableGroup {
        bool enabled = filters.HasFlag(FilterFlags.Enabled);
        bool disabled = filters.HasFlag(FilterFlags.Disabled);

        bool MatchsFlags(IConsumableGroup group) => group is TConsumableGroup && ((enabled && disabled) || (enabled == IsEnabled(group)));

        if (filters.HasFlag(FilterFlags.NonGlobal)) {
            if (!noOrdering) {
                foreach (DictionaryEntry entry in Requirements.EnabledTypes) {
                    IConsumableGroup group = ((Config.ConsumableTypeDefinition)entry.Key).ConsumableType;
                    if (MatchsFlags(group)) yield return (TConsumableGroup)group;
                }
            } else {
                foreach ((int id, IConsumableGroup group) in s_groups) {
                    if (id > 0 && MatchsFlags(group)) yield return (TConsumableGroup)group;
                }
            }
        }
        if (filters.HasFlag(FilterFlags.Global)) {
            foreach ((int id, IConsumableGroup group) in s_groups) {
                if (id < 0 && MatchsFlags(group)) yield return (TConsumableGroup)group;
            }
        }
    }

    public static IReadOnlyList<IStandardGroup<Item>> UsedConsumableGroups(this Item item){
        if(s_usedTypes.TryGetValue(item.type, out IReadOnlyList<IStandardGroup<Item>>? types)) return types;
        List<IStandardGroup<Item>> used = new();
        foreach (IStandardGroup<Item> consumableType in ConsumableGroups<IStandardGroup<Item>>()) {
            if(GetRequirement(item, consumableType.UID) is NoRequirement) continue;
            used.Add(consumableType);
            if (Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
        }
        return s_usedTypes[item.type] = used;
    }


    // TODO reduce boxing
    internal static Category GetCategory(object consumable, int groupID){
        ICategory group = (ICategory)ConsumableGroup(groupID);
        return s_caches[groupID].GetOrAdd(CacheType.Category, group.CacheID(consumable), () => HasCategoryOverride(consumable, groupID, out Category? c) ? c.Value :  group.GetCategory(consumable));
    }
    public static TCategory GetCategory<TCategory>(this Item item, int groupID) where TCategory : System.Enum => (TCategory)GetCategory(ConsumableGroup(groupID).ToConsumable(item), groupID);
    public static Category GetCategory<TConsumable>(TConsumable consumable, int groupID) where TConsumable : notnull => GetCategory((object)consumable, groupID);
    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, int groupID) where TConsumable : notnull where TCategory : System.Enum => (TCategory)GetCategory(consumable, groupID);
    
    private static bool HasCategoryOverride(object consumable, int groupID, [NotNullWhen(true), MaybeNullWhen(false)] out Category? category) {
        IConsumableGroup group = ConsumableGroup(groupID);
        category = null;
        return group is IDetectable && CategoryDetection.HasDetectedCategory(consumable, groupID, out category);
    }

    internal static IRequirement GetRequirement(object consumable, int groupID) {
        IConsumableGroup group = ConsumableGroup(groupID);
        return s_caches[groupID].GetOrAdd(CacheType.Requirement, group.CacheID(consumable), () => group.GetRequirement(consumable));
    }
    public static IRequirement GetRequirement(this Item item, int groupID) => GetRequirement(ConsumableGroup(groupID).ToConsumable(item), groupID);
    public static IRequirement GetRequirement<TConsumable>(TConsumable consumable, int groupID) where TConsumable : notnull => GetRequirement((object)consumable, groupID);

    // public static bool HasRequirementOverride(int cacheID, int consumableID, out IRequirement requirement) {
    //     requirement = new NoRequirement();
    //     return false;
    // }

    public static Infinity GetInfinity<TConsumable>(this Player player, TConsumable consumable, int groupID) where TConsumable : notnull {
        IConsumableGroup<TConsumable> group = (IConsumableGroup<TConsumable>)ConsumableGroup(groupID);
        Infinity InfGetter() => GetInfinity(consumable, group.CountConsumables(player, consumable), groupID);
        return UseCache(player) ? s_caches[groupID].GetOrAdd(CacheType.Infinity, group.CacheID(consumable), InfGetter) : InfGetter();
    }
    
    internal static Infinity GetInfinity(object consumable, long count, int groupID) {
        IRequirement req = GetRequirement(consumable, groupID);
        return req.Infinity(ConsumableGroup(groupID).LongToCount(consumable, count));
    }
    public static Infinity GetInfinity(this Item item, long count, int groupID) => GetInfinity(ConsumableGroup(groupID).ToConsumable(item), count, groupID);
    public static Infinity GetInfinity<TConsumable>(TConsumable consumable, long count, int groupID) where TConsumable : notnull => GetInfinity((object)consumable, count, groupID);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, int groupID) where TConsumable : notnull
        => IsEnabled(groupID) && ((groupID < 0 || (consumable as Item)!.IsUsed(groupID)) ? ConsumableGroup(groupID).LongToCount(consumable, consumed).CompareTo(player.GetInfinity(consumable, groupID).Value) <= 0 : player.HasInfinite(consumable, consumed, VanillaConsumableTypes.Mixed.ID ));
    
    public static Config.ConsumableTypeDefinition ToDefinition(this IConsumableGroup type) => new(type.Mod, type.Name);

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
        foreach ((int id, ConsumableCache cache) in s_caches) {
            IConsumableGroup group = ConsumableGroup(id);
            cache.ClearType(group.CacheID(group.ToConsumable(item)));
        }
        s_usedTypes.Remove(item.type);
        s_usedAmmoTypes.Clear();
    }

    internal static int LCM { get; private set; } = 1;

    private static readonly Dictionary<int, ConsumableCache> s_caches = new();
    
    private static readonly Dictionary<int, IReadOnlyList<IStandardGroup<Item>>> s_usedTypes = new();
    private static readonly Dictionary<int, IReadOnlyList<(IConsumableGroup, Item ammo)>> s_usedAmmoTypes = new();

    private static Config.RequirementSettings Requirements => Config.RequirementSettings.Instance;
    private static Config.CategoryDetection CategoryDetection => Config.CategoryDetection.Instance;

}

internal enum CacheType {
    Category,
    Requirement,
    Infinity
}

internal sealed class ConsumableCache {

    // ? reduce the amount of requirement instances
    private readonly Dictionary<int, Category> _categories = new();
    private readonly Dictionary<int, IRequirement> _requirements = new();
    private readonly Dictionary<int, Infinity> _infinities = new();

    // TODO >>> category and requirement overrides
    public T GetOrAdd<T>(CacheType type, int id, System.Func<T> getter) where T : notnull{
        IDictionary cache = type switch {
            CacheType.Category => _categories,
            CacheType.Requirement => _requirements,
            CacheType.Infinity => _infinities,
            _ => throw new System.ArgumentException("Invalid CacheType", nameof(type))
        };
        if(Utility.TryGet(cache, id, out object? value)) return (T)value!;
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
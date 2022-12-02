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
    private static void Register<TImplementation, TConsumable>(ConsumableGroup<TImplementation, TConsumable> group, bool global) where TImplementation : ConsumableGroup<TImplementation, TConsumable> where TConsumable : notnull {
        if (group.UID != 0) throw new System.ArgumentException("This group has already been registered", nameof(group));
        int id = group.UID = global ? s_nextGlobalID-- : s_nextTypeID++;
        
        s_groups[id] = group;
        s_caches[id] = new();
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        LCM = s_groups.Count * LCM / GCD(s_groups.Count, LCM);
    }


    // TODO >>> more generic
    public static IConsumableGroup ConsumableGroup(int id) => s_groups[id];
    public static IConsumableGroup<TConsumable> ConsumableGroup<TConsumable>(int id) where TConsumable : notnull => (IConsumableGroup<TConsumable>)ConsumableGroup(id);
    public static IConsumableGroup? ConsumableGroup(string mod, string Name) => s_groups.FindValue(kvp => kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);
    
    public static IEnumerable<IConsumableGroup> ConsumableGroups(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) => ConsumableGroups<IConsumableGroup>(filters, noOrdering);
    public static IEnumerable<TGroup> ConsumableGroups<TGroup>(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) where TGroup : IConsumableGroup {
        bool enabled = filters.HasFlag(FilterFlags.Enabled);
        bool disabled = filters.HasFlag(FilterFlags.Disabled);

        bool MatchsFlags(IConsumableGroup group) => group is TGroup && ((enabled && disabled) || (enabled == IsEnabled(group)));

        if (filters.HasFlag(FilterFlags.NonGlobal)) {
            if (!noOrdering) {
                foreach (DictionaryEntry entry in Requirements.EnabledGroups) {
                    IConsumableGroup group = ((Config.ConsumableGroupDefinition)entry.Key).ConsumableType;
                    if (MatchsFlags(group)) yield return (TGroup)group;
                }
            } else {
                foreach ((int id, IConsumableGroup group) in s_groups) {
                    if (id > 0 && MatchsFlags(group)) yield return (TGroup)group;
                }
            }
        }
        if (filters.HasFlag(FilterFlags.Global)) {
            foreach ((int id, IConsumableGroup group) in s_groups) {
                if (id < 0 && MatchsFlags(group)) yield return (TGroup)group;
            }
        }
    }


    public static bool IsEnabled(IConsumableGroup group) => group is not IToggleable toogleable || toogleable.Enabled;
    
    // ? use reference equality instead of id
    public static bool IsUsed(this Item item, IConsumableGroup<Item> group) => item.UsedConsumableGroups().Find(t => t.UID == group.UID) != null;
    public static IReadOnlyList<IStandardGroup<Item>> UsedConsumableGroups(this Item item){
        if(s_usedTypes.TryGetValue(item.type, out IReadOnlyList<IStandardGroup<Item>>? types)) return types;
        List<IStandardGroup<Item>> used = new();
        if (!IsBlacklisted(item)) {
            foreach (IStandardGroup<Item> group in ConsumableGroups<IStandardGroup<Item>>()) {
                if (GetRequirement(item, group) is NoRequirement) continue;
                used.Add(group);
                if (Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
            }
        }
        return s_usedTypes[item.type] = used;
    }

    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum
        => s_caches[group.UID].GetOrAddCategory<TCategory>(group.CacheID(consumable), () => HasCategoryOverride(consumable, out TCategory? c, group) ? c :  group.GetCategory(consumable));
    public static TCategory GetCategory<TCategory>(this Item item, ICategory<Item, TCategory> group) where TCategory : System.Enum => GetCategory<Item, TCategory>(item, group);
    
    private static bool HasCategoryOverride<TConsumable, TCategory>(TConsumable consumable, [NotNullWhen(true)] out TCategory? category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        category = default;
        return group is IDetectable && CategoryDetection.HasDetectedCategory(consumable, out category, group);
    }


    // TODO >>> reduce ICount boxing (and remove the function)
    internal static Requirement GetRequirement(object consumable, IConsumableGroup group)
        => s_caches[group.UID].GetOrAddRequirement(group.CacheID(consumable), () => group.GetRequirement(consumable));
    public static Requirement GetRequirement<TConsumable>(TConsumable consumable, IConsumableGroup<TConsumable> group) where TConsumable : notnull
        => s_caches[group.UID].GetOrAddRequirement(group.CacheID(consumable), () => group.GetRequirement(consumable));
    public static Requirement GetRequirement(this Item item, IConsumableGroup<Item> group) => GetRequirement<Item>(item, group);


    public static bool IsBlacklisted(Item item) => Requirements.BlackListedItems.Contains(new(item.type));
    // TODO >>> reduce TConsumable boxing (and remove the function)
    internal static bool IsBlacklisted(object consumable, IConsumableGroup group)
        => group.UID > 0 ? IsBlacklisted((consumable as Item)!) : Requirements.BlackListedConsumables[group.ToDefinition()].Contains(group.Key(consumable));
    public static bool IsBlacklisted<TConsumable>(TConsumable consumable, IConsumableGroup<TConsumable> group) where TConsumable : notnull
        => group.UID > 0 ? IsBlacklisted((consumable as Item)!) : Requirements.BlackListedConsumables[group.ToDefinition()].Contains(group.Key(consumable));


    public static Infinity GetInfinity<TConsumable>(this Player player, TConsumable consumable, IConsumableGroup<TConsumable> group) where TConsumable : notnull {
        Infinity InfGetter() => GetInfinity(consumable, group.CountConsumables(player, consumable), group);
        return UseCache(player) ? s_caches[group.UID].GetOrAddInfinity(group.CacheID(consumable), InfGetter) : InfGetter();
    }
    // TODO >>> reduce ICount boxing
    public static Infinity GetInfinity<TConsumable>(TConsumable consumable, long count, IConsumableGroup<TConsumable> group) where TConsumable : notnull {
        Requirement req = GetRequirement(consumable, group);
        return req.Infinity(group.LongToCount(consumable, count));
    }
    public static Infinity GetInfinity(this Item item, long count, IConsumableGroup<Item> group) => GetInfinity<Item>(item, count, group);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, IConsumableGroup<TConsumable> group) where TConsumable : notnull
        => !IsBlacklisted(consumable, group) && IsEnabled(group)
            && ((group.UID < 0 || (consumable as Item)!.IsUsed((IConsumableGroup<Item>)group)) ?
                group.LongToCount(consumable, consumed).CompareTo(player.GetInfinity(consumable, group).Value) <= 0 :
                player.HasInfinite((consumable as Item)!, consumed, VanillaGroups.Mixed.Instance
            ));
    
    public static Config.ConsumableGroupDefinition ToDefinition(this IConsumableGroup group) => new(group.Mod, group.Name);

    private static int s_nextTypeID = 1;
    private static int s_nextGlobalID = -1;
    private static readonly Dictionary<int, IConsumableGroup> s_groups = new();

    private static bool UseCache(Player player) => player == Main.LocalPlayer;
    public static void ClearCache<TConsumable>(TConsumable consumable, IConsumableGroup<TConsumable> group) where TConsumable: notnull {
        s_caches[group.UID].ClearType(group.CacheID(consumable));
        if(group.UID > 0 && consumable is Item item){
            s_usedTypes.Remove(item.type);
            s_usedAmmoTypes.Clear();
        }
    }
    public static void ClearCache(Item item) {
        foreach ((int id, ConsumableCache cache) in s_caches) {
            IConsumableGroup group = ConsumableGroup(id);
            cache.ClearType(group.CacheID(group.ToConsumable(item)));
        }
        s_usedTypes.Remove(item.type);
        s_usedAmmoTypes.Clear();
    }
    public static void ClearCache() {
        foreach ((int _, ConsumableCache cache) in s_caches) cache.ClearAll();
        s_usedTypes.Clear();
        s_usedAmmoTypes.Clear();
    }

    internal static int LCM { get; private set; } = 1;

    private static readonly Dictionary<int, ConsumableCache> s_caches = new();
    
    private static readonly Dictionary<int, IReadOnlyList<IStandardGroup<Item>>> s_usedTypes = new();
    private static readonly Dictionary<int, IReadOnlyList<(IConsumableGroup, Item ammo)>> s_usedAmmoTypes = new();

    private static Config.RequirementSettings Requirements => Config.RequirementSettings.Instance;
    private static Config.CategoryDetection CategoryDetection => Config.CategoryDetection.Instance;

}

internal sealed class ConsumableCache {

    // ? reduce the amount of requirement instances
    private readonly Dictionary<int, Category> _categories = new();
    private readonly Dictionary<int, Requirement> _requirements = new();
    private readonly Dictionary<int, Infinity> _infinities = new();

    public static T GetOrAdd<T>(Dictionary<int, T> cache, int id, System.Func<T> getter) where T : notnull => cache.TryGetValue(id, out T? value) ? value : (cache[id] = getter());
    public Category GetOrAddCategory(int id, System.Func<Category> getter) => GetOrAdd(_categories, id, getter);
    public TCategory GetOrAddCategory<TCategory>(int id, System.Func<Category> getter) where TCategory : System.Enum => (TCategory)GetOrAdd(_categories, id, getter);
    public Requirement GetOrAddRequirement(int id, System.Func<Requirement> getter) => GetOrAdd(_requirements, id, getter);
    public Infinity GetOrAddInfinity(int id, System.Func<Infinity> getter) => GetOrAdd(_infinities, id, getter);

    public void ClearType(int type) {
        _categories.Remove(type);
        _requirements.Remove(type);
        _infinities.Remove(type);
    }
    public void ClearAll() {
        _categories.Clear();
        _requirements.Clear();
        _infinities.Clear();
    }

}
using System.Collections;
using System.Collections.Generic;
using Terraria;
using SPIC.ConsumableGroup;
using System.Collections.ObjectModel;

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
    public static void RegisterAsGlobal<TImplementation, TConsumable, TCount>(ConsumableGroup<TImplementation, TConsumable, TCount> group) where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCount> where TConsumable : notnull where TCount : ICount<TCount> => Register(group, true);
    private static void Register<TImplementation, TConsumable, TCount>(ConsumableGroup<TImplementation, TConsumable, TCount> group, bool global) where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCount> where TConsumable : notnull where TCount : ICount<TCount> {
        if (group.UID != 0) throw new System.ArgumentException("This group has already been registered", nameof(group));
        int id = group.UID = global ? s_nextGlobalID-- : s_nextTypeID++;
        
        s_groups[id] = group;
        s_caches[id] = group.CreateCache();
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        LCM = s_groups.Count * LCM / GCD(s_groups.Count, LCM);
    }


    public static IConsumableGroup ConsumableGroup(int id) => s_groups[id];
    public static IConsumableGroup? ConsumableGroup(string fullName) => s_groups.FindValue(kvp => kvp.Value.ToString() == fullName);
    public static IConsumableGroup? ConsumableGroup(string mod, string Name) => s_groups.FindValue(kvp => kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);

    public static Config.ConsumableGroupDefinition ToDefinition(this IConsumableGroup group) => new(group.Mod, group.Name);


    public static IEnumerable<IConsumableGroup> ConsumableGroups(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) => ConsumableGroups<IConsumableGroup>(filters, noOrdering);
    public static IEnumerable<TGroup> ConsumableGroups<TGroup>(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) where TGroup : IConsumableGroup {
        bool enabled = filters.HasFlag(FilterFlags.Enabled);
        bool disabled = filters.HasFlag(FilterFlags.Disabled);

        bool MatchsFlags(IConsumableGroup group) => group is TGroup && ((enabled && disabled) || (enabled == group.IsEnabled()));

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


    public static bool IsEnabled(this IConsumableGroup group) => group is not IToggleable t || t.IsEnabled();
    public static bool IsEnabled(this IToggleable group) => group.UID > 0 ? (bool)Requirements.EnabledGroups[group.ToDefinition()]! : Requirements.EnabledGlobals[group.ToDefinition()];
    public static TSettings Settings<TSettings>(this IConfigurable<TSettings> group) => (TSettings)Requirements.Requirements[group.ToDefinition()];
    public static Microsoft.Xna.Framework.Color Color(this IColorable group) => Display.Colors[group.ToDefinition()];


    public static bool IsBlacklisted<TConsumable>(TConsumable consumable, IConsumableGroup<TConsumable> group) where TConsumable : notnull
        => group.UID > 0 ? Requirements.BlackListedItems.Contains(new((consumable as Item)!.type)) : Requirements.BlackListedConsumables[group.ToDefinition()].Contains(group.Key(consumable));
    public static bool IsBlacklisted<TConsumable>(this Item item, IConsumableGroup<TConsumable> group) where TConsumable : notnull => IsBlacklisted(group.ToConsumable(item), group);

    public static bool IsUsed<TConsumable, TCount>(TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount>
        => group.UID > 0 ? UsedConsumableGroups((consumable as Item)!).Contains((IStandardGroup<Item, ItemCount>)group) : !GetRequirement(consumable, group).IsNone;
    public static bool IsUsed<TConsumable, TCount>(this Item item, IConsumableGroup<Item, ItemCount> group) where TConsumable : notnull where TCount : ICount<TCount> => IsUsed(group.ToConsumable(item), group);
    public static ReadOnlyCollection<IStandardGroup<Item, ItemCount>> UsedConsumableGroups(Item item){
        if(s_usedGroups.TryGetValue(item.type, out ReadOnlyCollection<IStandardGroup<Item, ItemCount>>? groups)) return groups;
        
        List<IStandardGroup<Item, ItemCount>> used = new();
        foreach (IStandardGroup<Item, ItemCount> group in ConsumableGroups<IStandardGroup<Item, ItemCount>>()) {
            if(GetRequirement(item, group).IsNone) continue;
            used.Add(group);
            if (Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
        }
        
        return s_usedGroups[item.type] = used.AsReadOnly();
    }


    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum
        => ((ICategoryCache<TCategory>)s_caches[group.UID]).GetOrAddCategory(group.CacheID(consumable), () => group is IDetectable && CategoryDetection.HasDetectedCategory(consumable, out TCategory? category, group) ? category : group.GetCategory(consumable));
    public static TCategory GetCategory<TConsumable, TCategory>(this Item item, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum => GetCategory(group.ToConsumable(item), group);


    public static Requirement<TCount> GetRequirement<TConsumable, TCount>(TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount>
        => ((ICountCache<TCount>)s_caches[group.UID]).GetOrAddRequirement(group.ReqCacheID(consumable), () => group.GetRequirement(consumable));
    public static Requirement<TCount> GetRequirement<TConsumable, TCount>(this Item item, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount> => GetRequirement(group.ToConsumable(item), group);


    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(this Player player, TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount> {
        Infinity<TCount> InfGetter() => GetInfinity(consumable, group.CountConsumables(player, consumable), group);
        return UseCache(player) ? ((ICountCache<TCount>)s_caches[group.UID]).GetOrAddInfinity(group.CacheID(consumable), InfGetter) : InfGetter();
    }
    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(TConsumable consumable, long count, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount> {
        Requirement<TCount> req = GetRequirement(consumable, group);
        return req.Infinity(group.LongToCount(consumable, count));
    }
    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(this Item item, long count, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount> => GetInfinity(group.ToConsumable(item), count, group);

    public static bool HasInfinite<TConsumable, TCount>(this Player player, TConsumable consumable, long consumed, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : ICount<TCount>
        => !IsBlacklisted(consumable, group) && group.IsEnabled()
            && ((group.UID < 0 || IsUsed(consumable, group)) ?
                group.LongToCount(consumable, consumed).CompareTo(player.GetInfinity(consumable, group).Value) <= 0 :
                player.HasInfinite((consumable as Item)!, consumed, VanillaGroups.Mixed.Instance
            ));


    public static void ClearCache() {
        foreach ((int _, ICache cache) in s_caches) cache.Clear();
        s_usedGroups.Clear();
    }
    internal static void ClearCategoryCache<TConsumable, TCategory>(TConsumable consumable, ICategory<TConsumable, TCategory> group) where TConsumable: notnull where TCategory : System.Enum {
        ((ICategoryCache<TCategory>)s_caches[group.UID]).ClearCategory(group.CacheID(consumable));
        if(group.UID > 0) s_usedGroups.Remove(group.CacheID(consumable));
    }
    private static bool UseCache(Player player) => player == Main.LocalPlayer;


    private static int s_nextTypeID = 1;
    private static int s_nextGlobalID = -1;

    internal static int LCM { get; private set; } = 1;
    private static readonly Dictionary<int, IConsumableGroup> s_groups = new();

    private static readonly Dictionary<int, ICache> s_caches = new();
    private static readonly Dictionary<int, ReadOnlyCollection<IStandardGroup<Item, ItemCount>>> s_usedGroups = new();

    private static Config.RequirementSettings Requirements => Config.RequirementSettings.Instance;
    private static Config.InfinityDisplay Display => Config.InfinityDisplay.Instance;
    private static Config.CategoryDetection CategoryDetection => Config.CategoryDetection.Instance;
}
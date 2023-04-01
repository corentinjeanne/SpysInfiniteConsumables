using System.Collections;
using System.Collections.Generic;
using Terraria;
using SPIC.ConsumableGroup;
using System.Collections.ObjectModel;
using SPIC.Configs;

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
    public static void RegisterAsGlobal<TImplementation, TConsumable, TCount>(ConsumableGroup<TImplementation, TConsumable, TCount> group) where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCount> where TConsumable : notnull where TCount : struct, ICount<TCount> => Register(group, true);
    private static void Register<TImplementation, TConsumable, TCount>(ConsumableGroup<TImplementation, TConsumable, TCount> group, bool global) where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCount> where TConsumable : notnull where TCount : struct, ICount<TCount> {
        if (group.UID != 0) throw new System.ArgumentException("This group has already been registered", nameof(group));
        if(group is IStandardGroup<TConsumable, TCount> && group is IAmmunition<TConsumable> && group is not IStandardAmmunition<TConsumable>) throw new System.ArgumentException($"A Standard group implementing {nameof(IAmmunition<TConsumable>)} must implement {nameof(IStandardAmmunition<TConsumable>)}");
        int id = group.UID = global ? s_nextGlobalID-- : s_nextTypeID++;
        
        s_groups[id] = group;
        s_caches[id] = group.CreateCache();
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        GroupsLCM = s_groups.Count * GroupsLCM / GCD(s_groups.Count, GroupsLCM);
    }


    public static IConsumableGroup ConsumableGroup(int id) => s_groups[id];
    public static IConsumableGroup? ConsumableGroup(string fullName) => s_groups.FindValue(kvp => kvp.Value.ToString() == fullName);
    public static IConsumableGroup? ConsumableGroup(string mod, string intName) => s_groups.FindValue(kvp => kvp.Value.Mod.Name == mod && kvp.Value.InternalName == intName);

    public static Configs.ConsumableGroupDefinition ToDefinition(this IConsumableGroup group) => new(group.Mod, group.InternalName);


    public static IEnumerable<IConsumableGroup> ConsumableGroups(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) => ConsumableGroups<IConsumableGroup>(filters, noOrdering);
    public static IEnumerable<TGroup> ConsumableGroups<TGroup>(FilterFlags filters = FilterFlags.Default, bool noOrdering = false) where TGroup : IConsumableGroup {
        bool enabled = filters.HasFlag(FilterFlags.Enabled);
        bool disabled = filters.HasFlag(FilterFlags.Disabled);

        bool MatchsFlags(IConsumableGroup group) => group is TGroup && ((enabled && disabled) || (enabled == group.IsEnabled()));

        if (filters.HasFlag(FilterFlags.NonGlobal)) {
            if (!noOrdering) {
                foreach (DictionaryEntry entry in GroupSettings.EnabledGroups) {
                    IConsumableGroup group = ((Configs.ConsumableGroupDefinition)entry.Key).ConsumableGroup;
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
    public static bool IsEnabled(this IToggleable group) => group.UID > 0 ? (bool)GroupSettings.EnabledGroups[group.ToDefinition()]! : GroupSettings.EnabledGlobals[group.ToDefinition()];
    public static TSettings Settings<TSettings>(this IConfigurable<TSettings> group) => (TSettings)GroupSettings.Settings[group.ToDefinition()];
    public static Microsoft.Xna.Framework.Color Color(this IColorable group) => Display.Colors[group.ToDefinition()];

    public static bool IsUsed<TConsumable, TCount>(TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount>
        => !GroupSettings.IsBlacklisted(consumable, group) && (group.UID > 0 ? UsedConsumableGroups((consumable as Item)!, out _).Contains((IStandardGroup<Item, ItemCount>)group) : !GetRequirement(consumable, group).IsNone);
    public static bool IsUsed<TConsumable, TCount>(this Item item, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> => IsUsed(group.ToConsumable(item), group);
    public static ReadOnlyCollection<IStandardGroup<Item, ItemCount>> UsedConsumableGroups(Item item, out bool hasUnused){
        if (s_usedGroups.TryGetValue(item.type, out System.Tuple<ReadOnlyCollection<IStandardGroup<Item, ItemCount>>, bool>?value)) {
            hasUnused = value.Item2;
            return value.Item1;
        }
        hasUnused = false;
        List<IStandardGroup<Item, ItemCount>> used = new();
        foreach (IStandardGroup<Item, ItemCount> group in ConsumableGroups<IStandardGroup<Item, ItemCount>>()) {
            if(item.GetRequirement(group).IsNone) continue;
            if (GroupSettings.MaxConsumableTypes != 0 && used.Count >= GroupSettings.MaxConsumableTypes) {
                hasUnused = true;
                break;
            }
            used.Add(group);
        }
        s_usedGroups[item.type] = new(used.AsReadOnly(), hasUnused);
        return s_usedGroups[item.type].Item1;
    }


    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum
        => ((ICategoryCache<TCategory>)s_caches[group.UID]).GetOrAddCategory(group.CacheID(consumable), () => group is IDetectable && CategoryDetection.HasDetectedCategory(consumable, out TCategory? category, group) ? category : group.GetCategory(consumable));
    public static TCategory GetCategory<TConsumable, TCategory>(this Item item, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum => GetCategory(group.ToConsumable(item), group);



    public static Requirement<TCount> GetRequirement<TConsumable, TCount>(TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount>
        => ((ICountCache<TCount>)s_caches[group.UID]).GetOrAddRequirement(group.CacheID(consumable), () => {
            Requirement<TCount> req = group.GetRequirement(consumable);
            if (GroupSettings.HasCustomRequirement(consumable, out TCount? count, group)) req.Root = count.Value;
            return req;
        });
    public static Requirement<TCount> GetRequirement<TConsumable, TCount>(this Item item, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> => GetRequirement(group.ToConsumable(item), group);


    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(this Player player, TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> {
        Infinity<TCount> InfGetter() => GetInfinity(consumable, group.CountConsumables(player, consumable), group);
        return UseCache(player) ? ((ICountCache<TCount>)s_caches[group.UID]).GetOrAddInfinity(group.CacheID(consumable), InfGetter) : InfGetter();
    }
    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(this Item item, long count, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> => GetInfinity(group.ToConsumable(item), count, group);
    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(TConsumable consumable, long count, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> => GetInfinity(consumable, group.LongToCount(consumable, count), group);
    public static Infinity<TCount> GetInfinity<TConsumable, TCount>(TConsumable consumable, TCount count, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> => GetRequirement(consumable, group).Infinity(count);

    public static Globals.DisplayInfo<TCount> GetDisplayInfo<TConsumable, TCount>(this IConsumableGroup<TConsumable, TCount> group, Item item, bool isACopy, out TConsumable values) where TConsumable : notnull where TCount : struct, ICount<TCount> {
        Player player = Main.LocalPlayer;
        TConsumable consumable = group.ToConsumable(item);

        if (group is IAmmunition<TConsumable> iAmmo && iAmmo.HasAmmo(player, consumable, out values!)){
            if(group.UID > 0 && !IsUsed(values, group))
                group = (IConsumableGroup<TConsumable, TCount>)VanillaGroups.Mixed.Instance;
        }
        else values = consumable;
        
        Requirement<TCount> root = GetRequirement(values, group);
        Infinity<TCount> infinity;
        TCount consumableCount;

        System.Enum? category = null;

        if (group.GetType().ImplementsInterface(typeof(ICategory<,>), out _))
            category = InfinityManager.GetCategory(values, (dynamic)group);

        if (group.OwnsItem(player, item, isACopy)) {
            consumableCount = group.LongToCount(values, group.CountConsumables(player, values));
            infinity = GetInfinity(player, values, group);
        } else {
            consumableCount = group.LongToCount(values, 0).None;
            infinity = new(consumableCount, 0);
        }

        TCount next = infinity.CountsAsNone || infinity.Value.CompareTo(group.LongToCount(values, group.GetMaxInfinity(values))) < 0 ?
            root.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(category, infinity, next) & Configs.InfinityDisplay.Instance.DisplayFlags;
        return new(displayFlags, category, infinity, next, consumableCount);
    }

    public static bool HasInfinite<TConsumable, TCount>(this Player player, TConsumable consumable, long consumed, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> {
        if(!group.IsEnabled()) return false;
        if (IsUsed(consumable, group)) return group.LongToCount(consumable, consumed).CompareTo(player.GetInfinity(consumable, group).Value) <= 0;
        return group.UID > 0 && player.HasInfinite((consumable as Item)!, consumed, VanillaGroups.Mixed.Instance);
    }

    public static bool HasInfinite<TConsumable, TCount>(this Player player, TConsumable consumable, long consumed, System.Func<bool> retryIfNoneIncluded, params IConsumableGroup<TConsumable, TCount>[] groups) where TConsumable : notnull where TCount : struct, ICount<TCount> {
        foreach(IConsumableGroup<TConsumable, TCount> group in groups){
            if (!GetRequirement(consumable, group).IsNone) return player.HasInfinite(consumable, consumed, group);
        }
        if(retryIfNoneIncluded()) return player.HasInfinite(consumable, consumed, groups);
        return false;
    }
    public static bool HasInfinite<TConsumable, TCount>(this Player player, TConsumable consumable, long consumed, params IConsumableGroup<TConsumable, TCount>[] groups) where TConsumable : notnull where TCount : struct, ICount<TCount> => player.HasInfinite(consumable, consumed, () => false, groups);


    public static void ClearCache() {
        foreach ((int _, ICountCache cache) in s_caches) cache.ClearAll();
        s_usedGroups.Clear();
    }
    public static void ClearCache(Item item){
        ReadOnlyCollection<IStandardGroup<Item, ItemCount>> list = UsedConsumableGroups(item, out _);
        foreach (IConsumableGroup group in list) ClearConsumableCache(item, (dynamic)group);
        s_usedGroups.Remove(item.type);
    }
    public static void ClearConsumableCache<TConsumable>(Item item, IConsumableGroup<TConsumable> group) where TConsumable: notnull => ClearConsumableCache(group.ToConsumable(item), group);
    public static void ClearConsumableCache<TConsumable>(TConsumable consumable, IConsumableGroup<TConsumable> group) where TConsumable: notnull {
        int uid = group.CacheID(consumable), rid = group.CacheID(consumable);
        if(s_caches[group.UID] is ICategoryCache cat) cat.ClearCategory(uid);
        s_caches[group.UID].ClearRequirement(rid);
        s_caches[group.UID].ClearInfinity(uid);

        if(group.UID > 0) s_usedGroups.Remove(uid);
    }
    private static bool UseCache(Player player) => player == Main.LocalPlayer;


    public static IEnumerable<(IToggleable group, bool enabled, bool global)> LoadedToggleableGroups() {
        foreach (DictionaryEntry entry in GroupSettings.EnabledGroups) {
            ConsumableGroupDefinition def = (ConsumableGroupDefinition)entry.Key;
            if (!def.IsUnloaded) yield return ((IToggleable)def.ConsumableGroup, (bool)entry.Value!, false);
        }
        foreach ((ConsumableGroupDefinition def, bool state) in GroupSettings.EnabledGlobals) {
            if (!def.IsUnloaded) yield return ((IToggleable)def.ConsumableGroup, state, true);
        }
        
    }

    private static int s_nextTypeID = 1;
    private static int s_nextGlobalID = -1;

    internal static int GroupsLCM { get; private set; } = 1;
    private static readonly Dictionary<int, IConsumableGroup> s_groups = new();

    private static readonly Dictionary<int, ICountCache> s_caches = new();
    private static readonly Dictionary<int, System.Tuple<ReadOnlyCollection<IStandardGroup<Item, ItemCount>>, bool>> s_usedGroups = new();

    private static Configs.GroupSettings GroupSettings => Configs.GroupSettings.Instance;
    private static Configs.InfinityDisplay Display => Configs.InfinityDisplay.Instance;
    private static Configs.CategoryDetection CategoryDetection => Configs.CategoryDetection.Instance;
}
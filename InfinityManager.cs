using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;
using Terraria;

namespace SPIC;

public static class InfinityManager {

    public static TCategory GetCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : Enum => infinity.Group.GetCategory(consumable, infinity);
    public static Requirement GetRequirement<TGroup, TConsumable>(TConsumable consumable, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement;


    public static long GetInfinity<TGroup, TConsumable>(TConsumable consumable, long count, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement.Infinity(count);

    public static FullInfinity GetFullInfinity(this Player player, int type, IInfinity infinity)
        => infinity.Group.GetEffectiveInfinity(player, type, infinity);

    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity >= consumed;


    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool> retryIfNoneIncluded, params Infinity<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        foreach (Infinity<TGroup, TConsumable> infinity in infinities) {
            if (!infinity.Group.GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull => player.HasInfinite(consumable, consumed, () => false, infinities);


    public static ItemDisplay GetLocalItemDisplay(this Item item) {
        if (_displays.TryGetOrCache(item, out ItemDisplay? itemDisplay)) return itemDisplay;

        itemDisplay = new();
        foreach (IGroup group in Groups) {
            foreach ((IInfinity infinity, int type, bool used) in group.GetDisplayedInfinities(item)) {
                long consumed = infinity.GetConsumedFromContext(Main.LocalPlayer, item, out bool exclusive);
                if(!used && exclusive) itemDisplay.Add(infinity, type, consumed, exclusive);
                else if (used) itemDisplay.Add(infinity, type, consumed, exclusive);
            }
        }
        return itemDisplay;
    }

    public static void ClearInfinities() {
        foreach (IGroup group in s_groups) group.ClearInfinities();
        _displays.Clear();
        LogCacheStats();
    }
    public static void ClearInfinity(Item item) {
        foreach (IGroup group in s_groups) group.ClearInfinity(item);
        _displays.Clear(item);
    }

    internal static void Register<TGroup, TConsumable>(Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        Group<TGroup, TConsumable>? group = (Group<TGroup, TConsumable>?)s_groups.Find(mg => mg is TGroup);
        group?.Add(infinity);
        s_infinities.Add(infinity);
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / Utility.GCD(InfinitiesLCM, s_infinities.Count);
    }
    internal static void Register<TGroup, TConsumable>(Group<TGroup, TConsumable> group) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        s_groups.Add(group);
        GroupsLCM = s_groups.Count * GroupsLCM / Utility.GCD(GroupsLCM, s_groups.Count);
        foreach (IInfinity infinity in s_infinities) {
            if (infinity is Infinity<TGroup, TConsumable> inf) group.Add(inf);
        }
    }

    public static Wrapper<T> RegisterConfig<T>(IInfinity infinity) where T : new() {
        Wrapper<T> wrapper = new();
        s_configs[infinity] = wrapper;
        return wrapper;
    }

    public static IGroup? GetGroup(string mod, string name) => s_groups.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);


    public static void Unload() {
        s_groups.Clear();
        s_infinities.Clear();
        s_configs.Clear();
    }

    public static void SortInfinities() {
        foreach (IGroup group in s_groups) group.SortInfinities();
    }


    internal static void CacheTimer() {
        s_cacheTime--;
        if (s_cacheTime >= 0) return;
        LogCacheStats();
    }
    private static void LogCacheStats() {
        foreach(IGroup group in Groups) if(group is Infinities.Items) group.LogCacheStats();
        SpysInfiniteConsumables.Instance.Logger.Debug($"Diplay values:{_displays}");
        s_cacheTime = 120;
        _displays.ResetStats();
    }

    private static int s_cacheTime = 0;

    public static ReadOnlyCollection<IGroup> Groups => new(s_groups);
    public static ReadOnlyCollection<IInfinity> Infinities => new(s_infinities);
    public static ReadOnlyDictionary<IInfinity, IWrapper> Configs => new(s_configs);

    public static int GroupsLCM { get; private set; } = 1;
    public static int InfinitiesLCM { get; private set; } = 1;

    private static readonly List<IInfinity> s_infinities = new();
    private static readonly List<IGroup> s_groups = new();
    private static readonly Dictionary<IInfinity, IWrapper> s_configs = new();

    private static readonly Cache<Item, (int type, int stack, int prefix), ItemDisplay> _displays = new(item => (item.type, item.stack, item.prefix), GetLocalItemDisplay);
}
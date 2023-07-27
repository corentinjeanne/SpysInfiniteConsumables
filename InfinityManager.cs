using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;
using Terraria;

namespace SPIC;

public static class InfinityManager {

    public static TCategory GetCategory<TModConsumable, TConsumable, TCategory>(TConsumable consumable, ModGroup<TModConsumable, TConsumable, TCategory> group) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull where TCategory : Enum => group.ModConsumable.GetCategory(consumable, group);
    public static Requirement GetRequirement<TModConsumable, TConsumable>(TConsumable consumable, ModGroup<TModConsumable, TConsumable> group) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull
        => group.ModConsumable.GetEffectiveInfinity(Main.LocalPlayer, consumable, group).Requirement;


    public static long GetInfinity<TModConsumable, TConsumable>(TConsumable consumable, long count, ModGroup<TModConsumable, TConsumable> group) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull
        => group.ModConsumable.GetEffectiveInfinity(Main.LocalPlayer, consumable, group).Requirement.Infinity(count);

    public static FullInfinity GetFullInfinity(this Player player, int type, IModGroup group)
        => group.ModConsumable.GetEffectiveInfinity(player, type, group);

    public static bool HasInfinite<TModConsumable, TConsumable>(this Player player, TConsumable consumable, long consumed, ModGroup<TModConsumable, TConsumable> group) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull
        => group.ModConsumable.GetEffectiveInfinity(player, consumable, group).Infinity >= consumed;


    public static bool HasInfinite<TModConsumable, TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool> retryIfNoneIncluded, params ModGroup<TModConsumable, TConsumable>[] groups) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
        foreach (ModGroup<TModConsumable, TConsumable> group in groups) {
            if (!group.ModConsumable.GetRequirement(consumable, group).IsNone) return player.HasInfinite(consumable, consumed, group);
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, groups);
    }
    public static bool HasInfinite<TModConsumable, TConsumable>(this Player player, TConsumable consumable, long consumed, params ModGroup<TModConsumable, TConsumable>[] groups) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull => player.HasInfinite(consumable, consumed, () => false, groups);


    public static ItemDisplay GetLocalItemDisplay(this Item item) {
        if (_displays.TryGetOrCache(item, out ItemDisplay? itemDisplay)) return itemDisplay;

        itemDisplay = new();
        foreach (IModConsumable modConsumable in ModConsumables) {
            foreach ((IModGroup group, int type, bool used) in modConsumable.GetDisplayedGroups(item)) {
                long consumed = group.GetConsumedFromContext(Main.LocalPlayer, item, out bool exclusive);
                if(!used && exclusive) itemDisplay.AddGroup(group, type, consumed, exclusive);
                else if (used) itemDisplay.AddGroup(group, type, consumed, exclusive);
            }
        }
        return itemDisplay;
    }

    public static void ClearInfinities() {
        foreach (IModConsumable modConsumable in s_modConsumables) modConsumable.ClearInfinities();
        _displays.Clear();
        LogCacheStats();
    }
    public static void ClearInfinity(Item item) {
        foreach (IModConsumable modConsumable in s_modConsumables) modConsumable.ClearInfinity(item);
        _displays.Clear(item);
    }

    internal static void Register<TModConsumable, TConsumable>(ModGroup<TModConsumable, TConsumable> group) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
        ModConsumable<TModConsumable, TConsumable>? modConsumable = (ModConsumable<TModConsumable, TConsumable>?)s_modConsumables.Find(mg => mg is TModConsumable);
        modConsumable?.Add(group);
        s_groups.Add(group);
        GroupsLCM = s_groups.Count * GroupsLCM / Utility.GCD(GroupsLCM, s_groups.Count);
    }
    internal static void Register<TModConsumable, TConsumable>(ModConsumable<TModConsumable, TConsumable> modConsumable) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
        s_modConsumables.Add(modConsumable);
        ModConsumablesLCM = s_modConsumables.Count * ModConsumablesLCM / Utility.GCD(ModConsumablesLCM, s_modConsumables.Count);
        foreach (IModGroup group in s_groups) {
            if (group is ModGroup<TModConsumable, TConsumable> g) modConsumable.Add(g);
        }
    }

    public static Wrapper<T> RegisterConfig<T>(IModGroup group) where T : new() {
        Wrapper<T> wrapper = new();
        s_configs[group] = wrapper;
        return wrapper;
    }

    public static IModConsumable? GetModConsumable(string mod, string name) => s_modConsumables.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IModGroup? GetModGroup(string mod, string name) => s_groups.Find(g => g.Mod.Name == mod && g.Name == name);


    public static void Unload() {
        s_modConsumables.Clear();
        s_groups.Clear();
        s_configs.Clear();
    }

    public static void SortGroups() {
        foreach (IModConsumable modConsumable in s_modConsumables) modConsumable.SortGroups();
    }


    internal static void CacheTimer() {
        s_cacheTime--;
        if (s_cacheTime >= 0) return;
        LogCacheStats();
    }
    private static void LogCacheStats() {
        foreach(IModConsumable consumable in ModConsumables) if(consumable is Groups.Items) consumable.LogCacheStats();
        SpysInfiniteConsumables.Instance.Logger.Debug($"Diplay values:{_displays}");
        s_cacheTime = 120;
        _displays.ResetStats();
    }

    private static int s_cacheTime = 0;

    public static ReadOnlyCollection<IModConsumable> ModConsumables => new(s_modConsumables);
    public static ReadOnlyCollection<IModGroup> Groups => new(s_groups);
    public static ReadOnlyDictionary<IModGroup, IWrapper> Configs => new(s_configs);

    public static int ModConsumablesLCM { get; private set; } = 1;
    public static int GroupsLCM { get; private set; } = 1;

    private static readonly List<IModGroup> s_groups = new();
    private static readonly List<IModConsumable> s_modConsumables = new();
    private static readonly Dictionary<IModGroup, IWrapper> s_configs = new();

    private static readonly Cache<Item, (int type, int stack, int prefix), ItemDisplay> _displays = new(item => (item.type, item.stack, item.prefix), GetLocalItemDisplay);
}
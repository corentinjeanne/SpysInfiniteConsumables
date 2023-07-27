using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;
using Terraria;

namespace SPIC;

public static class InfinityManager {

    public static TCategory GetCategory<TMetaGroup, TConsumable, TCategory>(TConsumable consumable, ModGroup<TMetaGroup, TConsumable, TCategory> group) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> where TCategory : Enum => group.MetaGroup.GetCategory(consumable, group);
    public static Requirement GetRequirement<TMetaGroup, TConsumable>(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable>
        => group.MetaGroup.GetEffectiveInfinity(Main.LocalPlayer, consumable, group).Requirement;


    public static long GetInfinity<TMetaGroup, TConsumable>(TConsumable consumable, long count, ModGroup<TMetaGroup, TConsumable> group) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable>
        => group.MetaGroup.GetEffectiveInfinity(Main.LocalPlayer, consumable, group).Requirement.Infinity(count);

    public static FullInfinity GetFullInfinity(this Player player, int type, IModGroup group)
        => group.MetaGroup.GetEffectiveInfinity(player, type, group);

    public static bool HasInfinite<TMetaGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, ModGroup<TMetaGroup, TConsumable> group) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable>
        => group.MetaGroup.GetEffectiveInfinity(player, consumable, group).Infinity >= consumed;


    public static bool HasInfinite<TMetaGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool> retryIfNoneIncluded, params ModGroup<TMetaGroup, TConsumable>[] groups) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
        foreach (ModGroup<TMetaGroup, TConsumable> group in groups) {
            if (!group.MetaGroup.GetRequirement(consumable, group).IsNone) return player.HasInfinite(consumable, consumed, group);
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, groups);
    }
    public static bool HasInfinite<TMetaGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, params ModGroup<TMetaGroup, TConsumable>[] groups) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> => player.HasInfinite(consumable, consumed, () => false, groups);


    public static MetaDisplay GetLocalMetaDisplay(this Item item) {
        if (_displays.TryGetOrCache(item, out MetaDisplay? metaDisplay)) return metaDisplay;

        metaDisplay = new();
        foreach (IMetaGroup metaGroup in MetaGroups) {
            foreach ((IModGroup group, int type, bool used) in metaGroup.GetDisplayedGroups(item)) {
                long consumed = group.GetConsumedFromContext(Main.LocalPlayer, item, out bool exclusive);
                if(!used && exclusive) metaDisplay.AddGroup(group, type, consumed, exclusive);
                else if (used) metaDisplay.AddGroup(group, type, consumed, exclusive);
            }
        }
        return metaDisplay;
    }

    public static void ClearInfinities() {
        foreach (IMetaGroup metaGroup in s_metaGroups) metaGroup.ClearInfinities();
        _displays.Clear();
        LogCacheStats();
    }
    public static void ClearInfinity(Item item) {
        foreach (IMetaGroup metaGroup in s_metaGroups) metaGroup.ClearInfinity(item);
        _displays.Clear(item);
    }

    internal static void Register<TMetaGroup, TConsumable>(ModGroup<TMetaGroup, TConsumable> group) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
        MetaGroup<TMetaGroup, TConsumable>? metaGroup = (MetaGroup<TMetaGroup, TConsumable>?)s_metaGroups.Find(mg => mg is TMetaGroup);
        metaGroup?.Add(group);
        s_groups.Add(group);
        GroupsLCM = s_groups.Count * GroupsLCM / Utility.GCD(GroupsLCM, s_groups.Count);
    }
    internal static void Register<TMetaGroup, TConsumable>(MetaGroup<TMetaGroup, TConsumable> metaGroup) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
        s_metaGroups.Add(metaGroup);
        MetaGroupsLCM = s_metaGroups.Count * MetaGroupsLCM / Utility.GCD(MetaGroupsLCM, s_metaGroups.Count);
        foreach (IModGroup group in s_groups) {
            if (group is ModGroup<TMetaGroup, TConsumable> g) metaGroup.Add(g);
        }
    }

    public static Wrapper<T> RegisterConfig<T>(IModGroup group) where T : new() {
        Wrapper<T> wrapper = new();
        s_configs[group] = wrapper;
        return wrapper;
    }

    public static IMetaGroup? GetMetaGroup(string mod, string name) => s_metaGroups.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IModGroup? GetModGroup(string mod, string name) => s_groups.Find(g => g.Mod.Name == mod && g.Name == name);


    public static void Unload() {
        s_metaGroups.Clear();
        s_groups.Clear();
        s_configs.Clear();
    }

    public static void SortGroups() {
        foreach (IMetaGroup metaGroup in s_metaGroups) metaGroup.SortGroups();
    }


    internal static void CacheTimer() {
        s_cacheTime--;
        if (s_cacheTime >= 0) return;
        LogCacheStats();
    }
    private static void LogCacheStats() {
        foreach(IMetaGroup meta in MetaGroups) if(meta is Groups.ItemMG) meta.LogCacheStats();
        SpysInfiniteConsumables.Instance.Logger.Debug($"Diplay values:{_displays}");
        s_cacheTime = 120;
        _displays.ResetStats();
    }

    private static int s_cacheTime = 0;

    public static ReadOnlyCollection<IMetaGroup> MetaGroups => new(s_metaGroups);
    public static ReadOnlyCollection<IModGroup> Groups => new(s_groups);
    public static ReadOnlyDictionary<IModGroup, IWrapper> Configs => new(s_configs);

    public static int MetaGroupsLCM { get; private set; } = 1;
    public static int GroupsLCM { get; private set; } = 1;

    private static readonly List<IModGroup> s_groups = new();
    private static readonly List<IMetaGroup> s_metaGroups = new();
    private static readonly Dictionary<IModGroup, IWrapper> s_configs = new();

    private static readonly Cache<Item, (int type, int stack, int prefix), MetaDisplay> _displays = new(item => (item.type, item.stack, item.prefix), GetLocalMetaDisplay);
}
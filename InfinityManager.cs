using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;

namespace SPIC;

// TODO lag when picking up loads of items (1k+) with right click

public static class InfinityManager {

    public static TCategory GetCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum
        => (TCategory?)infinity.Group.GetFullInfinity(Main.LocalPlayer, consumable, infinity).Extras.Find(i => i is TCategory) ?? default;
    public static TCategory GetCategory<TGroup, TConsumable, TCategory>(int consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum
        => (TCategory?)infinity.Group.GetFullInfinity(Main.LocalPlayer, consumable, infinity).Extras.Find(i => i is TCategory) ?? default;

    public static long GetInfinity<TGroup, TConsumable>(this Player player, TConsumable consumable, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity;
    public static long GetInfinity<TGroup, TConsumable>(this Player player, int consumable, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity;

    public static long GetInfinity<TGroup, TConsumable>(TConsumable consumable, long count, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement.Infinity(count);
    public static long GetInfinity<TGroup, TConsumable>(int consumable, long count, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement.Infinity(count);

    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity >= consumed;
    public static bool HasInfinite<TGroup, TConsumable>(this Player player, int consumable, long consumed, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity >= consumed;
    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, System.Func<bool> retryIfNoneIncluded, params Infinity<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        foreach (Infinity<TGroup, TConsumable> infinity in infinities) {
            if (!infinity.Group.GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull => player.HasInfinite(consumable, consumed, () => false, infinities);


    public static ItemDisplay GetLocalItemDisplay(this Item item) {
        if (s_displays.TryGetOrCache(item, out ItemDisplay? itemDisplay)) return itemDisplay;

        itemDisplay = new();
        foreach (IGroup group in Groups) {
            foreach ((IInfinity infinity, FullInfinity display, InfinityVisibility visibility) in group.GetDisplayedInfinities(Main.LocalPlayer, item)) itemDisplay.Add(infinity, display, visibility);
        }
        
        return itemDisplay;
    }

    public static void ClearInfinities() {
        foreach (IGroup group in s_groups) group.ClearInfinities();
        s_displays.Clear();
        LogCacheStats();
    }
    public static void ClearInfinity(Item item) {
        foreach (IGroup group in s_groups) group.ClearInfinity(item);
        s_displays.Clear(item);
    }

    internal static void Register<TGroup, TConsumable>(Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        Group<TGroup, TConsumable>? group = (Group<TGroup, TConsumable>?)s_groups.Find(mg => mg is TGroup);
        group?.Add(infinity);
        s_infinities.Add(infinity);
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / Utility.GCD(InfinitiesLCM, s_infinities.Count);
    }
    internal static void Register<TGroup, TConsumable>(Group<TGroup, TConsumable> group) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        if(group is Infinities.Items) s_groups.Insert(0, group);
        else s_groups.Add(group);
        GroupsLCM = s_groups.Count * GroupsLCM / Utility.GCD(GroupsLCM, s_groups.Count);
        foreach (IInfinity infinity in s_infinities) {
            if (infinity is Infinity<TGroup, TConsumable> inf) group.Add(inf);
        }
    }

    public static IGroup? GetGroup(string mod, string name) => s_groups.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);

    public static bool SaveDetectedCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, TCategory category, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum {
        Terraria.ModLoader.Config.ItemDefinition def = new(infinity.Group.ToItem(consumable).type);
        if(infinity.Group.Config.Customs.ContainsKey(def)) return false;
        Configs.Custom custom = new() {
            Choice = nameof(Configs.Custom.Individual),
            Individual = new()
        };
        custom.Individual.Add(new(infinity), new Configs.Count<TCategory>(category));
        infinity.Group.Config.Customs[def] = custom;
        return true;
    }

    public static void Unload() {
        s_groups.Clear();
        s_infinities.Clear();
    }

    public static ReadOnlyCollection<IGroup> Groups => new(s_groups);
    public static ReadOnlyCollection<IInfinity> Infinities => new(s_infinities);

    public static int GroupsLCM { get; private set; } = 1;
    public static int InfinitiesLCM { get; private set; } = 1;


    internal static void CacheTimer() {
        s_cacheTime--;
        if (s_cacheTime >= 0) return;
        LogCacheStats();
    }
    private static void LogCacheStats() {
        foreach (IGroup group in Groups) if (group is Infinities.Items) group.LogCacheStats();
        SpysInfiniteConsumables.Instance.Logger.Debug($"Diplay values:{s_displays}");
        s_cacheTime = 120;
        s_displays.ResetStats();
    }
    private static int s_cacheTime = 0;

    private static readonly List<IGroup> s_groups = new();
    private static readonly List<IInfinity> s_infinities = new();

    private static readonly Cache<Item, (int type, int stack, int prefix), ItemDisplay> s_displays = new(item => (item.type, item.stack, item.prefix), GetLocalItemDisplay);
}
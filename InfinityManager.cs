using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;

namespace SPIC;

public static class InfinityManager {

    public delegate void ExclusiveDisplay(Item item, List<(IInfinity infinity, long consumed)> exclusiveGroups);

    public static event ExclusiveDisplay? ExclusiveDisplays;

    public static TCategory GetCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum => infinity.Group.GetFullInfinity(Main.LocalPlayer, consumable, infinity).Has(out TCategory category) ? category : default;

    public static Requirement GetRequirement<TGroup, TConsumable>(TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement;


    public static long GetInfinity<TGroup, TConsumable>(TConsumable consumable, long count, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement.Infinity(count);

    public static FullInfinity GetFullInfinity(this Player player, int type, IInfinity infinity)
        => infinity.Group.GetEffectiveInfinity(player, type, infinity);

    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity >= consumed;


    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, System.Func<bool> retryIfNoneIncluded, params InfinityRoot<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        foreach (InfinityRoot<TGroup, TConsumable> infinity in infinities) {
            if (!infinity.Group.GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, params InfinityRoot<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull => player.HasInfinite(consumable, consumed, () => false, infinities);


    public static ItemDisplay GetLocalItemDisplay(this Item item) {
        if (s_displays.TryGetOrCache(item, out ItemDisplay? itemDisplay)) return itemDisplay;

        List<(IInfinity infinity, long consumed)> exclusiveGroups = new();
        ExclusiveDisplays?.Invoke(item, exclusiveGroups);

        itemDisplay = new();
        if(exclusiveGroups.Count != 0) {
            foreach ((IInfinity infinity, long consumed) in exclusiveGroups) {
                (int type, bool displayed) = infinity.Group.GetDisplay(item, infinity);
                if(displayed) itemDisplay.Add(infinity, type, consumed);
            }
        }
        else  {
            foreach (IGroup group in Groups) {
                foreach ((IInfinity infinity, int type, bool used) in group.GetDisplayedInfinities(item)) {
                    long consumed = Main.LocalPlayer.IsFromVisibleInventory(item) ? 1 : 0;
                    if (used) itemDisplay.Add(infinity, type, consumed);
                }
            }
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

    internal static void Register<TGroup, TConsumable>(InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
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
            if (infinity is InfinityRoot<TGroup, TConsumable> inf) group.Add(inf);
        }
    }

    public static IGroup? GetGroup(string mod, string name) => s_groups.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);


    public static void Unload() {
        s_groups.Clear();
        s_infinities.Clear();
    }

    public static void UpdateInfinities() {
        foreach (IGroup group in s_groups) group.UpdateInfinities();
    }



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
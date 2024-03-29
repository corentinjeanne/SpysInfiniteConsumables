using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using SPIC.Configs.UI;
using SpikysLib;
using SpikysLib.DataStructures;
using Terraria;

namespace SPIC;

public static class InfinityManager {

    public static TCategory GetCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum
        => (TCategory?)infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Extras.Find(i => i is TCategory) ?? default;
    public static TCategory GetCategory<TGroup, TConsumable, TCategory>(int consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum
        => (TCategory?)infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Extras.Find(i => i is TCategory) ?? default;

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
            if (!infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Requirement.IsNone) return player.HasInfinite(consumable, consumed, infinity); // Change GetRequirement to used || unused
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TGroup, TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TGroup, TConsumable>[] infinities) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull => player.HasInfinite(consumable, consumed, () => false, infinities);


    public static ItemDisplay GetLocalItemDisplay(this Item item) => s_displays.GetOrAdd(item);
    private static ItemDisplay ComputeLocalItemDisplay(this Item item) {
        ItemDisplay itemDisplay = new();
        foreach (IGroup group in Groups) {
            foreach ((IInfinity infinity, int displayed, FullInfinity display, InfinityVisibility visibility) in group.GetDisplayedInfinities(Main.LocalPlayer, item))
                itemDisplay.Add(infinity, displayed, display, visibility);
        }
        return itemDisplay;
    }

    public static void ClearInfinities() {
        foreach (IGroup group in s_groups) group.ClearInfinities();
        if (s_cacheRefresh != 0) s_delayed = true;
        else {
            s_displays.Clear();
            s_cacheRefresh = Configs.InfinityDisplay.Instance.CacheRefreshDelay;
        }
    }

    public static void DecreaseCacheLock(){
        if (s_cacheRefresh > 0) s_cacheRefresh--;
        else {
            if (!s_delayed) return;
            ClearInfinities();
            s_delayed = false;
        }
    }

    internal static void Register<TGroup, TConsumable>(Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        Group<TGroup, TConsumable>? group = (Group<TGroup, TConsumable>?)s_groups.Find(mg => mg is TGroup);
        group?.Add(infinity);
        s_infinities.Add(infinity);
        s_defaultStates[infinity] = infinity.Enabled;
        s_defaultColors[infinity] = infinity.Color;
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / MathX.GCD(InfinitiesLCM, s_infinities.Count);
    }
    internal static void Register<TGroup, TConsumable>(Group<TGroup, TConsumable> group) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        if(group is Default.Infinities.Items) s_groups.Insert(0, group);
        else s_groups.Add(group);
        GroupsLCM = s_groups.Count * GroupsLCM / MathX.GCD(GroupsLCM, s_groups.Count);
        foreach (IInfinity infinity in s_infinities) {
            if (infinity is Infinity<TGroup, TConsumable> inf) group.Add(inf);
        }
    }

    public static IGroup? GetGroup(string mod, string name) => s_groups.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);

    public static bool DefaultState(this IInfinity infinity) => s_defaultStates[infinity];
    public static Color DefaultColor(this IInfinity infinity) => s_defaultColors[infinity];

    public static bool SaveDetectedCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, TCategory category, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum {
        if(!Configs.InfinitySettings.Instance.DetectMissingCategories) return false;
        Terraria.ModLoader.Config.ItemDefinition def = new(infinity.Group.ToItem(consumable).type);
        if(!infinity.Group.Config.Customs.TryGetValue(def, out Configs.Custom? custom)) custom = infinity.Group.Config.Customs[def] = new() { Choice = nameof(Configs.Custom.Individual), Individual = new() };
        InfinityDefinition infDef = new(infinity);
        if(custom.Choice == nameof(custom.Global) || custom.Individual.ContainsKey(infDef)) return false;
        
        custom.Individual[infDef] = new Configs.Count<TCategory>(category);
        ClearInfinities();
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

    internal static string CacheStats() {
        List<string> parts = new(){ $"Diplay: {s_displays.Stats()}" };
        foreach (IGroup group in Groups) parts.Add(group.CacheStats());
        s_displays.ClearStats();
        return string.Join('\n', parts);
    }

    private static readonly List<IGroup> s_groups = new();
    private static readonly List<IInfinity> s_infinities = new();
    private static readonly Dictionary<IInfinity, bool> s_defaultStates = new();
    private static readonly Dictionary<IInfinity, Color> s_defaultColors = new();
    
    private static int s_cacheRefresh = 0;
    private static bool s_delayed;
    private static readonly Cache<Item, (int type, int stack, int prefix), ItemDisplay> s_displays = new(item => (item.type, item.stack, item.prefix), ComputeLocalItemDisplay) {
        EstimateValueSize = (ItemDisplay value) => value.DisplayedInfinities.Length * FullInfinity.EstimatedSize
    };
}
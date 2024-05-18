using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using SPIC.Configs;
using SPIC.Configs.UI;
using SpikysLib;
using SpikysLib.DataStructures;
using SpikysLib.Extensions;
using Terraria;

namespace SPIC;

public static class InfinityManager {

    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, Infinity<TConsumable, TCategory> infinity) where TConsumable : notnull where TCategory : struct, System.Enum
        => (TCategory?)infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Extras.Find(i => i is TCategory) ?? default;
    public static TCategory GetCategory<TConsumable, TCategory>(int consumable, Infinity<TConsumable, TCategory> infinity) where TConsumable : notnull where TCategory : struct, System.Enum
        => (TCategory?)infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Extras.Find(i => i is TCategory) ?? default;

    public static long GetInfinity<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity) where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity;
    public static long GetInfinity<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity) where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity;

    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity) where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement.Infinity(count);
    public static long GetInfinity<TConsumable>(int consumable, long count, Infinity<TConsumable> infinity) where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(Main.LocalPlayer, consumable, infinity).Requirement.Infinity(count);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity) where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Infinity<TConsumable> infinity) where TConsumable : notnull
        => infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Infinity >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, System.Func<bool> retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) where TConsumable : notnull {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (!infinity.Group.GetEffectiveInfinity(player, consumable, infinity).Requirement.IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        if (!retryIfNoneIncluded()) return false;
        return player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TConsumable>[] infinities) where TConsumable : notnull => player.HasInfinite(consumable, consumed, () => false, infinities);


    public static ItemDisplay GetLocalItemDisplay(this Item item) => InfinityDisplay.Instance.Cache != CacheStyle.None ? s_displays.GetOrAdd(item) : ComputeLocalItemDisplay(item);
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
            s_cacheRefresh = InfinityDisplay.Instance.CacheRefreshDelay;
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

    internal static void Register<TConsumable>(Infinity<TConsumable> infinity) where TConsumable : notnull {
        ModConfigExtensions.SetInstance(infinity);
        Group<TConsumable>? group = (Group<TConsumable>?)s_groups.Find(mg => mg == infinity.Group);
        group?.Add(infinity);
        s_infinities.Add(infinity);
        s_defaultEnabled[infinity] = infinity.Enabled;
        s_defaultColors[infinity] = infinity.Color;
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / MathX.GCD<int>(InfinitiesLCM, s_infinities.Count);
    }
    internal static void Register<TConsumable>(Group<TConsumable> group) where TConsumable : notnull {
        ModConfigExtensions.SetInstance(group);
        if(group is Default.Infinities.Items) s_groups.Insert(0, group);
        else s_groups.Add(group);
        GroupsLCM = s_groups.Count * GroupsLCM / MathX.GCD<int>(GroupsLCM, s_groups.Count);
        foreach (IInfinity infinity in s_infinities) {
            if (infinity.Group == group) group.Add((Infinity<TConsumable>)infinity);
        }
    }

    public static IGroup? GetGroup(string mod, string name) => s_groups.Find(mg => mg.Mod.Name == mod && mg.Name == name);
    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);

    public static bool DefaultEnabled(this IInfinity infinity) => s_defaultEnabled[infinity];
    public static Color DefaultColor(this IInfinity infinity) => s_defaultColors[infinity];

    public static bool SaveDetectedCategory<TConsumable, TCategory>(TConsumable consumable, TCategory category, Infinity<TConsumable, TCategory> infinity) where TConsumable : notnull where TCategory : struct, System.Enum {
        if(!InfinitySettings.Instance.DetectMissingCategories) return false;
        Terraria.ModLoader.Config.ItemDefinition def = new(infinity.Group.ToItem(consumable).type);
        if(!infinity.Group.Config.Customs.TryGetValue(def, out Custom? custom)) custom = infinity.Group.Config.Customs[def] = new() { Choice = nameof(Custom.Individual), Individual = new() };
        InfinityDefinition infDef = new(infinity);
        if(custom.Choice == nameof(custom.Global) || custom.Individual.ContainsKey(infDef)) return false;
        
        custom.Individual[infDef] = new Count<TCategory>(category);
        ClearInfinities();
        return true;
    }

    public static void Unload() {
        foreach (var g in s_groups) ModConfigExtensions.SetInstance(g, true);
        s_groups.Clear();
        foreach (var i in s_infinities) ModConfigExtensions.SetInstance(i, true);
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
    private static readonly Dictionary<IInfinity, bool> s_defaultEnabled = new();
    private static readonly Dictionary<IInfinity, Color> s_defaultColors = new();
    
    private static int s_cacheRefresh = 0;
    private static bool s_delayed;
    private static readonly Cache<Item, (int type, int stack, int prefix), ItemDisplay> s_displays = new(item => (item.type, item.stack, item.prefix), ComputeLocalItemDisplay) {
        EstimateValueSize = (ItemDisplay value) => value.DisplayedInfinities.Length * FullInfinity.EstimatedSize
    };
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SPIC.Configs;
using Terraria;

namespace SPIC;

// TODO cache
public static class InfinityManager {
    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => infinity.GetCategory(consumable);
    public static TCategory GetCategory<TConsumable, TCategory>(int consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => GetCategory(infinity.Consumable.ToConsumable(consumable), infinity);

    public static Requirement GetRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) {
        if (!infinity.Enabled) return default;
        if (!infinity.IsConsumable && !UsedInfinities(consumable, infinity.Consumable).Contains(infinity)) return GetRequirement(consumable, infinity.Consumable);
        return infinity.GetRequirement(consumable);
    }
    // => infinity.Enabled ? infinity.GetRequirement(consumable) : default;
    public static Requirement GetRequirement<TConsumable>(int consumable, Infinity<TConsumable> infinity)
        => infinity.Enabled ? GetRequirement(infinity.Consumable.ToConsumable(consumable), infinity) : default;

    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);
    public static long GetInfinity<TConsumable>(int consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);

    public static long CountConsumables<TConsumable>(this Player player, TConsumable consumable, ConsumableInfinity<TConsumable> infinity)
        => infinity.CountConsumables(player, consumable);
    public static long CountConsumables<TConsumable>(this Player player, int consumable, ConsumableInfinity<TConsumable> infinity)
        => infinity.CountConsumables(player, infinity.ToConsumable(consumable));

    public static HashSet<Infinity<TConsumable>> UsedInfinities<TConsumable>(TConsumable consumable, ConsumableInfinity<TConsumable> infinity)
        => infinity.UsedInfinities(consumable);
    public static HashSet<Infinity<TConsumable>> UsedInfinities<TConsumable>(int consumable, ConsumableInfinity<TConsumable> infinity)
        => UsedInfinities(infinity.ToConsumable(consumable), infinity);

    public static long GetInfinity<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity.Consumable), infinity);
    public static long GetInfinity<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity.Consumable), infinity);

    public static ReadOnlyCollection<ReadOnlyCollection<InfinityDisplay>> GetDisplayedInfinities(Item item) {
        List<ReadOnlyCollection<InfinityDisplay>> displays = [];
        InfinityVisibility minVisibility = InfinityVisibility.Visible;

        foreach (var infinity in s_consumableInfinities.Where(i => i.Enabled)) {
            List<InfinityDisplay> subDisplays = [];
            void AddInfinityDisplay(IInfinity infinity) {
                foreach ((var visibility, var value) in infinity.GetDisplayedInfinities(item)) {
                    if (visibility < minVisibility) continue;
                    if (visibility > minVisibility) {
                        displays.Clear();
                        subDisplays.Clear();
                        minVisibility = visibility;
                    }
                    subDisplays.Add(new(infinity, value));
                }
            }
            if (InfinityDisplays.Instance.displayedInfinities.HasFlag(DisplayedInfinities.Consumables)) AddInfinityDisplay(infinity);
            if (InfinityDisplays.Instance.displayedInfinities.HasFlag(DisplayedInfinities.Infinities)) foreach(IInfinity i in infinity.Infinities.Where(i => i.Enabled)) AddInfinityDisplay(i);
            displays.Add(subDisplays.AsReadOnly());
        }
        return displays.AsReadOnly();
    }

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity) => infinity.Enabled && player.GetInfinity(consumable, infinity) >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Infinity<TConsumable> infinity) => infinity.Enabled && player.GetInfinity(consumable, infinity) >= consumed;

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TConsumable>[] infinities) => player.HasInfinite(consumable, consumed, () => false, infinities);
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, params Infinity<TConsumable>[] infinities) => player.HasInfinite(consumable, consumed, () => false, infinities);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool> retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (!GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Func<bool> retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (!GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, infinities);
    }

    public static void ClearCache() { }

    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);

    internal static void Register<TConsumable>(Infinity<TConsumable> infinity) {
        s_infinities.Add(infinity);
        if(infinity is ConsumableInfinity<TConsumable> consumable) s_consumableInfinities.Add(consumable);
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / SpikysLib.MathHelper.GCD(InfinitiesLCM, s_infinities.Count);
    }
    public static void Unload() {
        s_infinities.Clear();
        s_consumableInfinities.Clear();
    }

    public static ReadOnlyCollection<IInfinity> Infinities => new(s_infinities);
    public static ReadOnlyCollection<IConsumableInfinity> ConsumableInfinities => new(s_consumableInfinities);

    public static int InfinitiesLCM { get; private set; } = 1;

    private static readonly List<IInfinity> s_infinities = [];
    private static readonly List<IConsumableInfinity> s_consumableInfinities = [];
}

public enum InfinityVisibility { Hidden, Visible, Exclusive }

public readonly record struct InfinityValue(int Consumable, long Count, Requirement Requirement, long Value);
public readonly record struct InfinityDisplay(IInfinity Infinity, InfinityValue Value);

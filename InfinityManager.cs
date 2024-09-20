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
    public static TCategory GetCategory<TCategory>(int consumable, IInfinityBridge<TCategory> infinity) where TCategory : struct, Enum => infinity.GetCategory(consumable);

    public static long GetRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity)
        => !infinity.Enabled ? 0 : GetUsedInfinity(consumable, infinity).GetRequirement(consumable);
    public static long GetRequirement(int consumable, IInfinityBridge infinity) => infinity.GetRequirement(consumable);

    public static long CountConsumables<TConsumable>(this Player player, TConsumable consumable, ConsumableInfinity<TConsumable> infinity)
        => infinity.CountConsumables(player, consumable);
    public static long CountConsumables(this Player player, int consumable, IConsumableBridge infinity) => infinity.CountConsumables(player, consumable);

    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity)
        => !infinity.Enabled ? 0 : GetUsedInfinity(consumable, infinity).GetInfinity(consumable, count);
    public static long GetInfinity(int consumable, long count, IInfinityBridge infinity) => infinity.GetInfinity(consumable, count);
    
    public static IReadOnlySet<Infinity<TConsumable>> UsedInfinities<TConsumable>(TConsumable consumable, ConsumableInfinity<TConsumable> infinity)
        => infinity.UsedInfinities(consumable);
    public static bool IsUsed<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => infinity.IsConsumable || UsedInfinities(consumable, infinity.Consumable).Contains(infinity);
    public static Infinity<TConsumable> GetUsedInfinity<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => IsUsed(consumable, infinity) ? infinity : infinity.Consumable;

    public static ReadOnlyCollection<ReadOnlyCollection<InfinityDisplay>> GetDisplayedInfinities(Item item) {
        bool visible = Utility.IsVisibleInInventory(item);

        List<ReadOnlyCollection<InfinityDisplay>> displays = [];
        InfinityVisibility minVisibility = InfinityVisibility.Visible;
        foreach (var infinity in InfinityLoader.ConsumableInfinities.Where(i => i.Enabled)) {
            List<InfinityDisplay> subDisplays = [];
            void AddInfinityDisplay(IInfinity infinity) {
                foreach ((var visibility, var value) in infinity.GetDisplayedInfinities(item)) {
                    if (visibility < minVisibility) continue;
                    if (InfinityDisplays.Instance.alternateDisplays && visibility > minVisibility) {
                        displays.Clear();
                        subDisplays.Clear();
                        minVisibility = visibility;
                    }
                    subDisplays.Add(new(
                        infinity,
                        visible ? value : new(value.Consumable, value.Requirement, 0, 0)
                    ));
                }
            }
            if (infinity.DisplayedInfinities.HasFlag(DisplayedInfinities.Consumable)) AddInfinityDisplay(infinity);
            if (infinity.DisplayedInfinities.HasFlag(DisplayedInfinities.Infinities)) foreach (IInfinity i in infinity.Infinities.Where(i => i.Enabled)) AddInfinityDisplay(i);
            if (subDisplays.Count > 0) displays.Add(subDisplays.AsReadOnly());
        }
        return displays.AsReadOnly();
    }

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity)
        => infinity.Enabled && GetInfinity(consumable, player.CountConsumables(consumable, infinity.Consumable), infinity) >= consumed;
    public static bool HasInfinite(this Player player, int consumable, long consumed, IInfinityBridge infinity)
        => infinity.Enabled && GetInfinity(consumable, player.CountConsumables(consumable, infinity.Consumable), infinity) >= consumed;

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TConsumable>[] infinities) => player.HasInfinite(consumable, consumed, null, infinities);
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool>? retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (infinity.Enabled && GetRequirement(consumable, infinity) > 0) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded is not null && retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, null, infinities);
    }
    public static bool HasInfinite(this Player player, int consumable, long consumed, params IInfinityBridge[] infinities) => player.HasInfinite(consumable, consumed, null, infinities);
    public static bool HasInfinite(this Player player, int consumable, long consumed, Func<bool>? retryIfNoneIncluded, params IInfinityBridge[] infinities) {
        foreach (IInfinityBridge infinity in infinities) {
            if (infinity.Enabled && GetRequirement(consumable, infinity) > 0) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded is not null && retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, infinities);
    }

    public static void ClearCache() { }

}

public enum InfinityVisibility { Hidden, Visible, Exclusive }

public readonly record struct InfinityValue(int Consumable, long Requirement, long Count, long Infinity) {
    public InfinityValue For(long requirement) => For(requirement, Count);
    public InfinityValue For(long requirement, long count) => new(Consumable, requirement, count, count >= requirement ? count : 0);
}
public readonly record struct InfinityDisplay(IInfinity Infinity, InfinityValue Value);

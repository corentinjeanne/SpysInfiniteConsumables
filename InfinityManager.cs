using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SPIC.Configs;
using SpikysLib.Collections;
using Terraria;
using Terraria.Localization;

namespace SPIC;

public static class InfinityManager {
    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => (TCategory)s_categories.GetOrCache((infinity, infinity.Consumable.GetId(consumable)), _ => infinity.GetCategory(consumable));
    public static TCategory GetCategory<TCategory>(int consumable, IInfinity<TCategory> infinity) where TCategory : struct, Enum
        => s_categories.TryGetValue((infinity, consumable), out var category) ? (TCategory)category : infinity.GetCategory(consumable);

    public static long GetRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) {
        if (!infinity.Enabled) return 0;
        infinity = GetUsedInfinity(consumable, infinity);
        return s_requirements.GetOrCache((infinity, infinity.Consumable.GetId(consumable)), _ => infinity.GetRequirement(consumable));
    }
    public static long GetRequirement(int consumable, IInfinity infinity)
        => s_requirements.TryGetValue((infinity, consumable), out var requirement) ? requirement : infinity.GetRequirement(consumable);

    public static long CountConsumables<TConsumable>(this Player player, TConsumable consumable, ConsumableInfinity<TConsumable> infinity)
        => s_counts.GetOrCache((infinity, player.whoAmI, infinity.GetId(consumable)), _ => infinity.CountConsumables(player, consumable));
    public static long CountConsumables(this Player player, int consumable, IConsumableInfinity infinity)
        => s_counts.TryGetValue((infinity, player.whoAmI, consumable), out var count) ? count : infinity.CountConsumables(player, consumable);

    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity) {
        if (!infinity.Enabled) return 0;
        infinity = GetUsedInfinity(consumable, infinity);
        return s_infinities.GetOrCache((infinity, infinity.Consumable.GetId(consumable), count), _ => infinity.GetInfinity(consumable, count));
    }
    public static long GetInfinity(int consumable, long count, IInfinity infinity)
        => s_infinities.TryGetValue((infinity, consumable, count), out var value) ? value : infinity.GetInfinity(consumable, count);
    
    public static IReadOnlySet<Infinity<TConsumable>> UsedInfinities<TConsumable>(TConsumable consumable, ConsumableInfinity<TConsumable> infinity)
        => infinity.UsedInfinities(consumable);
    public static bool IsUsed<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => infinity.IsConsumable || UsedInfinities(consumable, infinity.Consumable).Contains(infinity);
    public static Infinity<TConsumable> GetUsedInfinity<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => IsUsed(consumable, infinity) ? infinity : infinity.Consumable;

    public static ReadOnlyCollection<ReadOnlyCollection<InfinityDisplay>> GetDisplayedInfinities(Item item) {
        if (!InfinityDisplays.Instance.DisableCache && s_displays.TryGetValue((item.type, item.stack, item.prefix), out var ds)) return ds;
        bool visible = Utility.IsVisibleInInventory(item);

        List<ReadOnlyCollection<InfinityDisplay>> displays = [];
        InfinityVisibility minVisibility = InfinityVisibility.Visible;
        foreach (var infinity in InfinityLoader.ConsumableInfinities.Where(i => i.Enabled)) {
            List<InfinityDisplay> subDisplays = [];
            void AddInfinityDisplay(IInfinity infinity) {
                foreach ((var visibility, var value) in infinity.GetDisplayedInfinities(item, visible)) {
                    if (visibility < minVisibility) continue;
                    if (InfinityDisplays.Instance.alternateDisplays && visibility > minVisibility) {
                        displays.Clear();
                        subDisplays.Clear();
                        minVisibility = visibility;
                    }
                    subDisplays.Add(new(infinity, value));
                }
            }
            if (infinity.DisplayedInfinities.HasFlag(DisplayedInfinities.Consumable)) AddInfinityDisplay(infinity);
            if (infinity.DisplayedInfinities.HasFlag(DisplayedInfinities.Infinities)) foreach (IInfinity i in infinity.Infinities.Where(i => i.Enabled)) AddInfinityDisplay(i);
            if (subDisplays.Count > 0) displays.Add(subDisplays.AsReadOnly());
        }
        return s_displays[(item.type, item.stack, item.prefix)] = displays.AsReadOnly();
    }

    public static long GetInfinity<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity.Consumable), infinity);
    public static long GetInfinity(this Player player, int consumable, IInfinity infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity.Consumable), infinity);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity)
        => infinity.Enabled && player.GetInfinity(consumable, infinity) >= consumed;
    public static bool HasInfinite(this Player player, int consumable, long consumed, IInfinity infinity)
        => infinity.Enabled && player.GetInfinity(consumable, infinity) >= consumed;

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TConsumable>[] infinities) => player.HasInfinite(consumable, consumed, null, infinities);
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool>? retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (infinity.Enabled && GetRequirement(consumable, infinity) > 0) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded is not null && retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, null, infinities);
    }
    public static bool HasInfinite(this Player player, int consumable, long consumed, params IInfinity[] infinities) => player.HasInfinite(consumable, consumed, null, infinities);
    public static bool HasInfinite(this Player player, int consumable, long consumed, Func<bool>? retryIfNoneIncluded, params IInfinity[] infinities) {
        foreach (IInfinity infinity in infinities) {
            if (infinity.Enabled && GetRequirement(consumable, infinity) > 0) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded is not null && retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, infinities);
    }

    public static List<LocalizedText> GetDebugInfo(int consumable, IInfinity infinity) => s_debug.GetOrAdd((infinity, consumable), _ => []);
    public static void AddDebugInfo(int consumable, LocalizedText info, IInfinity infinity) => GetDebugInfo(consumable ,infinity).Add(info);

    public static void DecreaseCacheLock() {
        if (s_cacheRefresh > 0) s_cacheRefresh--;
        else if (s_delayed) ClearCache(false);
    }

    public static void ClearCache(bool canDelayClear = true) {
        s_categories.Clear();
        s_requirements.Clear();
        s_counts.Clear();
        s_infinities.Clear();
        s_debug.Clear();
        if (canDelayClear && s_cacheRefresh > 0) s_delayed = true;
        else {
            s_delayed = false;
            s_displays.Clear();
            s_cacheRefresh = InfinityDisplays.Instance.CacheRefreshDelay;
        }
    }

    private static TValue GetOrCache<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> cache) where TKey : notnull
        => !InfinityDisplays.Instance.DisableCache && dict.TryGetValue(key, out var value) ? value : dict[key] = cache(key);

    private static int s_cacheRefresh;
    private static bool s_delayed;

    private readonly static Dictionary<(IInfinity infinity, int consumable), Enum> s_categories = [];
    private readonly static Dictionary<(IInfinity infinity, int consumable), long> s_requirements = [];
    private readonly static Dictionary<(IInfinity infinity, int consumable), List<LocalizedText>> s_debug = [];
    private readonly static Dictionary<(IConsumableInfinity infinity, int player, int consumable), long> s_counts = [];
    private readonly static Dictionary<(IInfinity infinity, int consumable, long count), long> s_infinities = [];
    private readonly static Dictionary<(int type, int stack, int prefix), ReadOnlyCollection<ReadOnlyCollection<InfinityDisplay>>> s_displays = [];
}

public enum InfinityVisibility { Hidden, Visible, Exclusive }

public readonly record struct InfinityValue(int Consumable, long Requirement, long Count, long Infinity);
public readonly record struct InfinityDisplay(IInfinity Infinity, InfinityValue Value);

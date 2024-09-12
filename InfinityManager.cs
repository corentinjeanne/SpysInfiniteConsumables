using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SPIC.Components;
using SPIC.Configs;
using Terraria;

namespace SPIC;

public static class InfinityManager {
    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, Endpoints.ICategoryAccessor<TConsumable, TCategory> category) where TCategory : struct, Enum
        => Endpoints.GetCategory(category).GetValue(consumable);
    public static TCategory GetCategory<TConsumable, TCategory>(int consumable, Endpoints.ICategoryAccessor<TConsumable, TCategory> category) where TCategory : struct, Enum
        => Endpoints.GetCategory(category).TryGetValue(consumable, out TCategory c) ? c : GetCategory(ToConsumable(consumable, category.Infinity), category);

    public static Requirement GetRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity)
        => !infinity.IsEnabled() ? default : Endpoints.GetRequirement(infinity).GetValue(consumable);
    public static Requirement GetRequirement<TConsumable>(int consumable, Infinity<TConsumable> infinity) {
        if (!infinity.IsEnabled()) return default;
        if (Endpoints.GetRequirement(infinity).TryGetValue(consumable, out Requirement requirement)) return requirement;
        return GetRequirement(ToConsumable(consumable, infinity), infinity);
    }

    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);
    public static long GetInfinity<TConsumable>(int consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);

    public static long CountConsumables<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => Endpoints.CountConsumables(infinity.IdInfinity()).GetValue(new(player, consumable));
    public static long CountConsumables<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity)
        => Endpoints.CountConsumables(infinity.IdInfinity()).TryGetValue((player.whoAmI, consumable), out long count) ? count : player.CountConsumables(ToConsumable(consumable, infinity), infinity);

    public static HashSet<Infinity<TConsumable>> UsedInfinities<TConsumable>(TConsumable consumable, InfinityGroup<TConsumable> infinityGroup)
        => Endpoints.UsedInfinities(infinityGroup).GetValue(consumable);
    public static HashSet<Infinity<TConsumable>> UsedInfinities<TConsumable>(int consumable, InfinityGroup<TConsumable> infinityGroup)
        => Endpoints.UsedInfinities(infinityGroup).TryGetValue(consumable, out var usedInfinities) ? usedInfinities : UsedInfinities(ToConsumable(consumable, infinityGroup.Infinity), infinityGroup);

    public static long GetInfinity<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity) // ? Make it an endpoint
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity), infinity);
    public static long GetInfinity<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity), infinity);

    public static InfinityVisibility GetVisibility(Item item, IInfinity infinity) => Endpoints.GetVisibility(infinity).GetValue(item);

    public static ReadOnlyCollection<InfinityDisplay> GetDisplayedInfinities(Item item) {
        InfinityVisibility minVisibility = InfinityVisibility.Visible;
        
        void AddInfinityDisplay(List<InfinityDisplay> displays, IInfinity infinity) {
            InfinityVisibility visibility = GetVisibility(item, infinity);
            int comp = minVisibility.CompareTo(visibility);
            bool addedGroup = false;
            if(visibility >= minVisibility) {
                if (visibility > minVisibility) {
                    displays.Clear();
                    minVisibility = visibility;
                }
                foreach(var value in infinity.GetDisplayedInfinities(item)) {
                    List<InfinityDisplay> subDisplays = [];
                    displays.Add(new(infinity, value, subDisplays));
                    if (!addedGroup && infinity.TryGetComponent(out IInfinityGroup? infinityGroup)) {
                        foreach (var i in infinityGroup.Infinities) AddInfinityDisplay(subDisplays, i);
                        addedGroup = true;
                    }
                }
            }
        }

        List<InfinityDisplay> displays = [];
        foreach (var infinity in s_rootInfinities.Where(IsEnabled)) AddInfinityDisplay(displays, infinity);
        return displays.AsReadOnly();
    }

    public static List<TConsumable> GetDisplayedConsumables<TConsumable>(Item item, Infinity<TConsumable> infinity) {
        TConsumable consumable = ToConsumable(item, infinity);
        List<TConsumable> consumables = [consumable];
        Endpoints.ModifyDisplayedConsumables(infinity).ModifyValue(item, ref consumables);
        return consumables;
    }
    
    public static void ModifyDisplayedInfinity<TConsumable>(Item item, TConsumable consumable, ref InfinityValue value, Infinity<TConsumable> infinity)
        => Endpoints.ModifyDisplayedInfinity(infinity).ModifyValue(new(item, consumable), ref value);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity) => infinity.IsEnabled() && player.GetInfinity(consumable, infinity) >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Infinity<TConsumable> infinity) => infinity.IsEnabled() && player.GetInfinity(consumable, infinity) >= consumed;

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

    public static bool IsEnabled(this IInfinity infinity) {
        IInfinity idInfinity = IdInfinity(infinity);
        return InfinitySettings.IsEnabled(idInfinity) && (idInfinity != infinity || InfinitySettings.IsEnabled(infinity));
    }

    public static Infinity<TConsumable> IdInfinity<TConsumable>(this Infinity<TConsumable> infinity) => Endpoints.IdInfinity(infinity).GetValue(null);
    public static IInfinity IdInfinity(IInfinity infinity) => (IInfinity)Endpoints.IdInfinity(infinity)?.GetValue(null)! ?? infinity;
    public static TConsumable ToConsumable<TConsumable>(int consumable, Infinity<TConsumable> infinity) => Endpoints.ToConsumable(infinity.IdInfinity()).GetValue(consumable);
    public static TConsumable ToConsumable<TConsumable>(Item item, Infinity<TConsumable> infinity) => Endpoints.ItemToConsumable(infinity.IdInfinity()).GetValue(item);
    public static int GetId<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => Endpoints.GetId(infinity.IdInfinity()).GetValue(consumable);

    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);

    internal static void Register<TConsumable>(Infinity<TConsumable> infinity) {
        s_infinities.Add(infinity);
        s_rootInfinities.Add(infinity);
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / SpikysLib.MathHelper.GCD(InfinitiesLCM, s_infinities.Count);
    }
    internal static void UnregisterRootInfinity<TConsumable>(Infinity<TConsumable> infinity) {
        s_rootInfinities.Remove(infinity);
    }
    public static void Unload() {
        s_infinities.Clear();
        s_rootInfinities.Clear();
    }

    public static ReadOnlyCollection<IInfinity> Infinities => new(s_infinities);
    public static ReadOnlyCollection<IInfinity> RootInfinities => new(s_rootInfinities);

    public static int InfinitiesLCM { get; private set; } = 1;

    private static readonly List<IInfinity> s_infinities = [];
    private static readonly List<IInfinity> s_rootInfinities = [];
}

public enum InfinityVisibility { Hidden, Visible, Exclusive }

public readonly record struct ItemConsumable<TConsumable>(Item Item, TConsumable Consumable);
public readonly record struct PlayerConsumable<TConsumable>(Player Player, TConsumable Consumable);

public readonly record struct InfinityValue(int Consumable, long Count, Requirement Requirement, long Value);
public readonly record struct InfinityDisplay(IInfinity Infinity, InfinityValue Value, List<InfinityDisplay> SubDisplays);

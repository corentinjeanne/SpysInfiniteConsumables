using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;

namespace SPIC;

public static class InfinityManager {
    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => Endpoints.GetCategory(infinity).GetValue(consumable);
    public static TCategory GetCategory<TConsumable, TCategory>(int consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => Endpoints.GetCategory(infinity).TryGetValue(consumable, out TCategory category) ? category : GetCategory(ToConsumable(consumable, infinity), infinity);

    public static Requirement GetRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity)
        => Endpoints.GetRequirement(infinity).GetValue(consumable);
    public static Requirement GetRequirement<TConsumable>(int consumable, Infinity<TConsumable> infinity)
        => Endpoints.GetRequirement(infinity).TryGetValue(consumable, out Requirement requirement) ? requirement : GetRequirement(ToConsumable(consumable, infinity), infinity);
    
    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);
    public static long GetInfinity<TConsumable>(int consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);
    
    public static long CountConsumables<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => Endpoints.CountConsumables(infinity.GetIdGroup()).GetValue(new(player, consumable));
    public static long CountConsumables<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity) 
        => Endpoints.CountConsumables(infinity.GetIdGroup()).TryGetValue((player.whoAmI, consumable), out long count) ? count : player.CountConsumables(ToConsumable(consumable, infinity), infinity);

    public static long GetInfinity<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity), infinity);
    public static long GetInfinity<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity), infinity);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity) => player.GetInfinity(consumable, infinity) >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Infinity<TConsumable> infinity) => player.GetInfinity(consumable, infinity) >= consumed;
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

    public static Infinity<TConsumable> GetIdGroup<TConsumable>(this Infinity<TConsumable> infinity) => Endpoints.GetIdGroup(infinity).GetValue(null);
    public static TConsumable ToConsumable<TConsumable>(int consumable, Infinity<TConsumable> infinity) => Endpoints.ToConsumable(infinity.GetIdGroup()).GetValue(consumable);
    public static int GetId<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => Endpoints.GetId(infinity.GetIdGroup()).GetValue(consumable);

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

public readonly record struct PlayerConsumable<TConsumable>(Player Player, TConsumable Consumable);
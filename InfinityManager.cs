using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpikysLib.Collections;
using Terraria;

namespace SPIC;

public static class InfinityManager {
    public static long CountConsumables<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => infinity.GetIdGroup().CountConsumablesEndpoint().GetValue(new(player, consumable));

    public static TCategory GetCategory<TConsumable, TCategory>(TConsumable consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => infinity.GetCategoryEndpoint().GetValue(consumable);

    public static Requirement GetRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity)
        => infinity.GetRequirementEndpoint().GetValue(consumable);

    public static long GetInfinity<TConsumable>(TConsumable consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);
    public static long GetInfinity<TConsumable>(this Player player, TConsumable consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity), infinity);

    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Infinity<TConsumable> infinity)
        => player.GetInfinity(consumable, infinity) >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, Func<bool> retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (!GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TConsumable>(this Player player, TConsumable consumable, long consumed, params Infinity<TConsumable>[] infinities) => player.HasInfinite(consumable, consumed, () => false, infinities);
    
    public static long CountConsumables<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity) 
        => infinity.GetIdGroup().CountConsumablesEndpoint().TryGetValue((player.whoAmI, consumable), out long count) ? count : player.CountConsumables(ToConsumable(consumable, infinity), infinity);

    public static TCategory GetCategory<TConsumable, TCategory>(int consumable, Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => infinity.GetCategoryEndpoint().TryGetValue(consumable, out TCategory category) ? category : GetCategory(ToConsumable(consumable, infinity), infinity);

    public static Requirement GetRequirement<TConsumable>(int consumable, Infinity<TConsumable> infinity)
        => infinity.GetRequirementEndpoint().TryGetValue(consumable, out Requirement requirement) ? requirement : GetRequirement(ToConsumable(consumable, infinity), infinity);

    public static long GetInfinity<TConsumable>(int consumable, long count, Infinity<TConsumable> infinity)
        => GetRequirement(consumable, infinity).Infinity(count);
    public static long GetInfinity<TConsumable>(this Player player, int consumable, Infinity<TConsumable> infinity)
        => GetInfinity(consumable, player.CountConsumables(consumable, infinity), infinity);

    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Infinity<TConsumable> infinity)
        => player.GetInfinity(consumable, infinity) >= consumed;
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, Func<bool> retryIfNoneIncluded, params Infinity<TConsumable>[] infinities) {
        foreach (Infinity<TConsumable> infinity in infinities) {
            if (!GetRequirement(consumable, infinity).IsNone) return player.HasInfinite(consumable, consumed, infinity);
        }
        return retryIfNoneIncluded() && player.HasInfinite(consumable, consumed, infinities);
    }
    public static bool HasInfinite<TConsumable>(this Player player, int consumable, long consumed, params Infinity<TConsumable>[] infinities) => player.HasInfinite(consumable, consumed, () => false, infinities);

    public static int GetId<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) => infinity.GetIdGroup().GetIdEndpoint().GetValue(consumable);
    public static TConsumable ToConsumable<TConsumable>(int consumable, Infinity<TConsumable> infinity) => infinity.GetIdGroup().ToConsumableEndpoint().GetValue(consumable);

    public static Infinity<TConsumable> GetIdGroup<TConsumable>(this Infinity<TConsumable> infinity) => infinity.GetIdGroupEndpoint().GetValue(null);

    public static void ClearEndpoints() {
        foreach (IEndpoint endpoint in s_countConsumables.Values) endpoint.ClearCache();
        foreach (IEndpoint endpoint in s_categories.Values) endpoint.ClearCache();
        foreach (IEndpoint endpoint in s_requirements.Values) endpoint.ClearCache();
    }

    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);
    
    internal static void Register<TConsumable>(Infinity<TConsumable> infinity) {
        s_infinities.Add(infinity);
        s_rootInfinities.Add(infinity);
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / SpikysLib.MathHelper.GCD(InfinitiesLCM, s_infinities.Count);
    }
    public static void Unload() {
        ClearEndpoints();
        foreach (IEndpoint endpoint in s_idGroups.Values) endpoint.ClearCache();
        foreach (IEndpoint endpoint in s_getId.Values) endpoint.ClearCache();
        foreach (IEndpoint endpoint in s_toConsumable.Values) endpoint.ClearCache();

        s_infinities.Clear();
        s_rootInfinities.Clear();
        s_idGroups.Clear();
        s_getId.Clear();
        s_toConsumable.Clear();
        s_countConsumables.Clear();
        s_categories.Clear();
        s_requirements.Clear();
    } 

    public static SimpleEndpoint<object?, Infinity<TConsumable>> GetIdGroupEndpoint<TConsumable>(this Infinity<TConsumable> infinity)
        => s_idGroups.GetOrAddRaw(infinity, () => new SimpleEndpoint<object?, Infinity<TConsumable>>());
    
    public static SimpleEndpoint<TConsumable, int> GetIdEndpoint<TConsumable>(this Infinity<TConsumable> infinity)
        => s_getId.GetOrAddRaw(infinity, () => new SimpleEndpoint<TConsumable, int>());
    
    public static SimpleEndpoint<int, TConsumable> ToConsumableEndpoint<TConsumable>(this Infinity<TConsumable> infinity)
        => s_toConsumable.GetOrAddRaw(infinity, () => new SimpleEndpoint<int, TConsumable>());
    
    public static Endpoint<PlayerConsumable<TConsumable>, long, (int, int)> CountConsumablesEndpoint<TConsumable>(this Infinity<TConsumable> infinity)
        => s_countConsumables.GetOrAddRaw(infinity, () => new Endpoint<PlayerConsumable<TConsumable>, long, (int, int)>(args => (args.Player.whoAmI, GetId(args.Consumable, infinity))));
    
    public static Endpoint<TConsumable, Requirement, int> GetRequirementEndpoint<TConsumable>(this Infinity<TConsumable> infinity)
        => s_requirements.GetOrAddRaw(infinity, () => new Endpoint<TConsumable, Requirement, int>(c => GetId(c, infinity)));
    
    public static Endpoint<TConsumable, TCategory, int> GetCategoryEndpoint<TConsumable, TCategory>(this Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => s_categories.GetOrAddRaw(infinity, () => new Endpoint<TConsumable, TCategory, int>(c => GetId(c, infinity)));

    internal static void UnregisterRootInfinity<TConsumable>(Infinity<TConsumable> infinity) {
        s_rootInfinities.Remove(infinity);
    }

    public static ReadOnlyCollection<IInfinity> Infinities => new(s_infinities);
    public static ReadOnlyCollection<IInfinity> RootInfinities => new(s_rootInfinities);

    public static int InfinitiesLCM { get; private set; } = 1;

    private static readonly List<IInfinity> s_infinities = [];
    private static readonly List<IInfinity> s_rootInfinities = [];


    private static readonly Dictionary<IInfinity, IEndpoint> s_idGroups = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_getId = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_toConsumable = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_countConsumables = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_categories = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_requirements = [];
}

public readonly record struct PlayerConsumable<TConsumable>(Player Player, TConsumable Consumable);
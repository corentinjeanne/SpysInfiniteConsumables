using System;
using System.Collections.Generic;
using SPIC.Default.Components;
using SpikysLib.Collections;

namespace SPIC;

public static class Endpoints {
    public static SimpleEndpoint<object?, Infinity<TConsumable>> IdInfinity<TConsumable>(Infinity<TConsumable> infinity)
       => s_idInfinities.GetOrAddRaw(infinity, () => new SimpleEndpoint<object?, Infinity<TConsumable>>());

    public static SimpleEndpoint<TConsumable, int> GetId<TConsumable>(Infinity<TConsumable> infinity)
        => s_getIds.GetOrAddRaw(infinity, () => new SimpleEndpoint<TConsumable, int>());

    public static SimpleEndpoint<int, TConsumable> ToConsumable<TConsumable>(Infinity<TConsumable> infinity)
        => s_toConsumables.GetOrAddRaw(infinity, () => new SimpleEndpoint<int, TConsumable>());

    public static CachedEndpoint<PlayerConsumable<TConsumable>, long, (int, int)> CountConsumables<TConsumable>(Infinity<TConsumable> infinity)
        => s_countConsumables.GetOrAddRaw(infinity, () => new CachedEndpoint<PlayerConsumable<TConsumable>, long, (int, int)>(new SimpleEndpoint<PlayerConsumable<TConsumable>, long>(), args => (args.Player.whoAmI, InfinityManager.GetId(args.Consumable, infinity))));

    public static CachedEndpoint<TConsumable, Requirement, int> GetRequirement<TConsumable>(Infinity<TConsumable> infinity)
        => s_requirements.GetOrAddRaw(infinity, () => new CachedEndpoint<TConsumable, Requirement, int>(c => InfinityManager.GetId(c, infinity)));

    public static CachedEndpoint<TConsumable, TCategory, int> GetCategory<TConsumable, TCategory>(Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => s_categories.GetOrAddRaw(infinity, () => new CachedEndpoint<TConsumable, TCategory, int>(c => InfinityManager.GetId(c, infinity)));

    public static CachedEndpoint<TConsumable, HashSet<Infinity<TConsumable>>, int> UsedInfinities<TConsumable>(InfinityGroup<TConsumable> infinityGroup)
        => s_usedInfinities.GetOrAddRaw(infinityGroup, () => new CachedEndpoint<TConsumable, HashSet<Infinity<TConsumable>>, int>(new SimpleEndpoint<TConsumable, HashSet<Infinity<TConsumable>>>(), c => InfinityManager.GetId(c, infinityGroup)));

    public static void ClearCache() {
        foreach (var endpoint in s_countConsumables.Values) endpoint.Clear();
        foreach (var endpoint in s_categories.Values) endpoint.Clear();
        foreach (var endpoint in s_requirements.Values) endpoint.Clear();
        foreach (var endpoint in s_usedInfinities.Values) endpoint.Clear();
    }

    public static void Unload() {
        s_idInfinities.Clear();
        s_getIds.Clear();
        s_toConsumables.Clear();
        s_countConsumables.Clear();
        s_categories.Clear();
        s_requirements.Clear();
        s_usedInfinities.Clear();
    }

    private static readonly Dictionary<IInfinity, IEndpoint> s_idInfinities = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_getIds = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_toConsumables = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_countConsumables = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_categories = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_requirements = [];
    private static readonly Dictionary<IComponent, ICachedEndpoint> s_usedInfinities = [];
}
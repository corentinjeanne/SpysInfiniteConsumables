using System;
using System.Collections.Generic;
using SpikysLib.Collections;

namespace SPIC;

public static class Endpoints {
    public static SimpleEndpoint<object?, Infinity<TConsumable>> GetIdGroup<TConsumable>(Infinity<TConsumable> infinity)
       => s_idGroups.GetOrAddRaw(infinity, () => new SimpleEndpoint<object?, Infinity<TConsumable>>());

    public static SimpleEndpoint<TConsumable, int> GetId<TConsumable>(Infinity<TConsumable> infinity)
        => s_getId.GetOrAddRaw(infinity, () => new SimpleEndpoint<TConsumable, int>());

    public static SimpleEndpoint<int, TConsumable> ToConsumable<TConsumable>(Infinity<TConsumable> infinity)
        => s_toConsumable.GetOrAddRaw(infinity, () => new SimpleEndpoint<int, TConsumable>());

    public static CachedEndpoint<PlayerConsumable<TConsumable>, long, (int, int)> CountConsumables<TConsumable>(Infinity<TConsumable> infinity)
        => s_countConsumables.GetOrAddRaw(infinity, () => new CachedEndpoint<PlayerConsumable<TConsumable>, long, (int, int)>(args => (args.Player.whoAmI, InfinityManager.GetId(args.Consumable, infinity))));

    public static CachedEndpoint<TConsumable, Requirement, int> GetRequirement<TConsumable>(Infinity<TConsumable> infinity)
        => s_requirements.GetOrAddRaw(infinity, () => new CachedEndpoint<TConsumable, Requirement, int>(c => InfinityManager.GetId(c, infinity)));

    public static CachedEndpoint<TConsumable, TCategory, int> GetCategory<TConsumable, TCategory>(Infinity<TConsumable, TCategory> infinity) where TCategory : struct, Enum
        => s_categories.GetOrAddRaw(infinity, () => new CachedEndpoint<TConsumable, TCategory, int>(c => InfinityManager.GetId(c, infinity)));

    public static void ClearCache() {
        foreach (var endpoint in s_countConsumables.Values) endpoint.Clear();
        foreach (var endpoint in s_categories.Values) endpoint.Clear();
        foreach (var endpoint in s_requirements.Values) endpoint.Clear();
    }

    public static void Unload() {
        s_idGroups.Clear();
        s_getId.Clear();
        s_toConsumable.Clear();
        s_countConsumables.Clear();
        s_categories.Clear();
        s_requirements.Clear();
    }

    private static readonly Dictionary<IInfinity, IEndpoint> s_idGroups = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_getId = [];
    private static readonly Dictionary<IInfinity, IEndpoint> s_toConsumable = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_countConsumables = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_categories = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_requirements = [];
}
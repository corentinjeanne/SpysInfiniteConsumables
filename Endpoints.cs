using System;
using System.Collections.Generic;
using SPIC.Components;
using SpikysLib.Collections;
using Terraria;

namespace SPIC;

public static class Endpoints {
    public interface ICategoryAccessor<TConsumable, TCategory> where TCategory : struct, Enum {
        Infinity<TConsumable> Infinity { get; }
    }

    public static ProviderList<object?, Infinity<TConsumable>> IdInfinity<TConsumable>(Infinity<TConsumable> infinity)
       => s_idInfinities.GetOrAddRaw(infinity, () => new ProviderList<object?, Infinity<TConsumable>>(_ => infinity));
    public static IProvider? IdInfinity(IInfinity infinity) => s_idInfinities.GetValueOrDefault(infinity);

    public static ProviderList<TConsumable, int> GetId<TConsumable>(Infinity<TConsumable> infinity)
        => s_getIds.GetOrAddRaw(infinity, () => new ProviderList<TConsumable, int>());

    public static ProviderList<int, TConsumable> ToConsumable<TConsumable>(Infinity<TConsumable> infinity)
        => s_toConsumables.GetOrAddRaw(infinity, () => new ProviderList<int, TConsumable>());

    public static ProviderList<Item, TConsumable> ItemToConsumable<TConsumable>(Infinity<TConsumable> infinity)
        => s_itemToConsumables.GetOrAddRaw(infinity, () => new ProviderList<Item, TConsumable>());

    public static CachedEndpoint<PlayerConsumable<TConsumable>, long, (int, int)> CountConsumables<TConsumable>(Infinity<TConsumable> infinity)
        => s_countConsumables.GetOrAddRaw(infinity, () => new CachedEndpoint<PlayerConsumable<TConsumable>, long, (int, int)>(args => (args.Player.whoAmI, InfinityManager.GetId(args.Consumable, infinity))));

    public static CachedEndpoint<TConsumable, Requirement, int> GetRequirement<TConsumable>(Infinity<TConsumable> infinity)
        => s_requirements.GetOrAddRaw(infinity, () => new CachedEndpoint<TConsumable, Requirement, int>(c => InfinityManager.GetId(c, infinity)));

    public static CachedEndpoint<TConsumable, TCategory, int> GetCategory<TConsumable, TCategory>(ICategoryAccessor<TConsumable, TCategory> accessor) where TCategory : struct, Enum
        => s_categories.GetOrAddRaw(accessor.Infinity, () => new CachedEndpoint<TConsumable, TCategory, int>(c => InfinityManager.GetId(c, accessor.Infinity)));

    public static CachedEndpoint<TConsumable, HashSet<Infinity<TConsumable>>, int> UsedInfinities<TConsumable>(InfinityGroup<TConsumable> infinity)
        => s_usedInfinities.GetOrAddRaw(infinity.Infinity, () => new CachedEndpoint<TConsumable, HashSet<Infinity<TConsumable>>, int>(c => InfinityManager.GetId(c, infinity.Infinity)));

    public static ProviderList<Item, InfinityVisibility> GetVisibility(IInfinity infinity)
        => s_visibilities.GetOrAdd(infinity.Infinity, () => new(static _ => InfinityVisibility.Visible));

    public static ModifierList<ItemConsumable<TConsumable>, InfinityValue> ModifyDisplayedInfinity<TConsumable>(Infinity<TConsumable> infinity)
        => s_modifyDisplayedInfinities.GetOrAddRaw(infinity, () => new ModifierList<ItemConsumable<TConsumable>, InfinityValue>());

    public static ModifierList<Item, List<TConsumable>> ModifyDisplayedConsumables<TConsumable>(Infinity<TConsumable> infinity)
        => s_modifyDisplayedConsumables.GetOrAddRaw(infinity, () => new ModifierList<Item, List<TConsumable>>());

    public static void ClearCache() {
        foreach (var endpoint in s_countConsumables.Values) endpoint.Clear();
        foreach (var endpoint in s_requirements.Values) endpoint.Clear();
        foreach (var endpoint in s_categories.Values) endpoint.Clear();
        foreach (var endpoint in s_usedInfinities.Values) endpoint.Clear();
    }

    public static void Unload() {
        s_idInfinities.Clear();
        s_getIds.Clear();
        s_toConsumables.Clear();
        s_itemToConsumables.Clear();
        s_countConsumables.Clear();
        s_requirements.Clear();
        s_categories.Clear();
        s_usedInfinities.Clear();
        s_visibilities.Clear();
        s_modifyDisplayedInfinities.Clear();
        s_modifyDisplayedConsumables.Clear();
    }

    private static readonly Dictionary<IInfinity, IProvider> s_idInfinities = [];
    private static readonly Dictionary<IInfinity, IProvider> s_getIds = [];
    private static readonly Dictionary<IInfinity, IProvider> s_toConsumables = [];
    private static readonly Dictionary<IInfinity, IProvider> s_itemToConsumables = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_countConsumables = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_requirements = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_categories = [];
    private static readonly Dictionary<IInfinity, ICachedEndpoint> s_usedInfinities = [];
    private static readonly Dictionary<IInfinity, ProviderList<Item, InfinityVisibility>> s_visibilities = [];
    private static readonly Dictionary<IInfinity, IModifier> s_modifyDisplayedInfinities = [];
    private static readonly Dictionary<IInfinity, IModifier> s_modifyDisplayedConsumables = [];
}
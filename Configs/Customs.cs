using System;
using System.Collections.Generic;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public interface ICustoms<TConsumable> {
    long? GetRequirement(TConsumable consumable);
}

public sealed class Customs<TConsumable> : ICustoms<TConsumable>, IConfigProvider<Dictionary<ItemDefinition, Count>> {
    public Customs(Infinity<TConsumable> infinity) => Infinity = infinity;

    public long? GetRequirement(TConsumable consumable)
        => Config.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count? custom) ? custom.Value : null;

    public Dictionary<ItemDefinition, Count> Config { get; set; } = null!;
    public Infinity<TConsumable> Infinity { get; }

    public ProviderDefinition ProviderDefinition => ProviderDefinition.Customs;
}

public sealed class Customs<TConsumable, TCategory> : ICustoms<TConsumable>, IConfigProvider<Dictionary<ItemDefinition, Count<TCategory>>> where TCategory : struct, Enum {
    public Customs(Infinity<TConsumable, TCategory> infinity) => Infinity = infinity;

    public TCategory? GetCategory(TConsumable consumable)
        => Config.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value < 0 ? custom.Category : null;

    public long? GetRequirement(TConsumable consumable)
        => Config.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value >= 0 ? custom.Value : null;

    public bool SaveDetectedCategory(TConsumable consumable, TCategory category) {
        if (!InfinitySettings.Instance.detectMissingCategories || !Config.TryAdd(Infinity.Consumable.ToDefinition(consumable), new(category)))
            return false;
        InfinityManager.ClearCache();
        return true;
    }

    public Dictionary<ItemDefinition, Count<TCategory>> Config { get; set; } = null!;
    public Infinity<TConsumable, TCategory> Infinity { get; }

    public ProviderDefinition ProviderDefinition => ProviderDefinition.Customs;
}
using System;
using System.Collections.Generic;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class CustomRequirements<TCount> where TCount : Count {
    public Dictionary<ItemDefinition, TCount> customs = [];
}

public interface ICustoms<TConsumable> {
    long? GetRequirement(TConsumable consumable);
}

public sealed class Customs<TConsumable> : ICustoms<TConsumable>, IConfigProvider<CustomRequirements<Count>> {
    public Customs(Infinity<TConsumable> infinity) => Infinity = infinity;

    public long? GetRequirement(TConsumable consumable)
        => Config.customs.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count? custom) ? custom.Value : null;

    public CustomRequirements<Count> Config { get; set; } = null!;
    public Infinity<TConsumable> Infinity { get; }
}

public sealed class Customs<TConsumable, TCategory> : ICustoms<TConsumable>, IConfigProvider<CustomRequirements<Count<TCategory>>> where TCategory : struct, Enum {
    public Customs(Infinity<TConsumable, TCategory> infinity) => Infinity = infinity;

    public TCategory? GetCategory(TConsumable consumable)
        => Config.customs.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value < 0 ? custom.Category : null;

    public long? GetRequirement(TConsumable consumable)
        => Config.customs.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value >= 0 ? custom.Value : null;

    public bool SaveDetectedCategory(TConsumable consumable, TCategory category) {
        if (!InfinitySettings.Instance.detectMissingCategories || !Config.customs.TryAdd(Infinity.Consumable.ToDefinition(consumable), new(category)))
            return false;
        InfinityManager.ClearCache();
        return true;
    }

    public CustomRequirements<Count<TCategory>> Config { get; set; } = null!;
    public Infinity<TConsumable, TCategory> Infinity { get; }
}
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class CustomRequirements<TCount> where TCount : Count {
    public Dictionary<ItemDefinition, TCount> customs = [];
}

public interface ICustoms<TConsumable> {
    Optional<Requirement> GetRequirement(TConsumable consumable);
}

public sealed class Customs<TConsumable> : ICustoms<TConsumable>, IConfigProvider<CustomRequirements<Count>> {
    public Customs(Infinity<TConsumable> infinity) => Infinity = infinity;

    public Optional<Requirement> GetRequirement(TConsumable consumable)
        => Config.customs.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count? custom) ? new(new(custom.Value)) : default;

    public CustomRequirements<Count> Config { get; set; } = null!;
    public Infinity<TConsumable> Infinity { get; }
}

public sealed class Customs<TConsumable, TCategory> : ICustoms<TConsumable>, IConfigProvider<CustomRequirements<Count<TCategory>>> where TCategory : struct, Enum {
    public Customs(Infinity<TConsumable, TCategory> infinity) => Infinity = infinity;

    public Optional<TCategory> GetCategory(TConsumable consumable)
        => Config.customs.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value < 0 ? new(custom.Category) : default;

    public Optional<Requirement> GetRequirement(TConsumable consumable)
        => Config.customs.TryGetValue(Infinity.Consumable.ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value >= 0 ? new(new(custom.Value)) : default;

    public bool SaveDetectedCategory(TConsumable consumable, TCategory category) {
        if (!InfinitySettings.Instance.DetectMissingCategories || !Config.customs.TryAdd(Infinity.Consumable.ToDefinition(consumable), new(category)))
            return false;
        InfinityManager.ClearCache();
        return true;
    }

    public CustomRequirements<Count<TCategory>> Config { get; set; } = null!;
    public Infinity<TConsumable, TCategory> Infinity { get; }
}
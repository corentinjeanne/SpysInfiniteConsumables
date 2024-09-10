using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SPIC.Configs;
using Terraria.ModLoader.Config;

namespace SPIC.Default.Components;

public class CustomRequirements<TCount> where TCount : Count {
    public Dictionary<ItemDefinition, TCount> customs = [];
}

public sealed class Customs<TConsumable> : Component<Infinity<TConsumable>>, IConfigurableComponents<CustomRequirements<Count>> {
    public Customs(Func<TConsumable, ItemDefinition> toDefinition) => ToDefinition = toDefinition;
    public override void Bind() {
        Endpoints.GetRequirement(Infinity).AddProvider(GetRequirement);
    }

    private Optional<Requirement> GetRequirement(TConsumable consumable) {
        var customRequirements = InfinitySettings.Get(this);
        return customRequirements.customs.TryGetValue(ToDefinition(consumable), out Count? custom) ? new(new(custom.Value)) : default;
    }

    public Func<TConsumable, ItemDefinition> ToDefinition { get; }
}

public sealed class Customs<TConsumable, TCategory> : Component<Infinity<TConsumable>>, IConfigurableComponents<CustomRequirements<Count<TCategory>>>, ICategoryAccessor<TConsumable, TCategory> where TCategory : struct, Enum {
    public Customs(Func<TConsumable, ItemDefinition> toDefinition) => ToDefinition = toDefinition;
    public override void Bind() {
        Endpoints.GetRequirement(Infinity).AddProvider(GetRequirement);
        Endpoints.GetCategory(this).AddProvider(GetCategory);
    }

    private Optional<TCategory> GetCategory(TConsumable consumable) {
        var customRequirements = InfinitySettings.Get(this);
        return customRequirements.customs.TryGetValue(ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value < 0 ? new(custom.Category) : default;
    }

    public bool SaveDetectedCategory(TConsumable consumable, TCategory category) {
        if (!InfinitySettings.Instance.DetectMissingCategories || !InfinitySettings.Get(this).customs.TryAdd(ToDefinition(consumable), new(category)))
            return false;

        Endpoints.ClearCache();
        return true;
    }

    private Optional<Requirement> GetRequirement(TConsumable consumable) {
        var customRequirements = InfinitySettings.Get(this);
        return customRequirements.customs.TryGetValue(ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value >= 0 ? new(new(custom.Value)) : default;
    }

    public Func<TConsumable, ItemDefinition> ToDefinition { get; }
}
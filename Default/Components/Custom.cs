using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SPIC.Configs;
using Terraria.ModLoader.Config;

namespace SPIC.Default.Components;

public class CustomRequirements<TCount> where TCount: Count {
    public Dictionary<ItemDefinition, TCount> customs = [];
}

public sealed class Custom<TConsumable> : Component<Infinity<TConsumable>>, IConfigurableComponents<CustomRequirements<Count>> where TConsumable: notnull {
    public Custom(Func<TConsumable, ItemDefinition> toDefinition) => ToDefinition = toDefinition;
    public override void Load() {
        InfinityManager.GetRequirementEndpoint(Infinity).Register(GetRequirement);
    }

    public Optional<Requirement> GetRequirement(TConsumable consumable) {
        var customRequirements = InfinitySettings.Get(this);
        return customRequirements.customs.TryGetValue(ToDefinition(consumable), out Count? custom) ? new(new(custom.Value)) : default;
    }

    public Func<TConsumable, ItemDefinition> ToDefinition { get; }
}

public sealed class   Custom<TConsumable, TCategory> : Component<Infinity<TConsumable, TCategory>>, IConfigurableComponents<CustomRequirements<Count<TCategory>>> where TConsumable: notnull where TCategory: struct, Enum {
    public Custom(Func<TConsumable, ItemDefinition> toDefinition) => ToDefinition = toDefinition;
    public override void Load() {
        InfinityManager.GetRequirementEndpoint(Infinity).Register(GetRequirement);
        InfinityManager.GetCategoryEndpoint(Infinity).Register(GetCategory);
    }

    public Optional<TCategory> GetCategory(TConsumable consumable) {
        var customRequirements = InfinitySettings.Get(this);
        return customRequirements.customs.TryGetValue(ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value < 0 ? new(custom.Category) : default;
    }
    
    public Optional<Requirement> GetRequirement(TConsumable consumable) {
        var customRequirements = InfinitySettings.Get(this);
        return customRequirements.customs.TryGetValue(ToDefinition(consumable), out Count<TCategory>? custom) && custom.Value >= 0 ? new(new(custom.Value)) : default;
    }

    public Func<TConsumable, ItemDefinition> ToDefinition { get; }
}
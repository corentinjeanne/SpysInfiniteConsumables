using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SPIC.Configs;
using Terraria;
using Terraria.ModLoader.Config;

namespace SPIC;

public interface IConsumableInfinity : IInfinity {
    ReadOnlyCollection<Preset> Presets { get; }
    ReadOnlyCollection<IInfinity> Infinities { get; }
}

public abstract class ConsumableInfinity<TConsumable> : Infinity<TConsumable>, IConsumableInfinity {

    public override ConsumableInfinity<TConsumable> Consumable => this;
    public ConsumableInfinitiesProvider<TConsumable> ConsumableInfinities { get; private set; } = null!;

    public abstract int GetId(TConsumable consumable);
    public abstract TConsumable ToConsumable(int id);
    public abstract TConsumable ToConsumable(Item item);
    public abstract long CountConsumables(Player player, TConsumable consumable);
    public abstract ItemDefinition ToDefinition(TConsumable consumable);

    public HashSet<Infinity<TConsumable>> UsedInfinities(TConsumable consumable) {
        HashSet<Infinity<TConsumable>> usedInfinities = [];
        int max = ConsumableInfinities.Config.UsedInfinities;
        foreach (Infinity<TConsumable> infinity in _orderedInfinities) {
            if (!infinity.Enabled) continue;
            Requirement value = infinity.GetRequirement(consumable);
            if (value.IsNone) continue;
            usedInfinities.Add(infinity);
            if (usedInfinities.Count == max) break;
        }
        return usedInfinities;
    }

    protected sealed override Requirement GetRequirementInner(TConsumable consumable) {
        long count = 0;
        float multiplier = float.MaxValue;
        foreach (Infinity<TConsumable> infinity in InfinityManager.UsedInfinities(consumable, this)) {
            Requirement requirement = InfinityManager.GetRequirement(consumable, infinity);
            count = Math.Max(count, requirement.Count);
            multiplier = Math.Min(multiplier, requirement.Multiplier);
        }
        return new(count, multiplier);
    }
    
    public void AddInfinity(Infinity<TConsumable> infinity) {
        _infinities.Add(infinity);
        _orderedInfinities.Add(infinity);
    }

    public ReadOnlyCollection<Infinity<TConsumable>> Infinities => _orderedInfinities.AsReadOnly();
    ReadOnlyCollection<IInfinity> IConsumableInfinity.Infinities => _orderedInfinities.Cast<IInfinity>().ToArray().AsReadOnly();
    public ReadOnlyCollection<Preset> Presets => _presets.AsReadOnly();

    internal readonly HashSet<Infinity<TConsumable>> _infinities = [];
    internal readonly List<Infinity<TConsumable>> _orderedInfinities = [];
    private readonly List<Preset> _presets = [];

    public override void Load() {
        ConsumableInfinities = new(this);
        AddConfig(ConsumableInfinities, "infinities");
        base.Load();
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        foreach (var preset in PresetLoader.Presets) {
            if (preset.AppliesTo(this)) _presets.Add(preset);
        }
    }
}

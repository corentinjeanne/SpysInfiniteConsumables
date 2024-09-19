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

public abstract class ConsumableInfinity<TConsumable> : Infinity<TConsumable>, IConsumableInfinity, IConsumableBridge {

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
        foreach (Infinity<TConsumable> infinity in _orderedInfinities.Where(i => i.Enabled)) {
            long value = infinity.GetRequirement(consumable);
            if (value <= 0) continue;
            usedInfinities.Add(infinity);
            if (usedInfinities.Count == max) break;
        }
        return usedInfinities;
    }

    protected sealed override long GetRequirementInner(TConsumable consumable) {
        long requirement = 0;
        foreach (Infinity<TConsumable> infinity in InfinityManager.UsedInfinities(consumable, this).Where(i => i.Enabled))
            requirement = Math.Max(requirement, InfinityManager.GetRequirement(consumable, infinity));
        return requirement;
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

    long IConsumableBridge.CountConsumables(Player player, int consumable) => player.CountConsumables(ToConsumable(consumable), this);
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SPIC.Configs;
using Terraria;
using Terraria.ModLoader.Config;

namespace SPIC;

public readonly struct ConsumableDefaults() {
    public readonly Preset? Preset { get; init; }
    public readonly DisplayedInfinities DisplayedInfinities { get; init; } = DisplayedInfinities.Infinities;
}

public interface IConsumableInfinity : IInfinity {
    ReadOnlyCollection<Preset> Presets { get; }
    ReadOnlyCollection<IInfinity> Infinities { get; }
    DisplayedInfinities DisplayedInfinities { get; }
    ConsumableDefaults ConsumableDefaults { get; }

    long CountConsumables(Player player, int consumable);
}

public abstract class ConsumableInfinity<TConsumable> : Infinity<TConsumable>, IConsumableInfinity {

    public sealed override ConsumableInfinity<TConsumable> Consumable => this;
    public ConsumableInfinitiesProvider<TConsumable> ConsumableInfinities { get; private set; } = null!;

    public abstract int GetId(TConsumable consumable);
    public abstract TConsumable ToConsumable(int id);
    public abstract TConsumable ToConsumable(Item item);
    public abstract long CountConsumables(Player player, TConsumable consumable);
    public abstract ItemDefinition ToDefinition(TConsumable consumable);

    public IReadOnlySet<IInfinity> UnusedInfinities(TConsumable consumable) {
        HashSet<IInfinity> unusedInfinities = [];
        int used = 0;
        int max = ConsumableInfinities.Config.usedInfinities;
        foreach (Infinity<TConsumable> infinity in _orderedInfinities.Where(i => i.Enabled)) {
            long value = infinity.GetRequirement(consumable);
            if (value <= 0) continue;
            if (max == 0 || used < max) used++;
            else unusedInfinities.Add(infinity);
        }
        return unusedInfinities;
    }

    protected sealed override long GetRequirementInner(TConsumable consumable) {
        long requirement = 0;
        var unused = InfinityManager.UnusedInfinities(consumable, this);
        foreach (Infinity<TConsumable> infinity in _orderedInfinities.Where(i => i.Enabled && !unused.Contains(i)))
            requirement = Math.Max(requirement, InfinityManager.GetRequirement(consumable, infinity));
        return requirement;
    }

    protected sealed override long GetInfinityInner(TConsumable consumable, long count) {
        long value = long.MaxValue;
        var unused = InfinityManager.UnusedInfinities(consumable, this);
        foreach (Infinity<TConsumable> infinity in _orderedInfinities.Where(i => i.Enabled && !unused.Contains(i) && InfinityManager.GetRequirement(consumable, i) > 0))
            value = Math.Min(value, InfinityManager.GetInfinity(consumable, count, infinity));
        return value == long.MaxValue ? 0 : value;
    }

    public void AddInfinity(Infinity<TConsumable> infinity) {
        _infinities.Add(infinity);
        _orderedInfinities.Add(infinity);
    }

    public ReadOnlyCollection<Infinity<TConsumable>> Infinities => _orderedInfinities.AsReadOnly();
    ReadOnlyCollection<IInfinity> IConsumableInfinity.Infinities => _orderedInfinities.Cast<IInfinity>().ToArray().AsReadOnly();

    public virtual ConsumableDefaults ConsumableDefaults => new();
    public ReadOnlyCollection<Preset> Presets => _presets.AsReadOnly();

    public DisplayedInfinities DisplayedInfinities => ConsumableInfinities.ClientConfig.displayedInfinities;

    internal readonly HashSet<Infinity<TConsumable>> _infinities = [];
    internal readonly List<Infinity<TConsumable>> _orderedInfinities = [];
    private readonly List<Preset> _presets = [];

    public override void Load() {
        ConsumableInfinities = new(this);
        AddConfig(ConsumableInfinities);
        base.Load();
    }

    public override void SetStaticDefaults() {
        foreach (var preset in PresetLoader.Presets) {
            if (preset.AppliesTo(this)) _presets.Add(preset);
        }
    }

    long IConsumableInfinity.CountConsumables(Player player, int consumable) => player.CountConsumables(ToConsumable(consumable), this);
}


using System;
using Microsoft.CodeAnalysis;

namespace SPIC.Default.Components;

public sealed class Group<TConsumable> : Component<Infinity<TConsumable>> {
    public Group(Func<InfinityGroup<TConsumable>> getter) => _getter = getter;

    public override void Load() {
        Endpoints.IdInfinity(Infinity).Register(_ => InfinityGroup.Infinity);
        Endpoints.GetRequirement(Infinity).Register(GetRequirement);
    }
    public override void SetStaticDefaults() {
        InfinityGroup ??= _getter();
        InfinityGroup.RegisterChild(this);
        InfinityManager.UnregisterRootInfinity(Infinity);
    }
    public override void Unload() {
        InfinityGroup = null!;
        _getter = null!;
    }

    internal Optional<Requirement> GetRequirement(TConsumable consumable)
        => InfinityManager.UsedInfinities(consumable, InfinityGroup).Contains(Infinity) ? default : InfinityManager.GetRequirement(consumable, InfinityGroup);

    public InfinityGroup<TConsumable> InfinityGroup { get; private set; } = null!;
    private Func<InfinityGroup<TConsumable>> _getter;

    public static implicit operator InfinityGroup<TConsumable>(Group<TConsumable> group) => group.InfinityGroup;
}
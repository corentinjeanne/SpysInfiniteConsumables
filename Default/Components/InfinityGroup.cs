
using System;
using Microsoft.CodeAnalysis;

namespace SPIC.Default.Components;

public sealed class InfinityGroup<TConsumable> : Component<Infinity<TConsumable>> {
    public InfinityGroup(Func<GroupInfinity<TConsumable>> getter) => _getter = getter;

    public override void Load() {
        Endpoints.GetRequirement(Infinity).Register(GetRequirement);
        Endpoints.GetIdGroup(Infinity).Register(_ => Group);
    }

    public override void SetStaticDefaults() {
        Group.RegisterChild(Infinity);
        InfinityManager.UnregisterRootInfinity(Infinity);
    }

    private Optional<Requirement> GetRequirement(TConsumable consumable) {
        return new(); // TODO
    }

    public GroupInfinity<TConsumable> Group => _getter();
    private Func<GroupInfinity<TConsumable>> _getter;

    // public static implicit operator InfinityGroup<TConsumable>(GroupInfinity<TConsumable> group) => new(group);
    public static implicit operator GroupInfinity<TConsumable>(InfinityGroup<TConsumable> group) => group.Group;
}
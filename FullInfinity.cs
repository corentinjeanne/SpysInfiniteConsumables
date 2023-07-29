using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;
using Terraria;

namespace SPIC;

public class FullInfinity {

    private FullInfinity() {}

    public Requirement Requirement { get; private set; }
    public long Count { get; private set; }
    public long Infinity { get; private set; }
    public List<object> Extras { get; private set; } = new();

    public static FullInfinity None => new();

    public static FullInfinity With(Requirement requirement, long count, long infinity, params object[] extras) => new() {
        Requirement = requirement,
        Count = count,
        Infinity = infinity,
        Extras = new(extras)
    };

    internal static FullInfinity WithRequirement<TGroup, TConsumable>(TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        FullInfinity fullInfinity = new();
        Group<TGroup, TConsumable> group = infinity.Group;

        List<object> extras = new();
        Requirement requirement = infinity.GetRequirement(consumable, extras);
        long maxStack = group.MaxStack(consumable); // TODO rework
        if (maxStack != 0 && requirement.Count > maxStack) requirement = new(maxStack, requirement.Multiplier);
        
        if (group.Config.HasCustomCount(consumable, infinity, out Count? custom)) {
            extras.Clear();
            extras.Add(new InfinityOverride("Custom"));
            requirement = new Requirement(custom, requirement.Multiplier);
        }

        fullInfinity.Requirement = requirement;
        fullInfinity.Extras = extras;
        return fullInfinity;
    }

    internal static FullInfinity WithInfinity<TGroup, TConsumable>(Player player, TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        FullInfinity fullInfinity = WithRequirement(consumable, infinity);
        fullInfinity.Count = infinity.Group.CountConsumables(player, consumable);
        long infinityValue = fullInfinity.Requirement.Infinity(fullInfinity.Count);
        infinity.OverrideInfinity(player, consumable, fullInfinity.Requirement, fullInfinity.Count, ref infinityValue, fullInfinity.Extras);
        fullInfinity.Infinity = infinityValue;
        return fullInfinity;
    }
}
public record class InfinityOverride(string Type) {
    public override string ToString() => Type;
}
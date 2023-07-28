using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using SPIC.Configs;
using Terraria;

namespace SPIC;

public class FullInfinity {

    private FullInfinity() {}

    public T Add<T>(T value) where T : notnull {
        _extras.Add(value);
        return value;
    }

    public bool Has<T>([NotNullWhen(true)] out T? value) {
        value = (T?)_extras.Find(e => e is T);
        return value is not null;
    }

    public Requirement Requirement { get; private set; }
    public long Count { get; private set; }
    public long Infinity { get; private set; }
    public ReadOnlyCollection<object> Extras => _extras.AsReadOnly();

    private List<object> _extras = new();

    public static FullInfinity None => new();

    public static FullInfinity With(Requirement requirement, long count, long infinity, params object[] extras) => new() {
        Requirement = requirement,
        Count = count,
        Infinity = infinity,
        _extras = new(extras)
    };

    internal static FullInfinity WithRequirement<TGroup, TConsumable>(TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        FullInfinity fullInfinity = new();
        Group<TGroup, TConsumable> group = infinity.Group;
        if (group.Config.HasCustomCount(consumable, infinity, out Count? custom)) { // Not used
            fullInfinity.Add(new InfinityOverride("Custom"));
            fullInfinity.Requirement = new Requirement(custom, infinity.GetRequirement(consumable, new()).Multiplier);
        } else {
            Requirement requirement = infinity.GetRequirement(consumable, fullInfinity);
            long maxStack = group.MaxStack(consumable);
            if (maxStack != 0 && requirement.Count > maxStack) requirement = new(maxStack, requirement.Multiplier);
            fullInfinity.Requirement = requirement;
        }
        return fullInfinity;
    }

    internal static FullInfinity WithInfinity<TGroup, TConsumable>(Player player, TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        FullInfinity fullInfinity = WithRequirement(consumable, infinity);
        fullInfinity.Count = infinity.Group.CountConsumables(player, consumable);
        fullInfinity.Infinity = fullInfinity.Requirement.Infinity(fullInfinity.Count);
        return fullInfinity;
    }
}
public record class InfinityOverride(string Type) {
    public override string ToString() => Type;
}
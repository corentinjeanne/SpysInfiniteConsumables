namespace SPIC.ConsumableGroup;

public abstract class Requirement {
    public abstract ICount NextRequirement(ICount count);
    public abstract Infinity Infinity(ICount count);
}
public abstract class Requirement<TCount> where TCount : ICount{
    public abstract TCount NextRequirement(TCount count);
    public abstract Infinity Infinity(TCount count);
}

public sealed class NoRequirement : Requirement {
    public override Infinity Infinity(ICount count) => new(count.None, 0);
    public override ICount NextRequirement(ICount count) => count.None;
}

public abstract class FixedRequirement : Requirement {
    public FixedRequirement(ICount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }
    public ICount Root { get; init; }

    public abstract ICount EffectiveRequirement(ICount count);
    public sealed override Infinity Infinity(ICount count) => new(EffectiveRequirement(count), Multiplier);
    public sealed override ICount NextRequirement(ICount count) => Root.CompareTo(count) > 0 ? Root.AdaptTo(count) : count.None;
}

public abstract class RecursiveRequirement : Requirement {
    protected RecursiveRequirement(ICount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }
    public ICount Root { get; init; }

    public ICount EffectiveRequirement(ICount count){
        ICount effective = count.None;
        ICount next = Root.AdaptTo(count);
        while(next.CompareTo(count) <= 0){
            effective = next;
            next = NextValue(next);
        }
        return effective;
    }

    protected abstract ICount NextValue(ICount value);
    public sealed override Infinity Infinity(ICount count) => new(EffectiveRequirement(count), Multiplier);
    public sealed override ICount NextRequirement(ICount count) => count.IsNone ? Root.AdaptTo(count) : NextValue(count);
}


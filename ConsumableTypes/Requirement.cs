namespace SPIC.ConsumableGroup;

public abstract class Requirement<TCount> where TCount : notnull, ICount<TCount>{
    public abstract TCount NextRequirement(TCount count);
    public abstract Infinity<TCount> Infinity(TCount count);

    public abstract bool IsNone { get; }
}

public sealed class NoRequirement<TCount> : Requirement<TCount> where TCount : notnull, ICount<TCount>{
    public override Infinity<TCount> Infinity(TCount count) => new(count.None, 0);
    public override TCount NextRequirement(TCount count) => count.None;
    public override bool IsNone => true;
}

public abstract class FixedRequirement<TCount> : Requirement<TCount> where TCount : notnull, ICount<TCount> {
    public FixedRequirement(TCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }
    public TCount Root { get; init; }
    public override bool IsNone => Root.IsNone || Multiplier == 0;

    public abstract TCount EffectiveRequirement(TCount count);
    public sealed override Infinity<TCount> Infinity(TCount count) => new(EffectiveRequirement(count), Multiplier);
    public sealed override TCount NextRequirement(TCount count) => Root.CompareTo(count) > 0 ? Root.AdaptTo(count) : count.None;
}

public abstract class RecursiveRequirement<TCount> : Requirement<TCount> where TCount : notnull, ICount<TCount> {
    protected RecursiveRequirement(TCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }
    public TCount Root { get; init; }

    public override bool IsNone => Root.IsNone || Multiplier == 0;

    public TCount EffectiveRequirement(TCount count){
        TCount effective = count.None;
        TCount next = Root.AdaptTo(count);
        while(next.CompareTo(count) <= 0){
            effective = next;
            next = NextValue(next);
        }
        return effective;
    }

    protected abstract TCount NextValue(TCount value);
    public sealed override Infinity<TCount> Infinity(TCount count) => new(EffectiveRequirement(count), Multiplier);
    public sealed override TCount NextRequirement(TCount count) => count.IsNone ? Root.AdaptTo(count) : NextValue(count);
}


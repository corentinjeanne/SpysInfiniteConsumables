namespace SPIC.ConsumableGroup;

public abstract class Requirement<TCount> where TCount : struct, ICount<TCount>{
    public TCount Root { get; set; }
    public abstract bool IsNone { get; }
    public abstract TCount NextRequirement(TCount count);
    public abstract Infinity<TCount> Infinity(TCount count);
}

public sealed class NoRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount>{
    public override bool IsNone => true;
    public override Infinity<TCount> Infinity(TCount count) => new(count.None, 0);
    public override TCount NextRequirement(TCount count) => count.None;
}

public abstract class FixedRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount> {
    public override bool IsNone => Root.IsNone || Multiplier == 0;
    public float Multiplier { get; init; }
    
    public FixedRequirement(TCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public abstract TCount EffectiveRequirement(TCount count);
    public sealed override Infinity<TCount> Infinity(TCount count) => new(EffectiveRequirement(count), Multiplier);
    public sealed override TCount NextRequirement(TCount count) => Root.IsNone || Root.CompareTo(count) > 0 ? Root.AdaptTo(count) : count.None;
}

public abstract class RecursiveRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount> {
    public override bool IsNone => Root.IsNone || Multiplier == 0;
    protected RecursiveRequirement(TCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }

    public TCount EffectiveRequirement(TCount count){
        if(Root.IsNone) return count.None;
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
    public sealed override TCount NextRequirement(TCount count) => Root.IsNone || count.IsNone ? Root.AdaptTo(count) : NextValue(count);
}


namespace SPIC.ConsumableGroup;

public abstract class Requirement<TCount> where TCount : struct, ICount<TCount>{
    public abstract bool IsNone { get; }
    public abstract TCount NextRequirement(TCount count);
    public abstract Infinity<TCount> Infinity(TCount count);

    public abstract void Customize(TCount custom);
}

public sealed class NoRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount>{
    public override bool IsNone => true;
    public override Infinity<TCount> Infinity(TCount count) => new(count.None, 0);
    public override TCount NextRequirement(TCount count) => count.None;
    public override void Customize(TCount custom) {}
}

public abstract class FixedRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount> {
    public TCount Root { get; private set; }
    public override bool IsNone => Root.IsNone || Multiplier == 0;
    public float Multiplier { get; }
    
    public FixedRequirement(TCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public abstract TCount EffectiveRequirement(TCount count);
    public sealed override Infinity<TCount> Infinity(TCount count) => new(EffectiveRequirement(count), Multiplier);
    public sealed override TCount NextRequirement(TCount count) => Root.IsNone || Root.CompareTo(count) > 0 ? Root.AdaptTo(count) : count.None;
    public sealed override void Customize(TCount custom) => Root = custom;
}

public abstract class RecursiveRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount> {
    public TCount Root { get; private set; }
    public TCount? MaxRequirement { get; }
    public override bool IsNone => Root.IsNone || Multiplier == 0;
    public float Multiplier { get; }
    
    protected RecursiveRequirement(TCount root, float multiplier, TCount? maxInfinity) {
        Root = root;
        Multiplier = multiplier;
        MaxRequirement = maxInfinity?.Multiply(1 / Multiplier);
    }

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
    public sealed override TCount NextRequirement(TCount count) => (MaxRequirement.HasValue && MaxRequirement.Value.CompareTo(count) < 0) ? count.None : Root.IsNone || count.IsNone ? Root.AdaptTo(count) : NextValue(count);
    public sealed override void Customize(TCount custom) => Root = custom;
}


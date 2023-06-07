namespace SPIC.ConsumableGroup;

public sealed class CountRequirement<TCount> : FixedRequirement<TCount> where TCount : struct, ICount<TCount> {
    public CountRequirement(TCount root, float multiplier = 1) : base(root, multiplier) { }

    public override TCount EffectiveRequirement(TCount count) => Root.IsNone || Root.CompareTo(count) > 0 ? count.None : count;
}

public sealed class DisableAboveRequirement<TCount> : Requirement<TCount> where TCount : struct, ICount<TCount> {
    public TCount Root { get; private set; }
    public override bool IsNone => Root.IsNone || Multiplier == 0;
    public DisableAboveRequirement(TCount root, float multiplier = 1) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }

    public override Infinity<TCount> Infinity(TCount count) => (Root.IsNone ? 1 : Root.CompareTo(count)) switch {
        0 => new(count, Multiplier),
        < 0 => new(count, 0),
        _ => new(count.None, Multiplier)
    };
    public override TCount NextRequirement(TCount count) => Root.IsNone || Root.CompareTo(count) > 0 ? Root.AdaptTo(count) : count.None;
    public sealed override void Customize(TCount custom) => Root = custom;
}

public sealed class PowerRequirement<TCount> : RecursiveRequirement<TCount> where TCount : struct, ICount<TCount> {
    public PowerRequirement(TCount root, int power, float multiplier = 1, TCount? maxInfinity = null) : base(root, multiplier, maxInfinity) {
        Power = power;
    }

    public int Power { get; init; }

    protected override TCount NextValue(TCount value) => value.Multiply(Power);
}

public sealed class MultipleRequirement<TCount> : RecursiveRequirement<TCount> where TCount : struct, ICount<TCount> {
    public MultipleRequirement(TCount root, float multiplier = 1, TCount? maxInfinity = null) : base(root, multiplier, maxInfinity) { }

    protected override TCount NextValue(TCount value) => value.Add(Root);
}
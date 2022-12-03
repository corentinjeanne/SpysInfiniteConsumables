namespace SPIC.ConsumableGroup;

public sealed class CountRequirement<TCount> : FixedRequirement<TCount> where TCount : notnull, ICount<TCount> {
    public CountRequirement(TCount root, float multiplier = 1) : base(root, multiplier) { }

    public override TCount EffectiveRequirement(TCount count) => Root.CompareTo(count) > 0 ? count.None : count;
}

public sealed class DisableAboveRequirement<TCount> : FixedRequirement<TCount> where TCount : notnull, ICount<TCount> {
    public DisableAboveRequirement(TCount root, float multiplier = 1) : base(root, multiplier) { }

    public override TCount EffectiveRequirement(TCount count) => Root.CompareTo(count) == 0 ? count : count.None;
}

public sealed class PowerRequirement<TCount> : RecursiveRequirement<TCount> where TCount : notnull, ICount<TCount> {
    public PowerRequirement(TCount root, int power, float multiplier = 1) : base(root, multiplier) {
        Power = power;
    }

    public int Power { get; init; }

    protected override TCount NextValue(TCount value) => value.Multiply(Power);
}

public sealed class MultipleRequirement<TCount> : RecursiveRequirement<TCount> where TCount : notnull, ICount<TCount> {
    public MultipleRequirement(TCount root, float multiplier = 1) : base(root, multiplier) { }

    protected override TCount NextValue(TCount value) => value.Add(Root);
}
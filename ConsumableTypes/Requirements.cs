namespace SPIC.ConsumableGroup;

public sealed class CountRequirement : FixedRequirement {
    public CountRequirement(ICount root, float multiplier = 1) : base(root, multiplier) { }

    public override ICount EffectiveRequirement(ICount count) => Root.CompareTo(count) >= 0 ? count.None : count;
}

public sealed class DisableAboveRequirement : FixedRequirement {
    public DisableAboveRequirement(ICount root, float multiplier = 1) : base(root, multiplier) { }

    public override ICount EffectiveRequirement(ICount count) => Root.CompareTo(count) == 0 ? count : count.None;
}

public sealed class PowerRequirement : RecursiveRequirement {
    public PowerRequirement(ICount root, int power, float multiplier = 1) : base(root, multiplier) {
        Power = power;
    }

    public int Power { get; init; }

    protected override ICount NextValue(ICount value) => value.Multiply(Power);
}

public sealed class MultipleRequirement : RecursiveRequirement {
    public MultipleRequirement(ICount root, float multiplier = 1) : base(root, multiplier) { }

    protected override ICount NextValue(ICount value) => value.Add(Root);
}
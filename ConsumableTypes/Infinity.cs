namespace SPIC.ConsumableGroup;

public readonly struct Infinity<TCount> where TCount : ICount<TCount> {
    public Infinity(TCount effectiveRequirement, float multiplier) {
        EffectiveRequirement = effectiveRequirement;
        Multiplier = multiplier;
    }

    public bool CountsAsNone => EffectiveRequirement.IsNone;

    public TCount EffectiveRequirement { get; init; }
    public float Multiplier { get; init; }
    public TCount Value => EffectiveRequirement.Multiply(Multiplier);
}
namespace SPIC.ConsumableGroup;

public struct Infinity<TCount> where TCount : ICount<TCount> {
    public Infinity(TCount effectiveRequirement, float multiplier) {
        EffectiveRequirement = effectiveRequirement;
        Multiplier = multiplier;
    }

    public TCount EffectiveRequirement { get; init; }
    public float Multiplier { get; init; }
    public TCount Value => EffectiveRequirement.Multiply(Multiplier);
}
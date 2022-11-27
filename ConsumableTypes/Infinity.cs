namespace SPIC.ConsumableGroup;

public struct Infinity {
    public Infinity(ICount effectiveRequirement, float multiplier) {
        EffectiveRequirement = effectiveRequirement;
        Multiplier = multiplier;
    }

    public ICount EffectiveRequirement { get; init; }
    public float Multiplier { get; init; }
    public ICount Value => EffectiveRequirement.Multiply(Multiplier);
}
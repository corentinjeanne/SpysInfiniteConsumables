namespace SPIC.ConsumableTypes;

public struct Infinity {
    public Infinity(ItemCount effectiveRequirement, float multiplier) {
        EffectiveRequirement = effectiveRequirement;
        Multiplier = multiplier;
    }

    public ItemCount EffectiveRequirement { get; init; }
    public float Multiplier { get; init; }
    public ItemCount Value => EffectiveRequirement * Multiplier;

    public static readonly Infinity None = new(ItemCount.None, 0);
}
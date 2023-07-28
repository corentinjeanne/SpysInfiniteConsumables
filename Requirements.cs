using System;

namespace SPIC;

public readonly record struct Requirement(long Count, float Multiplier = 1f) {

    public readonly bool IsNone => Count == 0 || Multiplier == 0f;

    public long Infinity(long count) => count >= Count ? (long)(count * Multiplier) : 0;

    public long CountForInfinity(long infinity) => Math.Max(Count, (int)MathF.Ceiling(infinity / Multiplier));
}
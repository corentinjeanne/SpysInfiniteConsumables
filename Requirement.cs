using System;

namespace SPIC;

public readonly record struct Requirement(long Count, float Multiplier = 1f) {

    public static Requirement None => default;
    public readonly bool IsNone => Count <= 0 || Multiplier <= 0f;

    public long Infinity(long count) => IsNone || count < Count ? 0 : (long)(count * Multiplier);
    public Requirement ForInfinity(long infinity, float? multiplier = null) => IsNone ? this : new(Math.Max(Count, (int)MathF.Ceiling(infinity / Multiplier)), multiplier ?? Multiplier);
}
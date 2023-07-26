using System;

namespace SPIC;

public readonly record struct Requirement(long Count, float Multiplier = 1f) {

    public readonly bool IsNone => Count == 0 || Multiplier == 0f;

    public long Infinity(long count) => count >= Count ? (long)(count * Multiplier) : 0;

    public long CountForInfinity(long infinity) => Math.Max(Count, (int)MathF.Ceiling(infinity / Multiplier));
}

public interface IFullRequirement {
    Requirement Requirement { get; }

    string ExtraInfo();
}

// TODO localize extra
public readonly record struct FullRequirement(Requirement Requirement) : IFullRequirement {
    public string ExtraInfo() => string.Empty;
}
public readonly record struct FullRequirement<TCategory>(TCategory Category, Requirement Requirement) : IFullRequirement where TCategory : System.Enum {
    public string ExtraInfo() => Category.ToString();
}
public readonly record struct MixedRequirement(Requirement Requirement) : IFullRequirement {
    public string ExtraInfo() => "Mixed";
}
public readonly record struct CustomRequirement(Requirement Requirement) : IFullRequirement {
    public string ExtraInfo() => "Custom";
}

public readonly record struct FullInfinity(IFullRequirement FullRequirement, long Count, long Infinity) {
    public Requirement Requirement => FullRequirement.Requirement;
}
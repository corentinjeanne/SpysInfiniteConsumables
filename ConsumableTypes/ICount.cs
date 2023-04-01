namespace SPIC.ConsumableGroup;

public interface ICount<TCount> : System.IComparable<TCount> where TCount : ICount<TCount> {
    long Value { init; }
    bool IsNone { get; }

    TCount Multiply(float value);
    TCount Add(TCount count);
    TCount AdaptTo(TCount reference);

    TCount None { get; }

    float Ratio(TCount other);

    string DisplayRawValue(Configs.InfinityDisplay.CountStyle style);
    string Display(Configs.InfinityDisplay.CountStyle style);
}

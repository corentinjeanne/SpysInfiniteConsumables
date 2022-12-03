namespace SPIC.ConsumableGroup;

public interface ICount<TCount> : System.IComparable<TCount> where TCount : ICount<TCount> {
    TCount Multiply(float value);
    TCount Add(TCount count);
    TCount AdaptTo(TCount reference);

    TCount None { get; }
    bool IsNone { get; }

    float Ratio(TCount other);

    string DisplayRawValue(Config.InfinityDisplay.CountStyle style);
    string Display(Config.InfinityDisplay.CountStyle style);
}

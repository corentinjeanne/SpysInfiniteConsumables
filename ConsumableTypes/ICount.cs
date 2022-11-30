namespace SPIC.ConsumableGroup;

public interface ICount : System.IComparable<ICount> {
    ICount Multiply(float value);
    ICount Add(ICount count);
    ICount AdaptTo(ICount reference);

    ICount None { get; }
    bool IsNone { get; }

    float Ratio(ICount other);

    string DisplayRawValue(Config.InfinityDisplay.CountStyle style);
    string Display(Config.InfinityDisplay.CountStyle style);
}

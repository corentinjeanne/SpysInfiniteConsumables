namespace SPIC.ConsumableGroup;

public interface ICount : System.IComparable<ICount> {
    ICount Multiply(float value);
    ICount Add(ICount count);
    ICount AdaptTo(ICount reference);

    ICount None { get; }
    bool IsNone { get; }

    string Display();
    string DisplayRatio(ICount other);
}

namespace SPIC.Categories;



public struct Infinity {
    public enum AboveRequirementInfinity{
        NotInfinite,
        AlwaysInfinite,
        InfiniteOnPower
    }
    public int Requirement;
    public int Multiplier;
    public AboveRequirementInfinity above;
}
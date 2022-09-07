namespace SPIC.Infinities;

public abstract class Infinity<Type> : Infinity where Type : Infinity<Type>, new() {
    public static readonly Type Instance = new();
    public static int ID => Instance.UID;
    protected Infinity() { }
    static Infinity() { }

    public static void Register() => InfinityManager.RegisterInfinity(Instance);
}

public abstract class Infinity {

    public int UID { get; internal set; } = -1;

    public abstract string Name { get; }
    public abstract int IconType { get; }

    public virtual bool DefaultValue => true;
}
using Terraria.ModLoader;

namespace SPIC.Infinities;

public abstract class Infinity<Type> : Infinity where Type : Infinity<Type>, new() {
    public static readonly Type Instance = new();
    public static int ID => Instance.UID;
    protected Infinity() { }
    static Infinity() { }

    public static void Register() => InfinityManager.RegisterInfinity(Instance);
}

public abstract class Infinity {
    public int UID { get; internal set; }
    public abstract Mod Mod { get; }
    public virtual string Name => GetType().Name;

    public virtual string LocalizedName => Name;
    public abstract int IconType { get; }

    public abstract bool DefaultValue { get; }

    public override string ToString() => Name;
}
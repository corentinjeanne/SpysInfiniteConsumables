using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

public abstract class ConsumableType<TImplementation> where TImplementation : ConsumableType<TImplementation>, IConsumableType, new() {
    public static TImplementation Instance => _instance ??= new TImplementation();
    public static int ID => Instance.UID;
    private static TImplementation _instance;
    static ConsumableType() { }
    protected ConsumableType() { }

    public static void Register() => InfinityManager.Register(Instance);
    public static void RegisterAsGlobal() => InfinityManager.Register(Instance, true);

    public abstract Mod Mod { get; }
    public string Name => GetType().Name;
    public abstract int IconType { get; }

    public int UID { get; internal set; }

    public string Label() {
        LabelAttribute label = System.Attribute.GetCustomAttribute(GetType(), typeof(LabelAttribute), true) as LabelAttribute;
        return label is not null ? label.Label : Name;
    }
}

public interface IStandardConsumableType<TCategory, TSettings> : IConsumableType<TCategory>, IToggleable, IColorable, IConfigurable<TSettings>, IDefaultInfinity<TCategory>, IDefaultDisplay
where TCategory : System.Enum
where TSettings : new() {}
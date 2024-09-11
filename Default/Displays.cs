using System.ComponentModel;
using Terraria.ModLoader.Config;
using SpikysLib.Configs;

namespace SPIC.Default.Displays;

public sealed class TooltipConfig {
    [DefaultValue(true)] public bool AddMissingLines = true;
}

// public interface ITooltipLineDisplay {
//     public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed);
// }
// public interface ICountToString {
//     public string CountToString(int consumable, long count, long value);
// }

public sealed class Tooltip : Display, IConfigurableDisplay<TooltipConfig> {
    public static Tooltip Instance = null!;

    // public static (TooltipLine, TooltipLineID?) GetTooltipLine(IInfinity infinity, Item item, int displayed) => infinity is ITooltipLineDisplay td ? td.GetTooltipLine(item, displayed) : DefaultTooltipLine(infinity);
    // public static string CountToString(IGroup group, int consumable, long owned, long value) => group is ICountToString cs ? cs.CountToString(consumable, owned, value) : DefaultCount(owned, value);

    // public static (TooltipLine, TooltipLineID?) DefaultTooltipLine(IInfinity infinity) => (new(infinity.Mod, infinity.Name, infinity.DisplayName.Value), null);
    // public static string DefaultCount(long owned, long value) => owned == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value) : $"{owned}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value)}";
}

public sealed class GlowConfig {
    [DefaultValue(true)] public bool FancyGlow = true;
    [DefaultValue(0.75f)] public float Intensity = 0.75f;
    [DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] public float AnimationLength = 2f;
}

public sealed class Glow : Display, IConfigurableDisplay<GlowConfig> {
    public static Glow Instance = null!;
}
public sealed class DotsConfig {
    [DefaultValue(Corner.BottomRight)] public Corner Start = Corner.BottomRight;
    [DefaultValue(Direction.Horizontal)] public Direction Direction = Direction.Horizontal;
    [DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] public float GroupTime = 5f;

    // Compatibility version < v3.2
    [DefaultValue(5f)] private float AnimationLength { set => ConfigHelper.MoveMember(value != 5f, c => GroupTime = value); }
}

public sealed class Dots : Display, IConfigurableDisplay<DotsConfig> {
    public static Dots Instance = null!;
}

public enum Direction { Vertical, Horizontal }
public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
public enum CacheStyle { None, Smart, Performances }
public enum GlowStyle { Off, Simple, Fancy }
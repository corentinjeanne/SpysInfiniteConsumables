using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Terraria.ModLoader;
using Terraria;
using Terraria.Localization;
using SpikysLib.Extensions;

namespace SPIC.Default.Displays;

public enum CountStyle { Sprite, Name }

public sealed class TooltipConfig {
    [LabelKey($"${Localization.Keys.Displays}.Tooltip.AddMissingLines")]
    [DefaultValue(true)] public bool AddMissingLines = true;
}

public interface ITooltipLineDisplay {
    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed);
}
public interface ICountToString {
    public string CountToString(int consumable, long count, long value);
}

public sealed class Tooltip : Display {

    public static Tooltip Instance = null!;
    public static TooltipConfig Config = null!;

    public static (TooltipLine, TooltipLineID?) GetTooltipLine(IInfinity infinity, Item item, int displayed) => infinity is ITooltipLineDisplay td ? td.GetTooltipLine(item, displayed) : DefaultTooltipLine(infinity);
    public static string CountToString(IGroup group, int consumable, long owned, long value) => group is ICountToString cs ? cs.CountToString(consumable, owned, value) : DefaultCount(owned, value);

    public static (TooltipLine, TooltipLineID?) DefaultTooltipLine(IInfinity infinity) => (new (infinity.Mod, infinity.Name, infinity.DisplayName.Value), null);
    public static string DefaultCount(long owned, long value) => owned == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value) : $"{owned}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value)}";

    public override int IconType => ItemID.Sign;
} 

public sealed class GlowConfig {
    [LabelKey($"${Localization.Keys.Displays}.Glow.FancyGlow")]
    [DefaultValue(true)] public bool FancyGlow = true;
    [LabelKey($"${Localization.Keys.Displays}.Glow.Intensity")]
    [DefaultValue(0.75f)] public float Intensity = 0.75f;
    [LabelKey($"${Localization.Keys.Displays}.Glow.AnimationLength")]
    [DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] public float AnimationLength = 2f;
}

public sealed class Glow : Display {

    public static Glow Instance = null!;
    public static GlowConfig Config = null!;

    public override int IconType => ItemID.FallenStar;
} 
public sealed class DotsConfig {
    [LabelKey($"${Localization.Keys.Displays}.Dots.Start")]
    [DefaultValue(Corner.BottomRight)] public Corner Start = Corner.BottomRight;
    [LabelKey($"${Localization.Keys.Displays}.Dots.Direction")]
    [DefaultValue(Direction.Horizontal)] public Direction Direction = Direction.Horizontal;
    [LabelKey($"${Localization.Keys.Displays}.Dots.AnimationLength")]
    [DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] public float AnimationLength = 5f;
}

public sealed class Dots : Display {

    public static Dots Instance = null!;
    public static DotsConfig Config = null!;

    public override int IconType => ItemID.WireBulb;
} 
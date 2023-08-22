using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using Terraria.Localization;

namespace SPIC.Default.Displays;

public enum CountStyle { Sprite, Name }

public sealed class TooltipConfig {
    [LabelKey($"${Localization.Keys.Displays}.Tooltip.AddMissingLines")]
    [DefaultValue(true)] public bool AddMissingLines = true;
}

public sealed class Tooltip : DisplayStatic<Tooltip> {

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = DisplayLoader.AddConfig<TooltipConfig>(this);
    }
    public override void Unload() {
        base.Unload();
        _lines.Clear();
        _strs.Clear();
    }

    public void RegisterTooltipLine(IInfinity infinity, TooltipLineGetter getter) => _lines[infinity] = getter;
    public void RegisterCountStr(IGroup group, CountToStringFn fn) => _strs[group] = fn;
    public (TooltipLine, TooltipLineID?) GetTooltipLine(IInfinity infinity, Item item, int displayed) => _lines.TryGetValue(infinity, out var getter) ? getter(item, displayed) : DefaultTooltipLine(infinity);
    public string CountToString(IGroup group, int consumable, long owned, long value) => _strs.TryGetValue(group, out var fn) ? fn(consumable, owned, value) : DefaultCount(owned, value);

    public static (TooltipLine, TooltipLineID?) DefaultTooltipLine(IInfinity infinity) => (new (infinity.Mod, infinity.Name, infinity.DisplayName.Value), null);
    public static string DefaultCount(long owned, long value) => owned == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value) : $"{owned}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value)}";

    public override int IconType => ItemID.Sign;

    public static Wrapper<TooltipConfig> Config = null!;

    private readonly Dictionary<IInfinity, TooltipLineGetter> _lines = new();
    private readonly Dictionary<IGroup, CountToStringFn> _strs = new();

    public delegate (TooltipLine, TooltipLineID?) TooltipLineGetter(Item item, int displayed);
    public delegate string CountToStringFn(int consumable, long owned, long value);
} 

public sealed class GlowConfig {
    [LabelKey($"${Localization.Keys.Displays}.Glow.FancyGlow")]
    [DefaultValue(true)] public bool FancyGlow = true;
    [LabelKey($"${Localization.Keys.Displays}.Glow.Intensity")]
    [DefaultValue(0.75f)] public float Intensity = 0.75f;
    [LabelKey($"${Localization.Keys.Displays}.Glow.AnimationLength")]
    [DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] public float AnimationLength = 2f;
}

public sealed class Glow : DisplayStatic<Glow> {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = DisplayLoader.AddConfig<GlowConfig>(this);
    }

    public override int IconType => ItemID.FallenStar;

    public static Wrapper<GlowConfig> Config = null!;
} 
public sealed class DotsConfig {
    [LabelKey($"${Localization.Keys.Displays}.Dots.Start")]
    [DefaultValue(Corner.BottomRight)] public Corner Start = Corner.BottomRight;
    [LabelKey($"${Localization.Keys.Displays}.Dots.Direction")]
    [DefaultValue(Direction.Horizontal)] public Direction Direction = Direction.Horizontal;
    [LabelKey($"${Localization.Keys.Displays}.Dots.AnimationLength")]
    [DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] public float AnimationLength = 5f;
}

public sealed class Dots : DisplayStatic<Dots> {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = DisplayLoader.AddConfig<DotsConfig>(this);
    }

    public static Wrapper<DotsConfig> Config = null!;

    public override int IconType => ItemID.WireBulb;
} 
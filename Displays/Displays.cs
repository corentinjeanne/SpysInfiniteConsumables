using System.ComponentModel;
using SPIC.Configs;
using Terraria.ModLoader.Config;

namespace SPIC.Displays;

public enum CountStyle { Sprite, Name }

public sealed class TooltipConfig {
    [DefaultValue(true)] public bool AddMissingLines = true;
    [DefaultValue(CountStyle.Name)] public CountStyle RequirementStyle = CountStyle.Name;
}

public sealed class Tooltip : DisplayStatic<Tooltip> {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = DisplayLoader.AddConfig<TooltipConfig>(this);
    }

    public static Wrapper<TooltipConfig> Config = null!;
} 

public sealed class GlowConfig {
    [DefaultValue(true)] public bool FancyGlow = true;
    [DefaultValue(0.75f)] public float Intensity = 0.75f;
    [DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] public float AnimationTime = 2f;
}

public sealed class Glow : DisplayStatic<Glow> {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = DisplayLoader.AddConfig<GlowConfig>(this);
    }

    public static Wrapper<GlowConfig> Config = null!;
} 
public sealed class DotsConfig {
    [DefaultValue(Corner.BottomRight)] public Corner Start = Corner.BottomRight;
    [DefaultValue(Direction.Horizontal)] public Direction Direction = Direction.Horizontal;
    [DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] public float AnimationTime = 5f;
}

public sealed class Dots : DisplayStatic<Dots> {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = DisplayLoader.AddConfig<DotsConfig>(this);
    }

    public static Wrapper<DotsConfig> Config = null!;
} 
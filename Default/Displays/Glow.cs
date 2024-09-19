using System.ComponentModel;
using Terraria.ModLoader.Config;
using System;
using SPIC.Configs;

namespace SPIC.Default.Displays;

public sealed class GlowConfig {
    [DefaultValue(true)] public bool FancyGlow = true;
    [DefaultValue(0.75f)] public float Intensity = 0.75f;
    [DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] public float AnimationLength = 2f;
}

public sealed class Glow : Display, IConfigProvider<GlowConfig> {
    public static Glow Instance = null!;

    public GlowConfig Config { get; set; } = null!;
}

using System.ComponentModel;
using Terraria.ModLoader.Config;
using SpikysLib.Configs;
using System;
using SPIC.Configs;

namespace SPIC.Default.Displays;

public sealed class DotsConfig {
    [DefaultValue(Corner.BottomRight)] public Corner Start = Corner.BottomRight;
    [DefaultValue(Direction.Horizontal)] public Direction Direction = Direction.Horizontal;
    [DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] public float GroupTime = 5f;

    // Compatibility version < v3.2
    [DefaultValue(5f)] private float AnimationLength { set => ConfigHelper.MoveMember(value != 5f, c => GroupTime = value); }
}

public sealed class Dots : Display, IConfigProvider<DotsConfig> {
    public static Dots Instance = null!;

    public DotsConfig Config { get; set; } = null!;
}

public enum Direction { Vertical, Horizontal }
public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
public enum CacheStyle { None, Smart, Performances }
public enum GlowStyle { Off, Simple, Fancy }
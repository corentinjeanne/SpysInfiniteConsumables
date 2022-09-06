using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using SPIC.ConsumableTypes;
using Terraria.ModLoader.Config;


namespace SPIC.Configs;
[Label("$Mods.SPIC.Configs.InfinityDisplay.name")]
public class InfinityDisplay : ModConfig {
    [Header("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.Infinities")]
    public bool toopltip_ShowInfinities;
    [Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.Categories")]
    public bool toopltip_ShowCategories;
    [Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.Requirements")]
    public bool toopltip_ShowRequirement;
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.MissingLines")]
    public bool toopltip_ShowMissingLines;
    [Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.ItemName")]
    public bool tooltip_UseItemName;
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.Color")]
    public bool tooltip_Color;

    [Header("$Mods.SPIC.Configs.InfinityDisplay.Glow.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Glow")]
    public bool glow_ShowGlow;
    [DefaultValue(1f), Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Intensity")]
    public float glow_Intensity;
    [DefaultValue(120), Range(0, 60*5), Slider, Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Pulse")]
    public int glow_PulseTime;

    [Header("$Mods.SPIC.Configs.InfinityDisplay.Dots.header")]
    [Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Dots")]
    public bool dots_ShowDots;
    [DefaultValue(typeof(Vector2), "0.9, 0.9"), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Start")]
    public Vector2 dots_Start;
    [DefaultValue(typeof(Vector2), "0.1, 0.9"), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.End")]
    public Vector2 dots_End;
    [DefaultValue(6), Range(1,6), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Count"), Tooltip("$Mods.SPIC.Configs.InfinityDisplay.Dots.t_count")]
    public int dots_PerPage;

    // [ColorNoAlpha, ColorHSLSlider]
    private Dictionary<string, Color> _colors = new();
    public Dictionary<string, Color> Colors {
        get => _colors;
        set {
            for (int i = 0; i < InfinityManager.ConsumableTypesCount; i++) {
                ConsumableType type = InfinityManager.ConsumableType(i);
                value.TryAdd(type.Name, type.DefaultColor());
            }
            _colors = value;
        }
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance;
}
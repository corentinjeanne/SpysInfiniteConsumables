using System.ComponentModel;
using Microsoft.Xna.Framework;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;


namespace SPIC.Configs;
[Label("$Mods.SPIC.Configs.InfinityDisplay.name")]
public class InfinityDisplay : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance => _instance ??= ModContent.GetInstance<InfinityDisplay>();
    private static InfinityDisplay _instance;

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
    [DefaultValue(CategoryManager.CategoryCount), Range(1,CategoryManager.CategoryCount), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Count"), Tooltip("$Mods.SPIC.Configs.InfinityDisplay.Dots.t_count")]
    public int dots_Count;


    [Header("$Mods.SPIC.Configs.InfinityDisplay.Colors.header")]
    [ColorNoAlpha, ColorHSLSlider, DefaultValue(typeof(Color), "0, 255, 200, 255"), Label("$Mods.SPIC.Configs.InfinityDisplay.Colors.Consumables")]
    public Color color_Consumables;
    [ColorNoAlpha, ColorHSLSlider, DefaultValue(typeof(Color), "0, 180, 60, 255"), Label("$Mods.SPIC.Configs.InfinityDisplay.Colors.Ammo")]
    public Color color_Ammo;
    [ColorNoAlpha, ColorHSLSlider, DefaultValue(typeof(Color), "125, 80, 0, 255"), Label("$Mods.SPIC.Configs.InfinityDisplay.Colors.Placeables")]
    public Color color_Placeables;
    [ColorNoAlpha, ColorHSLSlider, DefaultValue(typeof(Color), "150, 100, 255, 255"), Label("$Mods.SPIC.Configs.InfinityDisplay.Colors.Bags")]
    public Color color_Bags;
    [ColorNoAlpha, ColorHSLSlider, DefaultValue(typeof(Color), "255, 120, 187, 255"), Label("$Mods.SPIC.Configs.InfinityDisplay.Colors.Materials")]
    public Color color_Materials;
    [ColorNoAlpha, ColorHSLSlider, DefaultValue(typeof(Color), "255, 255, 70, 255"), Label("$Mods.SPIC.Configs.InfinityDisplay.Colors.Currencies")]
    public Color color_Currencies;
}
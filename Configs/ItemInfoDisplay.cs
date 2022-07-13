using System.ComponentModel;
using Microsoft.Xna.Framework;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;


namespace SPIC.Configs;

[Label("$Mods.SPIC.Configs.Tooltip.name")]
public class IntemInfiDisplay : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static IntemInfiDisplay Instance => _instance ??= ModContent.GetInstance<IntemInfiDisplay>();
    private static IntemInfiDisplay _instance;

    [Header("$Mods.SPIC.Configs.Tooltip.Infos.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Tooltip.Infos.Infinities")]
    public bool ShowInfinities;
    [Label("$Mods.SPIC.Configs.Tooltip.Infos.Categories")]
    public bool ShowCategories;
    [Label("$Mods.SPIC.Configs.Tooltip.Infos.Requirements")]
    public bool ShowRequirement;

    
    [Header("$Mods.SPIC.Configs.Tooltip.Colors.header")]
    [ColorNoAlpha, DefaultValue(typeof(Color), "0, 200, 200, 255"), Label("$Mods.SPIC.Configs.Tooltip.Colors.Consumables")]
    public Color color_Consumables;
    [ColorNoAlpha, DefaultValue(typeof(Color), "0, 180, 60, 255"), Label("$Mods.SPIC.Configs.Tooltip.Colors.Ammo")]
    public Color color_Ammo;
    [ColorNoAlpha, DefaultValue(typeof(Color), "200, 50, 0, 255"), Label("$Mods.SPIC.Configs.Tooltip.Colors.Placeables")]
    public Color color_Placeable;
    [ColorNoAlpha, DefaultValue(typeof(Color), "150, 100, 255, 255"), Label("$Mods.SPIC.Configs.Tooltip.Colors.Bags")]
    public Color color_Bags;
    [ColorNoAlpha, DefaultValue(typeof(Color), "250, 150, 150, 255"), Label("$Mods.SPIC.Configs.Tooltip.Colors.Materials")]
    public Color color_Materials;
    [ColorNoAlpha, DefaultValue(typeof(Color), "255, 150, 100, 255"), Label("$Mods.SPIC.Configs.Tooltip.Colors.Currencies")]
    public Color color_Currencies;
}
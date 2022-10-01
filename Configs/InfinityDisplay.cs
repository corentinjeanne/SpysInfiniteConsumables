using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using SPIC.ConsumableTypes;
using SPIC.Configs.UI;
using Newtonsoft.Json;

namespace SPIC.Configs;

[Label("$Mods.SPIC.Configs.InfinityDisplay.name")]
public class InfinityDisplay : ModConfig {
    [Header("$Mods.SPIC.Configs.InfinityDisplay.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.General.Infinities")]
    public bool general_ShowInfinities;
    [Label("$Mods.SPIC.Configs.InfinityDisplay.General.Categories")]
    public bool general_ShowCategories;
    [Label("$Mods.SPIC.Configs.InfinityDisplay.General.Requirements")]
    public bool general_ShowRequirement;

    [Header("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.Tooltip")]
    public bool toopltip_ShowTooltip;
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.MissingLines")]
    public bool toopltip_AddMissingLines;
    [Label("$Mods.SPIC.Configs.InfinityDisplay.Tooltip.ItemName")]
    public bool tooltip_UseItemName;

    [Header("$Mods.SPIC.Configs.InfinityDisplay.Glow.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Glow")]
    public bool glow_ShowGlow;
    [DefaultValue(1f), Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Intensity")]
    public float glow_Intensity;
    [DefaultValue(120), Range(0, 60*5), Slider, Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Pulse")]
    public int glow_PulseTime;

    public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}

    [Header("$Mods.SPIC.Configs.InfinityDisplay.Dots.header")]
    [Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Dots")]
    public bool dots_ShowDots;
    [DefaultValue(Corner.BottomRight), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Start")]
    public Corner dots_Start;
    [DefaultValue(false), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Vertical")]
    public bool dots_vertical;
    [DefaultValue(6), Range(1,6), Label("$Mods.SPIC.Configs.InfinityDisplay.Dots.Count"), Tooltip("$Mods.SPIC.Configs.InfinityDisplay.Dots.t_count")]
    public int dots_PerPage;
    [DefaultValue(120), Range(0, 60 * 5), Slider, Label("$Mods.SPIC.Configs.InfinityDisplay.Glow.Pulse")]
    public int dot_PulseTime;

    [Header("$Mods.SPIC.Configs.InfinityDisplay.Colors.header")]
    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys, ColorNoAlpha, ColorHSLSlider]
    public Dictionary<ConsumableTypeDefinition, Color> Colors {
        get => _colors;
        set {
            foreach(IColorable type in InfinityManager.ConsumableTypes<IColorable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true))
                value.TryAdd(type.ToDefinition(), type.DefaultColor);
            _colors = value;
        }
    }
    private Dictionary<ConsumableTypeDefinition, Color> _colors = new();

    [JsonIgnore]
    public DisplayFlags DisplayFlags {
        get {
            DisplayFlags flags = 0;
            if (general_ShowCategories) flags |= DisplayFlags.Category;
            if (general_ShowRequirement) flags |= DisplayFlags.Requirement;
            if (general_ShowInfinities) flags |= DisplayFlags.Infinity;
            return flags;
        }
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance;

    public void UpdateProperties() {
        this.SaveConfig();
    }
}
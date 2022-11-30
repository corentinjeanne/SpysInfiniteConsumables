using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Config.UI;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace SPIC.Config;

[Label("$Mods.SPIC.Config.InfinityDisplay.name")]
public class InfinityDisplay : ModConfig {
    [Header("$Mods.SPIC.Config.InfinityDisplay.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Config.InfinityDisplay.General.Infinities")]
    public bool general_ShowInfinities;
    [DefaultValue(true), Label("$Mods.SPIC.Config.InfinityDisplay.General.Requirements")]
    public bool general_ShowRequirement;
    [Label("$Mods.SPIC.Config.InfinityDisplay.General.Categories")]
    public bool general_ShowCategories;

    [Header("$Mods.SPIC.Config.InfinityDisplay.Tooltip.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Config.InfinityDisplay.Tooltip.Tooltip")]
    public bool toopltip_ShowTooltip;
    [DefaultValue(true), Label("$Mods.SPIC.Config.InfinityDisplay.Tooltip.MissingLines")]
    public bool toopltip_AddMissingLines;
    [DefaultValue(CountStyle.Name), Label("$Mods.SPIC.Config.InfinityDisplay.Tooltip.Style")]
    public CountStyle tooltip_RequirementStyle;

    [Header("$Mods.SPIC.Config.InfinityDisplay.Glow.header")]
    [Label("$Mods.SPIC.Config.InfinityDisplay.Glow.Glow")]
    public bool glow_ShowGlow;
    [DefaultValue(1f), Label("$Mods.SPIC.Config.InfinityDisplay.Glow.Intensity")]
    public float glow_Intensity;
    [DefaultValue(120), Range(0, 60*5), Slider, Label("$Mods.SPIC.Config.InfinityDisplay.Glow.Pulse")]
    public int glow_PulseTime;

    [Header("$Mods.SPIC.Config.InfinityDisplay.Dots.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Config.InfinityDisplay.Dots.Dots")]
    public bool dots_ShowDots;
    [DefaultValue(Corner.BottomRight), Label("$Mods.SPIC.Config.InfinityDisplay.Dots.Start")]
    public Corner dots_Start;
    [DefaultValue(Direction.Horizontal), Label("$Mods.SPIC.Config.InfinityDisplay.Dots.Direction")]
    public Direction dots_Direction;
    [Range(0,8), Label("$Mods.SPIC.Config.InfinityDisplay.Dots.Count"), Tooltip("$Mods.SPIC.Config.InfinityDisplay.Dots.t_count")]
    public int dots_Count;
    [DefaultValue(60), Range(0, 60 * 5), Slider, Label("$Mods.SPIC.Config.InfinityDisplay.Glow.Pulse")]
    public int dot_PulseTime;

    [Header("$Mods.SPIC.Config.InfinityDisplay.Colors.header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys, ColorNoAlpha, ColorHSLSlider]
    public Dictionary<ConsumableTypeDefinition, Color> Colors {
        get => _colors;
        set {
            _colors.Clear();
            foreach((ConsumableTypeDefinition def, Color color) in value){
                if (def.IsUnloaded && ModLoader.HasMod(def.Mod)) continue;
                _colors.Add(def, color);
            }
            foreach(IColorable type in InfinityManager.ConsumableGroups<IColorable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true))
                _colors.TryAdd(type.ToDefinition(), type.DefaultColor);
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, Color> _colors = new();


    public enum CountStyle { Sprite, Name }
    public enum Direction {Vertical, Horizontal}
    public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
    [JsonIgnore]
    public Globals.DisplayFlags DisplayFlags {
        get {
            Globals.DisplayFlags flags = 0;
            if (general_ShowCategories) flags |= Globals.DisplayFlags.Category;
            if (general_ShowRequirement) flags |= Globals.DisplayFlags.Requirement;
            if (general_ShowInfinities) flags |= Globals.DisplayFlags.Infinity;
            return flags;
        }
    }


    public override ConfigScope Mode => ConfigScope.ClientSide;
#nullable disable
    public static InfinityDisplay Instance;
#nullable restore

}
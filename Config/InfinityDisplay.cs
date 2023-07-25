using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using Newtonsoft.Json;

namespace SPIC.Configs;

public class InfinityDisplay : ModConfig {
    [Header($"${Localization.Keys.InfinityDisplay}.General.Header")]
    [DefaultValue(true)]
    public bool general_ShowInfinities;
    [DefaultValue(true)]
    public bool general_ShowRequirement;
    public bool general_ShowCategories;
    [DefaultValue(WelcomMessageFrequency.OncePerUpdate)]
    public WelcomMessageFrequency general_welcomeMessage;
    [JsonProperty, DefaultValue("")] internal string general_lastLogs = "";

    [Header($"${Localization.Keys.InfinityDisplay}.Tooltip.Header")]
    [DefaultValue(true)]
    public bool toopltip_ShowTooltip;
    [DefaultValue(true)]
    public bool toopltip_AddMissingLines;
    [DefaultValue(CountStyle.Name)]
    public CountStyle tooltip_RequirementStyle;
    public bool tooltip_ShowMixed;

    [Header($"${Localization.Keys.InfinityDisplay}.Glow.Header")]
    public bool glow_ShowGlow;
    [DefaultValue(1f)]
    public float glow_Intensity;
    [DefaultValue(2), Range(0, 5), Slider]
    public int glow_GroupTime;

    [Header($"${Localization.Keys.InfinityDisplay}.Dots.Header")]
    [DefaultValue(true)]
    public bool dots_ShowDots;
    [DefaultValue(Corner.BottomRight)]
    public Corner dots_Start;
    [DefaultValue(Direction.Horizontal)]
    public Direction dots_Direction;
    [Range(1,Globals.InfinityDisplayItem.MaxDots), DefaultValue(Globals.InfinityDisplayItem.MaxDots)]
    public int dots_Count;
    [DefaultValue(5), Range(1,10), Slider]
    public int dot_PageTime;

    [Header($"${Localization.Keys.InfinityDisplay}.Colors.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ColorNoAlpha, ColorHSLSlider]
    public Dictionary<ModGroupDefinition, Color> Colors {
        get => _colors;
        set {
            _colors.Clear();
            foreach (IModGroup group in InfinityManager.Groups) {
                ModGroupDefinition def = new(group);
                if (value.TryGetValue(def, out Color color)) _colors[def] = color;
                else _colors[def] = group.DefaultColor;
            } 
        }
    }

    private readonly Dictionary<ModGroupDefinition, Color> _colors = new();


    public enum CountStyle { Sprite, Name }
    public enum Direction {Vertical, Horizontal}
    public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
    public enum WelcomMessageFrequency {Never, OncePerUpdate, Always}

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;
}
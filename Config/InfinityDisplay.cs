using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using Newtonsoft.Json;

namespace SPIC.Configs;

public class InfinityDisplay : ModConfig {
    [Header("General")]
    [DefaultValue(true)]
    public bool general_ShowInfinities;
    [DefaultValue(true)]
    public bool general_ShowRequirement;
    public bool general_ShowCategories;
    [DefaultValue(WelcomMessageFrequency.OncePerUpdate)]
    public WelcomMessageFrequency general_welcomeMessage;
    [JsonProperty, DefaultValue("")] internal string general_lastLogs = "";

    [Header("Tooltip")]
    [DefaultValue(true)]
    public bool toopltip_ShowTooltip;
    [DefaultValue(true)]
    public bool toopltip_AddMissingLines;
    [DefaultValue(CountStyle.Name)]
    public CountStyle tooltip_RequirementStyle;

    [Header("Glow")]
    public bool glow_ShowGlow;
    [DefaultValue(1f)]
    public float glow_Intensity;
    [DefaultValue(2), Range(0, 5), Slider]
    public int glow_InfinityTime;

    [Header("Dots")]
    [DefaultValue(true)]
    public bool dots_ShowDots;
    [DefaultValue(Corner.BottomRight)]
    public Corner dots_Start;
    [DefaultValue(Direction.Horizontal)]
    public Direction dots_Direction;
    [Range(1, Globals.InfinityDisplayItem.MaxDots), DefaultValue(Globals.InfinityDisplayItem.MaxDots)]
    public int dots_Count;
    [DefaultValue(5), Range(1, 10), Slider]
    public int dot_PageTime;

    [Header("Colors")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<GroupDefinition, GroupColors> Colors {
        get => _colors;
        set {
            foreach (IGroup group in InfinityManager.Groups) {
                GroupDefinition def = new(group);
                if (value.TryGetValue(def, out GroupColors? colors)) colors.SetGroup(group);
                else value[def] = colors = new(group);
                group.Colors = colors;
            }
            _colors = value;
        }
    }
    private Dictionary<GroupDefinition, GroupColors> _colors = new();

    public enum CountStyle { Sprite, Name }
    public enum Direction {Vertical, Horizontal}
    public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
    public enum WelcomMessageFrequency {Never, OncePerUpdate, Always}

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;
}
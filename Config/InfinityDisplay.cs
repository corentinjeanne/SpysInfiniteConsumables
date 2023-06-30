using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.UI;
using Newtonsoft.Json;
using Terraria.ModLoader;

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
    [DefaultValue(120), Range(0, 60*5), Slider]
    public int glow_PulseTime;

    [Header($"${Localization.Keys.InfinityDisplay}.Dots.Header")]
    [DefaultValue(true)]
    public bool dots_ShowDots;
    [DefaultValue(Corner.BottomRight)]
    public Corner dots_Start;
    [DefaultValue(Direction.Horizontal)]
    public Direction dots_Direction;
    [Range(1,Globals.InfinityDisplayItem.MaxDots), DefaultValue(Globals.InfinityDisplayItem.MaxDots)]
    public int dots_Count;
    [DefaultValue(60), Range(0, 60 * 5), Slider]
    public int dot_PulseTime;

    [Header($"${Localization.Keys.InfinityDisplay}.Colors.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ColorNoAlpha, ColorHSLSlider]
    public Dictionary<ConsumableGroupDefinition, Color> Colors {
        get => _colors;
        set {
            _colors.Clear();
            foreach((ConsumableGroupDefinition def, Color color) in value){
                if (def.IsUnloaded && ModLoader.HasMod(def.Mod)) continue;
                _colors.Add(def, color);
            }
            foreach(IColorable group in InfinityManager.ConsumableGroups<IColorable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true))
                _colors.TryAdd(group.ToDefinition(), group.DefaultColor);
                
        }
    }

    private readonly Dictionary<ConsumableGroupDefinition, Color> _colors = new();


    public enum CountStyle { Sprite, Name }
    public enum Direction {Vertical, Horizontal}
    public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
    public enum WelcomMessageFrequency {Never, OncePerUpdate, Always}
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
    public static InfinityDisplay Instance = null!;
}
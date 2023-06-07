using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.UI;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace SPIC.Configs;

[Label($"${Localization.Keys.InfinityDisplay}.Name")]
public class InfinityDisplay : ModConfig {
    [Header($"${Localization.Keys.InfinityDisplay}.General.Header")]
    [DefaultValue(true), Label($"${Localization.Keys.InfinityDisplay}.General.Infinities.Label")]
    public bool general_ShowInfinities;
    [DefaultValue(true), Label($"${Localization.Keys.InfinityDisplay}.General.Requirements.Label")]
    public bool general_ShowRequirement;
    [Label($"${Localization.Keys.InfinityDisplay}.General.Categories.Label")]
    public bool general_ShowCategories;
    [DefaultValue(WelcomMessageFrequency.OncePerUpdate)]
    [Label($"${Localization.Keys.InfinityDisplay}.General.Welcome.Label")]
    public WelcomMessageFrequency general_welcomeMessage;
    [JsonProperty, DefaultValue("0.0.0.0")] internal string _lastVersionMessage = "";

    [Header($"${Localization.Keys.InfinityDisplay}.Tooltip.Header")]
    [DefaultValue(true), Label($"${Localization.Keys.InfinityDisplay}.Tooltip.Tooltip.Label")]
    public bool toopltip_ShowTooltip;
    [DefaultValue(true), Label($"${Localization.Keys.InfinityDisplay}.Tooltip.MissingLines.Label")]
    public bool toopltip_AddMissingLines;
    [DefaultValue(CountStyle.Name), Label($"${Localization.Keys.InfinityDisplay}.Tooltip.Style.Label")]
    public CountStyle tooltip_RequirementStyle;
    [Label($"${Localization.Keys.InfinityDisplay}.Tooltip.Mixed.Label")]
    public bool tooltip_ShowMixed;

    [Header($"${Localization.Keys.InfinityDisplay}.Glow.Header")]
    [Label($"${Localization.Keys.InfinityDisplay}.Glow.Glow.Label")]
    public bool glow_ShowGlow;
    [DefaultValue(1f), Label($"${Localization.Keys.InfinityDisplay}.Glow.Intensity.Label")]
    public float glow_Intensity;
    [DefaultValue(120), Range(0, 60*5), Slider, Label($"${Localization.Keys.InfinityDisplay}.Glow.Pulse.Label")]
    public int glow_PulseTime;

    [Header($"${Localization.Keys.InfinityDisplay}.Dots.Header")]
    [DefaultValue(true), Label($"${Localization.Keys.InfinityDisplay}.Dots.Dots.Label")]
    public bool dots_ShowDots;
    [DefaultValue(Corner.BottomRight), Label($"${Localization.Keys.InfinityDisplay}.Dots.Start.Label")]
    public Corner dots_Start;
    [DefaultValue(Direction.Horizontal), Label($"${Localization.Keys.InfinityDisplay}.Dots.Direction.Label")]
    public Direction dots_Direction;
    [Range(1,Globals.InfinityDisplayItem.MaxDots), DefaultValue(Globals.InfinityDisplayItem.MaxDots), Label($"${Localization.Keys.InfinityDisplay}.Dots.Count.Label"), Tooltip($"${Localization.Keys.InfinityDisplay}.Dots.Count.Tooltip")]
    public int dots_Count;
    [DefaultValue(60), Range(0, 60 * 5), Slider, Label($"${Localization.Keys.InfinityDisplay}.Glow.Pulse.Label")]
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
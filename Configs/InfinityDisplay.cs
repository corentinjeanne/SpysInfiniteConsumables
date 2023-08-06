using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using Newtonsoft.Json;
using System;

namespace SPIC.Configs;

public sealed class InfinityDisplay : ModConfig {
    [Header("General")]
    [Obsolete("Use ShowInfinities instead", true), DefaultValue(true)] public bool general_ShowInfinities;
    [Obsolete("Use ShowRequirement instead", true), DefaultValue(true)] public bool general_ShowRequirement;
    [Obsolete("Use ShowInfo instead", true)] public bool general_ShowInfo;
    [Obsolete("Use ExclusiveDisplay instead", true), DefaultValue(true)] public bool general_ExclusiveDisplay;

    [DefaultValue(true)] public bool ShowInfinities;
    [DefaultValue(true)] public bool ShowRequirement;
    public bool ShowInfo;
    [DefaultValue(true)] public bool ExclusiveDisplay;
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<DisplayDefinition, bool> Display {
        get => _displays;
        set {
            foreach (Display display in DisplayLoader.Displays) {
                DisplayDefinition def = new(display.Mod.Name, display.Name);
                display.Enabled = value[def] = value.GetValueOrDefault(def, display.Enabled);
            }
            _displays = value;
        }
    }
    [Obsolete("Use WelcomeMessage instead", true)] public WelcomMessageFrequency general_welcomeMessage;    
    [DefaultValue(WelcomMessageFrequency.OncePerUpdate)]
    public WelcomMessageFrequency WelcomeMessage;    
    
    [Header("Configs")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<DisplayDefinition, Wrapper> Configs {
        get => _configs;
        set {
            foreach ((Display display, Wrapper wrapper) in DisplayLoader.Configs) {
                DisplayDefinition def = new(display.Mod.Name, display.Name);
                value[def] = value.TryGetValue(def, out var c) ? c.ChangeType(wrapper.Member.Type) : Wrapper.From(wrapper.Member.Type);
                wrapper.Value = value[def].Value;
            }
            _configs = value;
        }
    }

    [Header("Tooltip")]
    [Obsolete($"Use SPIC.Displays.Tooltip.Instance.Enabled instead", true), DefaultValue(true)] public bool toopltip_ShowTooltip;
    [Obsolete($"Use SPIC.Displays.Tooltip.Instance.Config.Value.AddMissingLines instead", true), DefaultValue(true)] public bool toopltip_AddMissingLines;
    [Obsolete($"Use SPIC.Displays.Tooltip.Instance.Config.Value.RequirementStyle instead", true), DefaultValue(Displays.CountStyle.Name)] public Displays.CountStyle tooltip_RequirementStyle;

    [Header("Glow")]
    [Obsolete($"Use SPIC.Displays.Glow.Instance.Enabled instead", true), DefaultValue(Glow.Fancy)] public Glow glow_ShowGlow;
    [Obsolete($"Use SPIC.Displays.Glow.Instance.Config.Value.Intensity instead", true), DefaultValue(0.75f)] public float glow_Intensity;
    [Obsolete($"Use SPIC.Displays.Glow.Instance.Config.Value.AnimationLength instead", true), DefaultValue(2), Range(1f, 5f), Increment(0.1f)] public float glow_InfinityTime;

    [Header("Dots")]
    [Obsolete($"Use SPIC.Displays.Dots.Instance.Enabled instead", true), DefaultValue(true)] public bool dots_ShowDots;
    [Obsolete($"Use SPIC.Displays.Dots.Instance.Config.Value.Start instead", true), DefaultValue(Corner.BottomRight)] public Corner dots_Start;
    [Obsolete($"Use SPIC.Displays.Dots.Instance.Config.Value.Direction instead", true), DefaultValue(Direction.Horizontal)] public Direction dots_Direction;
    [Obsolete($"Use SPIC.Displays.Dots.Instance.Config.Value.AnimationLength instead", true), DefaultValue(5), Range(1f, 10f),  Increment(0.1f)] public float dot_PageTime;

    [Header("Colors")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))] public Dictionary<GroupDefinition, GroupColors> Colors {
        get => _colors;
        set {
            foreach (IGroup group in InfinityManager.Groups) {
                GroupDefinition def = new(group);
                value[def] = value.GetValueOrDefault(def, new());
                group.LoadConfig(value[def]);
            }
            _colors = value;
        }
    }

    [Header("Performances")]
    [DefaultValue(CacheStyle.Smart)] public CacheStyle Cache { get; set; }
    [DefaultValue(1), Range(0, 1000)] public int CacheRefreshDelay { get; set; }

    [JsonProperty, DefaultValue("")] internal string general_lastLogs = "";
    private Dictionary<DisplayDefinition, bool> _displays = new();
    private Dictionary<DisplayDefinition, Wrapper> _configs = new();
    private Dictionary<GroupDefinition, GroupColors> _colors = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;
}

public enum Direction {Vertical, Horizontal}
public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
public enum WelcomMessageFrequency {Never, OncePerUpdate, Always}
public enum CacheStyle {None, Smart, Performances }
public enum Glow { Off, Simple, Fancy }
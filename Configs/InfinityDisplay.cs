using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using SPIC.Default.Displays;

namespace SPIC.Configs;

public sealed class InfinityDisplay : ModConfig {
    [Header("Displays")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<DisplayDefinition, bool> Display {
        get => _displays;
        set {
            foreach (Display display in DisplayLoader.Displays) {
                DisplayDefinition def = new(display.Mod.Name, display.Name);
                display.Enabled = value[def] = value.GetValueOrDefault(def, display.DefaultState());
            }
            _displays = value;
        }
    }
    
    [Header("General")]
    [DefaultValue(true)] public bool ShowInfinities;
    [DefaultValue(true)] public bool ShowRequirement;
    public bool ShowInfo;
    [DefaultValue(true)] public bool ShowExclusiveDisplay;
    [DefaultValue(true)] public bool ShowAlternateDisplays;
    
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


    [Header("Changelog")]
    public Text? ChangeLog { get; set; }
    [JsonProperty, DefaultValue("")] internal string version = "";

    private Dictionary<DisplayDefinition, bool> _displays = new();
    private Dictionary<DisplayDefinition, Wrapper> _configs = new();
    private Dictionary<GroupDefinition, GroupColors> _colors = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;

    [JsonProperty, MovedTo("ShowInfinities"), DefaultValue(true)] private bool general_ShowInfinities;
    [JsonProperty, MovedTo("ShowRequirement"), DefaultValue(true)] private bool general_ShowRequirement;
    [JsonProperty, MovedTo("ShowInfo")] private bool general_ShowInfo;
    [JsonProperty, MovedTo("ShowExclusiveDisplay"), DefaultValue(true)] private bool general_ExclusiveDisplay;
    [JsonProperty, MovedTo("WelcomeMessage")] private WelcomMessageFrequency general_welcomeMessage;
    [JsonProperty, MovedTo(typeof(Tooltip), "Instance.Config.Value.AddMissingLines"), DefaultValue(true)] private bool toopltip_AddMissingLines;
    [JsonProperty, MovedTo(typeof(Tooltip), "Instance.Config.Value.RequirementStyle"), DefaultValue(CountStyle.Name)] private CountStyle tooltip_RequirementStyle;
    [JsonProperty, MovedTo(typeof(Glow), "Instance.Config.Value.Intensity"), DefaultValue(0.75f)] private float glow_Intensity;
    [JsonProperty, MovedTo(typeof(Glow), "Instance.Config.Value.AnimationLength"), DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] private float glow_InfinityTime;
    [JsonProperty, MovedTo(typeof(Dots), "Instance.Config.Value.Start"), DefaultValue(Corner.BottomRight)] private Corner dots_Start;
    [JsonProperty, MovedTo(typeof(Dots), "Instance.Config.Value.Direction"), DefaultValue(Direction.Horizontal)] private Direction dots_Direction;
    [JsonProperty, MovedTo(typeof(Dots), "Instance.Config.Value.AnimationLength"), DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] private float dot_PageTime;
    [JsonProperty, MovedTo("version"), DefaultValue("")] internal string general_lastLogs = "";
}

public enum Direction {Vertical, Horizontal}
public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
public enum WelcomMessageFrequency {Never, OncePerUpdate, Always}
public enum CacheStyle {None, Smart, Performances }
public enum Glow { Off, Simple, Fancy }
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


    [Header("Version")]
    public Text? Info { get; set; }
    public Text? Changelog { get; set; }
    [JsonProperty, DefaultValue("")] internal string version = "";

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
public enum GlowStyle { Off, Simple, Fancy }
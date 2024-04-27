using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using SpikysLib.Configs;

namespace SPIC.Configs;

public sealed class InfinityDisplay : ModConfig {
    [Header("General")]
    [DefaultValue(true)] public bool ShowInfinities;
    [DefaultValue(true)] public bool ShowRequirement;
    public bool ShowInfo;
    [DefaultValue(true)] public bool ShowExclusiveDisplay;
    [DefaultValue(true)] public bool ShowAlternateDisplays;

    [Header("Displays")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<DisplayDefinition, object> Displays {
        get => _displays;
        set {
            _displays = value;
            DisplayLoader.LoadConfig(this);
        }
    }

    [JsonProperty] internal Dictionary<DisplayDefinition, bool> Display = new(); // Compatibility version < v3.1.1
    [JsonProperty] internal Dictionary<DisplayDefinition, Wrapper> Configs = new(); // Compatibility version < v3.1.1

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

    private Dictionary<DisplayDefinition, object> _displays = new();
    private Dictionary<GroupDefinition, GroupColors> _colors = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;
}

public enum Direction {Vertical, Horizontal}
public enum Corner {TopLeft, TopRight, BottomLeft, BottomRight}
public enum WelcomMessageFrequency {Never, OncePerUpdate, Always}
public enum CacheStyle {None, Smart, Performances }
public enum GlowStyle { Off, Simple, Fancy }
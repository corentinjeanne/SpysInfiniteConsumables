using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
namespace SPIC.Configs;

public sealed class InfinityDisplay : ModConfig {

    [Header("General")]
    [DefaultValue(true)] public bool exclusiveDisplay;
    [DefaultValue(true)] public bool alternateDisplays;

    [Header("Displays")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(EntityDefinitionValueWrapper<,>))]
    public Dictionary<DisplayDefinition, Toggle<object>> displays = [];

    [Header("Infinities")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))]
    public Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>> infinities = [];

    [Header("Performances")]
    public bool disableCache;
    [DefaultValue(1), Range(0, 9999)] public int displayRefresh;


    // TODO Compatibility version < v4.0
    // [DefaultValue(true)] private bool ShowRequirement { set => ConfigHelper.MoveMember(value != true, _ => {}); }
    // private bool ShowInfo { set => ConfigHelper.MoveMember(value != false, _ => {}); }
    [JsonProperty, DefaultValue(true)] private bool ShowExclusiveDisplay { set => ConfigHelper.MoveMember(value != true, _ => exclusiveDisplay = value); }
    [JsonProperty, DefaultValue(true)] private bool ShowAlternateDisplays { set => ConfigHelper.MoveMember(value != true, _ => alternateDisplays = value); }
    // [JsonProperty] private Dictionary<DisplayDefinition, Toggle<object>> Displays { set => ConfigHelper.MoveMember(value is not null, _ => {}); }
    // [JsonProperty] private Dictionary<DisplayDefinition, object>? Colors { set => ConfigHelper.MoveMember(value is not null, _ => {}); }
    [JsonProperty, DefaultValue(CacheStyle.Smart)] private CacheStyle Cache { set => ConfigHelper.MoveMember(value == CacheStyle.None, _ => disableCache = true); }
    [JsonProperty, DefaultValue(1)] private int CacheRefreshDelay { set => ConfigHelper.MoveMember(value != 1, _ => displayRefresh = value); }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context) {
        foreach (Display display in DisplayLoader.Displays) LoadConfig(display, displays.GetOrAdd(new(display), new Toggle<object>(display.DefaultEnabled) { Value = null! }));
        foreach (IInfinity infinity in InfinityLoader.ConsumableInfinities) LoadConfig(infinity, infinities.GetOrAdd(new(infinity), _ => new(infinity.Defaults.Color)));
    }
    public static void LoadConfig(Display display, Toggle<object> config) {
        Type configType = display is IConfigProvider c1 ? c1.ConfigType : typeof(Empty);
        object? oldConfig = config.Value;
        config.Value = oldConfig switch {
            null => JsonConvert.DeserializeObject("{}", configType, ConfigManager.serializerSettings)!,
            JToken token => token.ToObject(configType)!,
            _ => oldConfig
        };
        if (display is IConfigProvider c2) {
            c2.Config = config.Value;
            c2.OnLoaded(oldConfig is null);
        }
        display.Enabled = config.Key;
    }
    public static void LoadConfig(IInfinity infinity, NestedValue<Color, Dictionary<string, object>> config) {
        (var oldConfigs, config.Value) = (config.Value, []);
        infinity.Color = config.Key;
        foreach ((var key, var provider) in _configs.GetValueOrDefault(infinity, [])) {
            Type configType = provider.ConfigType;
            object? oldConfig = oldConfigs.GetValueOrDefault(key, null!);
            config.Value[key] = oldConfig switch {
                null => JsonConvert.DeserializeObject("{}", configType, ConfigManager.serializerSettings)!,
                JToken token => token.ToObject(configType)!,
                _ => oldConfig
            };
            provider.ClientConfig = config.Value[key];
            provider.OnLoadedClient(oldConfig is null);
        }
    }
    public static void AddConfig(IInfinity infinity, string key, IClientConfigProvider config) => _configs.GetOrAdd(infinity, []).Add((key, config));
    private static readonly Dictionary<IInfinity, List<(string key, IClientConfigProvider)>> _configs = [];
}

[Flags] public enum DisplayedInfinities { Infinities = 0b01, Consumable = 0b10, Both = 0b11 }
public enum CacheStyle { None, Smart, Performances }
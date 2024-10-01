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
    [CustomModConfigItem(typeof(DictionaryValuesElement))]
    public Dictionary<DisplayDefinition, Toggle<object>> displays = [];

    [Header("Infinities")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))]
    public Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<ProviderDefinition, object>>> infinities = [];

    [Header("Performances")]
    public bool disableCache;
    [DefaultValue(1), Range(0, 9999)] public int displayRefresh;

    // Compatibility version < v4.0
    [JsonProperty, DefaultValue(true)] private bool ShowRequirement { set => ConfigHelper.MoveMember(value != true, _ => {
        ((JObject)displays[new("SPIC/Tooltip")].Value)["displayRequirement"] = value;
        ((JObject)displays[new("SPIC/Dots")].Value)["displayRequirement"] = value;
    }); }
    [JsonProperty] private bool ShowInfo { set => ConfigHelper.MoveMember(value != false, _ => {
        ((JObject)displays[new(nameof(SPIC), nameof(Default.Displays.Tooltip))].Value)[nameof(Default.Displays.TooltipConfig.displayDebug)] = value;
    }); }
    [JsonProperty, DefaultValue(true)] private bool ShowExclusiveDisplay { set => ConfigHelper.MoveMember(value != true, _ => exclusiveDisplay = value); }
    [JsonProperty, DefaultValue(true)] private bool ShowAlternateDisplays { set => ConfigHelper.MoveMember(value != true, _ => alternateDisplays = value); }
    [JsonProperty] private Dictionary<InfinityDefinition, GroupColors>? Colors { set => ConfigHelper.MoveMember(value is not null, _ => {
        foreach ((var d, var colors) in value!) {
            if (d.ToString() == "SPIC/Currencies") {
                Default.Infinities.Currency.PortClientConfig(infinities, colors);
                continue;
            }
            Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<ProviderDefinition, object>>> dictionary = [];
            foreach ((var infinity, var color) in colors.Colors) dictionary[infinity] = new(color);
            InfinityDefinition def = d.ToString() == "SPIC/Items" ? new("SPIC/ConsumableItem") : d;
            infinities.GetOrAdd(def, () => new(default)).Value[ProviderDefinition.Infinities] = new ClientConsumableInfinities() { infinities = dictionary };
        }
    }); }
    [JsonProperty, DefaultValue(CacheStyle.Smart)] private CacheStyle Cache { set => ConfigHelper.MoveMember(value == CacheStyle.None, _ => disableCache = true); }
    [JsonProperty, DefaultValue(1)] private int CacheRefreshDelay { set => ConfigHelper.MoveMember(value != 1, _ => displayRefresh = value); }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplay Instance = null!;

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context) {
        foreach (Display display in DisplayLoader.Displays) LoadConfig(display, displays.GetOrAdd(new(display), new Toggle<object>(display.DefaultEnabled) { Value = null! }));
        foreach (IInfinity infinity in InfinityLoader.ConsumableInfinities) LoadConfig(infinity, infinities.GetOrAdd(new(infinity), () => new(default)));
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
    public static void LoadConfig(IInfinity infinity, NestedValue<Color, Dictionary<ProviderDefinition, object>> config) {
        if (config.Key == default) config.Key = infinity.Defaults.Color;
        infinity.Color = config.Key;
        (var oldConfigs, config.Value) = (config.Value, []);
        foreach (var provider in _configs.GetValueOrDefault(infinity, [])) {
            var key = provider.ProviderDefinition;
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
    public static void AddConfig(IInfinity infinity, IClientConfigProvider config) => _configs.GetOrAdd(infinity, []).Add(config);
    private static readonly Dictionary<IInfinity, List<IClientConfigProvider>> _configs = [];
}

[Flags] public enum DisplayedInfinities { Infinities = 0b01, Consumable = 0b10, Both = 0b11 }
public enum CacheStyle { None, Smart, Performances }
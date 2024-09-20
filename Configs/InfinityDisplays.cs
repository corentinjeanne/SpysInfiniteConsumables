using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
namespace SPIC.Configs;

public sealed class InfinityDisplays : ModConfig {

    [Header("General")]
    [DefaultValue(true)] public bool exclusiveDisplay;
    [DefaultValue(true)] public bool alternateDisplays;

    [Header("Displays")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(EntityDefinitionValueWrapper<,>))]
    public Dictionary<DisplayDefinition, Toggle<object>> Displays {
        get => _displays;
        set {
            _displays = value;
            foreach (Display display in DisplayLoader.Displays) LoadConfig(display, _displays.GetOrAdd(new(display), new Toggle<object>(display.DefaultEnabled) { Value = null! }));
        }
    }

    [Header("Infinities")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))]
    public Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>> Infinities {
        get => _infinities;
        set {
            _infinities = value;
            foreach (IInfinity infinity in InfinityManager.ConsumableInfinities) LoadConfig(infinity, _infinities.GetOrAdd(new(infinity), DefaultClientConfig(infinity)));
        }
    }

    private Dictionary<DisplayDefinition, Toggle<object>> _displays = [];

    private Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>> _infinities = [];

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static InfinityDisplays Instance = null!;

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

    public static NestedValue<Color, Dictionary<string, object>> DefaultClientConfig(IInfinity infinity) => new(infinity.Defaults.Color);
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
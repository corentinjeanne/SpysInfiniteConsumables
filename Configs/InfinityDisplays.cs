using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using InfinityConfig = SpikysLib.Configs.NestedValue<Microsoft.Xna.Framework.Color, System.Collections.Generic.Dictionary<string, object>>;
namespace SPIC.Configs;

public sealed class InfinityDisplays : ModConfig {

    [Header("Displays")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(EntityDefinitionValueWrapper<,>))]
    public Dictionary<DisplayDefinition, Toggle<object>> Displays {
        get => _rootDisplays;
        set {
            _rootDisplays = value;
            _displays.Clear();
            foreach (Display display in DisplayLoader.Displays) {
                DisplayDefinition def = new(display);
                Type configType = display is IConfigurableDisplay c1 ? c1.ConfigType : typeof(Empty);
                var config = _rootDisplays.GetOrAdd(def, new Toggle<object>(display.DefaultEnabled) { Value = null! });
                config.Value = config.Value switch {
                    null => JsonConvert.DeserializeObject("{}", configType, ConfigManager.serializerSettings)!,
                    JToken token => token.ToObject(configType)!,
                    _ => config.Value
                };
                if (display is IConfigurableDisplay c2) c2.OnLoaded(config.Value);
                _displays[display] = config;
            }
        }
    }

    [Header("Infinities")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))]
    public Dictionary<InfinityDefinition, InfinityConfig> Infinities {
        get => _rootInfinities;
        set {
            _rootInfinities = value;
            _infinities.Clear();
            foreach (IInfinity infinity in InfinityManager.RootInfinities) LoadInfinityConfig(infinity, _rootInfinities.GetOrAdd(new(infinity), DefaultConfig(infinity)));
        }
    }

    internal static InfinityConfig DefaultConfig(IInfinity infinity) => new(infinity.DefaultColor);
    internal void LoadInfinityConfig(IInfinity infinity, InfinityConfig config) {
        (var oldConfigs, config.Value) = (config.Value, []);
        foreach (IComponent component in infinity.Components) {
            if (component is not IClientConfigurableComponents configurable) continue;
            Type configType = configurable.ConfigType;
            string key = configurable.ConfigKey;
            object? oldConfig = oldConfigs.GetValueOrDefault(key, null!);
            config.Value[key] = oldConfig switch {
                null => JsonConvert.DeserializeObject("{}", configType, ConfigManager.serializerSettings)!,
                JToken token => token.ToObject(configType)!,
                _ => oldConfig
            };
            configurable.OnLoaded(config.Value[key]);
        }
        _infinities[infinity] = config;
    }

    private Dictionary<DisplayDefinition, Toggle<object>> _rootDisplays = [];
    private readonly Dictionary<IDisplay, Toggle<object>> _displays = [];

    private Dictionary<InfinityDefinition, InfinityConfig> _rootInfinities = [];
    private readonly Dictionary<IInfinity, InfinityConfig> _infinities = [];

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static InfinityDisplays Instance = null!;

    public InfinityConfig GetConfig(IInfinity infinity) => _infinities[infinity];
    public static Color GetColor(IInfinity infinity) => Instance.GetConfig(infinity).Key;
    public static TConfig Get<TConfig>(IClientConfigurableComponents<TConfig> config) where TConfig : new() => (TConfig)Instance.GetConfig(config.Infinity).Value[config.ConfigKey];

    public static bool IsEnabled(IDisplay display) => Instance._displays[display].Key;
    public static TConfig Get<TConfig>(IConfigurableDisplay<TConfig> config) where TConfig : new() => (TConfig)Instance._displays[config].Value;
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using InfinityConfig = SpikysLib.Configs.NestedValue<Microsoft.Xna.Framework.Color, System.Collections.Generic.Dictionary<string, object>>;
namespace SPIC.Configs;

public sealed class InfinityDisplays : ModConfig {

    [Header("Configs")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))]
    public Dictionary<InfinityDefinition, InfinityConfig> Configs {
        get => _rootConfigs;
        set {
            _rootConfigs = value;
            _configs.Clear();
            foreach(IInfinity infinity in InfinityManager.RootInfinities) LoadInfinityConfig(infinity, _rootConfigs.GetOrAdd(new(infinity), DefaultConfig(infinity)));
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
        _configs[infinity] = config;
    }

    private Dictionary<InfinityDefinition, InfinityConfig> _rootConfigs = [];
    private readonly Dictionary<IInfinity, InfinityConfig> _configs = [];

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static InfinityDisplays Instance = null!;

    public InfinityConfig GetConfig(IInfinity infinity) => _configs[infinity];

    public static Color GetColor(IInfinity infinity) => Instance.GetConfig(infinity).Key;
    public static TConfig Get<TConfig>(IClientConfigurableComponents<TConfig> config) where TConfig: new() => (TConfig)Instance.GetConfig(config.Infinity).Value[config.ConfigKey];
}
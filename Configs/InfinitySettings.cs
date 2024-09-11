using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using InfinityConfig = SpikysLib.Configs.Toggle<System.Collections.Generic.Dictionary<string, object>>;

namespace SPIC.Configs;

public sealed class InfinitySettings : ModConfig {

    [Header("Features")]
    [DefaultValue(true)] public bool DetectMissingCategories;
    [DefaultValue(true)] public bool PreventItemDuplication { get; set; }

    [Header("Infinities")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityConfigsWrapper))]
    public Dictionary<InfinityDefinition, InfinityConfig> Infinities {
        get => _rootInfinities;
        set {
            _rootInfinities = value;
            _infinities.Clear();
            foreach (IInfinity infinity in InfinityManager.RootInfinities) LoadInfinityConfig(infinity, _rootInfinities.GetOrAdd(new(infinity), DefaultConfig(infinity)));
        }
    }

    internal static InfinityConfig DefaultConfig(IInfinity infinity) => new(infinity.DefaultEnabled);
    internal void LoadInfinityConfig(IInfinity infinity, InfinityConfig config) {
        (var oldConfigs, config.Value) = (config.Value, []);
        foreach (IComponent component in infinity.Components.Reverse()) {
            if (component is not IConfigurableComponents configurable) continue;
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

    private Dictionary<InfinityDefinition, InfinityConfig> _rootInfinities = [];
    private readonly Dictionary<IInfinity, InfinityConfig> _infinities = [];

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static InfinitySettings Instance = null!;

    public InfinityConfig GetConfig(IInfinity infinity) => _infinities[infinity];
    public static bool IsEnabled(IInfinity infinity) => Instance.GetConfig(infinity).Key;
    public static TConfig Get<TConfig>(IConfigurableComponents<TConfig> config) where TConfig : new() => (TConfig)Instance.GetConfig(config.Infinity).Value[config.ConfigKey];
}
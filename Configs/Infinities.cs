using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public interface IConfigurableComponents: IComponent {
    Type ConfigType { get; }
    string ConfigKey => $"{ConfigType.Assembly.GetName().Name}/{ConfigType.Name}";
    void OnLoaded(object config) {}
}

public interface IConfigurableComponents<TConfig>: IConfigurableComponents where TConfig: new() {
    Type IConfigurableComponents.ConfigType => typeof(TConfig);
    void IConfigurableComponents.OnLoaded(object config) => OnLoaded((TConfig)config);
    void OnLoaded(TConfig config) { }
}

public sealed class InfinityConfigsWrapper: ValueWrapper<InfinityDefinition, Toggle<Dictionary<string, object>>> {
    [ValueWrapper(typeof(InfinityConfigWrapper))] public override Toggle<Dictionary<string, object>> Value { get; set; } = default!;
}
public sealed class InfinityConfigWrapper: ValueWrapper<bool, Dictionary<string, object>> {
    [CustomModConfigItem(typeof(DictionaryValuesElement)), ValueWrapper(typeof(InfinityConfigValueWrapper<>))] public override Dictionary<string, object> Value { get; set; } = default!;
}
public sealed class InfinityConfigValueWrapper<TValue>: ValueWrapper<string, TValue> {
    [CustomModConfigItem(typeof(ObjectMembersElement))] public override TValue Value { get; set; } = default!;
}

public sealed class Infinities : ModConfig {

    [Header("Features")]
    [DefaultValue(true)] public bool DetectMissingCategories;
    [DefaultValue(true)] public bool PreventItemDuplication { get; set; }

    [CustomModConfigItem(typeof(DictionaryValuesElement)), ValueWrapper(typeof(InfinityConfigsWrapper))] public Dictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> Configs {
        get => _rootConfigs;
        set {
            _rootConfigs = value;
            _configs.Clear();
            foreach(IInfinity infinity in InfinityManager.RootInfinities) {
                Toggle<Dictionary<string, object>> config = _rootConfigs.GetOrAdd(new(infinity), DefaultConfig(infinity));
                LoadInfinityConfig(infinity, config);
            }
        }
    }

    internal static Toggle<Dictionary<string, object>> DefaultConfig(IInfinity infinity) => new(infinity.EnabledByDefault);
    internal void LoadInfinityConfig(IInfinity infinity, Toggle<Dictionary<string, object>> config) {
        (var oldConfigs, config.Value) = (config.Value, []);
        foreach (IComponent component in infinity.Components) {
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
        _configs[infinity] = config;
    }

    private Dictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> _rootConfigs = [];
    private readonly Dictionary<IInfinity, Toggle<Dictionary<string, object>>> _configs = [];

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static Infinities Instance = null!;

    public Toggle<Dictionary<string, object>> GetConfig(IInfinity infinity) => _configs[infinity];

    public static bool IsEnabled<TConfig>(IInfinity infinity) => Instance.GetConfig(infinity).Key;
    public static TConfig Get<TConfig>(IConfigurableComponents<TConfig> config) where TConfig: new() => (TConfig)Instance.GetConfig(config.Infinity).Value[config.ConfigKey];
}
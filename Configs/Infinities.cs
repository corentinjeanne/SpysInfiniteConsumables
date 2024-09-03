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
    Type ConfigType();
}

public interface IConfigurableComponents<TConfig>: IConfigurableComponents where TConfig: new() {
    Type IConfigurableComponents.ConfigType() => typeof(TConfig);
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
        get => _configs;
        set {
            _configs = value;
            foreach(IInfinity infinity in InfinityManager.Infinities) {
                InfinityDefinition definition = new(infinity);
                Toggle<Dictionary<string, object>> config = _configs.GetOrAdd(definition, new Toggle<Dictionary<string, object>>(true));
                (var oldConfigs, config.Value) = (config.Value, []);
                foreach(IComponent component in infinity.Components) {
                    if (component is not IConfigurableComponents configurable) continue;
                    string key = $"{component.GetType().Assembly.FullName}/{component.GetType().Name}";
                    object? oldConfig = oldConfigs.GetValueOrDefault(key, null!);
                    config.Value[key] = oldConfig switch {
                        null => JsonConvert.DeserializeObject("{}", ConfigManager.serializerSettings)!,
                        JToken token => token.ToObject(configurable.ConfigType())!,
                        _ => oldConfig
                    };
                }
            }
        }
    }

    private Dictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> _configs = [];

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static Infinities Instance = null!;

    public Toggle<Dictionary<string, object>> GetConfig(IInfinity infinity) => _configs[new(infinity)];

    public static bool IsEnabled<TConfig>(IInfinity infinity) => Instance.GetConfig(infinity).Key;
    public static TConfig Get<TConfig>(IConfigurableComponents<TConfig> config) where TConfig: new() => (TConfig)Instance.GetConfig(config.Infinity).Value[config.GetType().Name];
}
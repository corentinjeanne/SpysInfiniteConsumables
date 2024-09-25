using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpikysLib.Collections;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public sealed class InfinitySettings : ModConfig {

    [Header("Features")]
    [DefaultValue(true)] public bool detectMissingCategories;
    [DefaultValue(true)] public bool preventItemDuplication;

    [Header("Infinities")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityConfigsWrapper))]
    public Dictionary<InfinityDefinition, Toggle<Dictionary<ProviderDefinition, object>>> infinities = [];

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static InfinitySettings Instance = null!;

    // Compatibility version < v4.0
    [JsonProperty] private Dictionary<InfinityDefinition, ConsumableInfinities> Configs { set => ConfigHelper.MoveMember(value is not null, _ => {
        foreach ((var d, var config) in value!) {
            if (d.ToString() == "SPIC/Currencies") {
                Default.Infinities.Currency.PortConfig(infinities, config);
                continue;
            }
            InfinityDefinition def = d.ToString() == "SPIC/Items" ? new("SPIC/ConsumableItem") : d;
            foreach ((var infinity, var requirements) in config.infinities) {
                requirements.Value[ProviderDefinition.Config] = JObject.FromObject(requirements.Value);
                requirements.Value[ProviderDefinition.Customs] = JObject.FromObject(new Dictionary<ItemDefinition, Count>());
            }
            Dictionary<ItemDefinition, Count> customs = [];
            if (config.customs is not null) {
                foreach((var item, var custom) in config.customs) {
                    if (custom.Choice == nameof(Custom.Individual)) {
                        foreach((var infinity, var requirement) in custom.Individual) {
                            ((JObject)config.infinities[infinity].Value[ProviderDefinition.Customs]!)[item.ToString()] = requirement.Value;
                        }
                    } else customs[item] = custom.Global;
                }
            }
            infinities.GetOrAdd(def, _ => new(true)).Value[ProviderDefinition.Infinities] = config;
            infinities[def].Value[ProviderDefinition.Customs] = customs;
        }
    }); }

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context) {
        foreach (IConsumableInfinity infinity in InfinityLoader.ConsumableInfinities) LoadConfig(infinity, infinities.GetOrAdd(new(infinity), _ => new(infinity.Defaults.Enabled)));
    }
    public static void LoadConfig(IInfinity infinity, Toggle<Dictionary<ProviderDefinition, object>> config) {
        (var oldConfigs, config.Value) = (config.Value, []);
        infinity.Enabled = config.Key;
        foreach (var provider in _configs.GetValueOrDefault(infinity, [])) {
            var key = provider.ProviderDefinition;
            Type configType = provider.ConfigType;
            object? oldConfig = oldConfigs.GetValueOrDefault(key, null!);
            config.Value[key] = oldConfig switch {
                null => JsonConvert.DeserializeObject("{}", configType, ConfigManager.serializerSettings)!,
                JToken token => token.ToObject(configType)!,
                _ => oldConfig
            };
            provider.Config = config.Value[key];
            provider.OnLoaded(oldConfig is null);
        }
    }
    public static void AddConfig(IInfinity infinity, IConfigProvider config) => _configs.GetOrAdd(infinity, []).Add(config);
    private static readonly Dictionary<IInfinity, List<IConfigProvider>> _configs = [];

    public override void OnChanged() {
        if (!Main.gameMenu && Main.netMode != NetmodeID.Server) InfinityManager.ClearCache();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpikysLib.Collections;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using SpikysLib.DataStructures;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;


public sealed class UsedInfinities : MultiChoice<int> {
    public UsedInfinities() : base() { }
    public UsedInfinities(int value) : base(value) { }

    [Choice] public Text All { get; set; } = new();
    [Choice, Range(1, 9999)] public int Maximum { get; set; } = 1;

    public override int Value {
        get => Choice == nameof(All) ? 0 : Maximum;
        set {
            if (value != 0) {
                Choice = nameof(Maximum);
                Maximum = value;
            } else Choice = nameof(All);
        }
    }

    public static implicit operator UsedInfinities(int used) => new(used);
}

public class ConsumableInfinities {
    [JsonIgnore, ShowDespiteJsonIgnore]
    public PresetDefinition Preset {
        get => _preset;
        set {
            _preset = value;
            if (Infinities.Count != 0) value.Entity?.ApplyCriterias(this);
        }
    }
    public UsedInfinities UsedInfinities { get; set; } = 0;
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityConfigsWrapper))] public OrderedDictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> Infinities { get; set; } = [];
    
    private PresetDefinition _preset = new();
}

public class ClientConsumableInfinities {
    [DefaultValue(DisplayedInfinities.Infinities)] public DisplayedInfinities displayedInfinities = DisplayedInfinities.Infinities;
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))] public OrderedDictionary<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>> Infinities { get; set; } = [];
}

public class ConsumableInfinitiesProvider<TConsumable>(ConsumableInfinity<TConsumable> Infinity) : IConfigProvider<ConsumableInfinities>, IClientConfigProvider<ClientConsumableInfinities> {
    public ConsumableInfinities Config { get; set; } = null!;
    public ClientConsumableInfinities ClientConfig { get; set; } = null!;

    public void OnLoaded(bool created) {
        Infinity._orderedInfinities.Clear();
        List<IInfinity> toRemove = [];
        foreach (var infinity in Infinity._infinities) Config.Infinities.GetOrAdd(new(infinity), InfinitySettings.DefaultConfig(infinity));
        foreach ((var key, var value) in Config.Infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !Infinity._infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            InfinitySettings.LoadConfig(infinity, value);
            Infinity._orderedInfinities.Add(infinity);
        }
        foreach (var infinity in toRemove) Config.Infinities.Remove(new(infinity));

        if (created && Infinity.ConsumableDefaults.Preset is not null) {
            Infinity.ConsumableDefaults.Preset.ApplyCriterias(Config);
            Config.Preset = new(Infinity.ConsumableDefaults.Preset);
        } else {
            Preset? preset = null;
            foreach (Preset p in Infinity.Presets) if (p.MeetsCriterias(Config) && (preset is null || p.CriteriasCount >= preset.CriteriasCount)) preset = p;
            Config.Preset = preset is not null ? new(preset) : new();
        }
        Config.Preset.Consumable = Infinity;
    }
    public void OnLoadedClient(bool created) {
        List<IInfinity> toRemove = [];
        foreach (Infinity<TConsumable> infinity in Infinity._orderedInfinities) ClientConfig.Infinities.GetOrAdd(new(infinity), InfinityDisplays.DefaultClientConfig(infinity));
        foreach ((InfinityDefinition key, NestedValue<Color, Dictionary<string, object>> value) in ClientConfig.Infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !Infinity._infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            InfinityDisplays.LoadConfig(infinity, value);
        }
        foreach (var infinity in toRemove) ClientConfig.Infinities.Remove(new(infinity));
        if (created) ClientConfig.displayedInfinities = Infinity.ConsumableDefaults.DisplayedInfinities;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
            if (infinities.Count != 0) value.Entity?.ApplyCriterias(this);
        }
    }
    public UsedInfinities usedInfinities { get; set; } = 0;
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityConfigsWrapper))]
    public OrderedDictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> infinities { get; set; } = [];
    
    private PresetDefinition _preset = new();

    // Compatibility version < v4.0
    [JsonProperty] private Dictionary<ItemDefinition, Custom> Customs { set => customs = value; }
    internal Dictionary<ItemDefinition, Custom>? customs;
}

public class ClientConsumableInfinities {
    [DefaultValue(DisplayedInfinities.Infinities)] public DisplayedInfinities displayedInfinities = DisplayedInfinities.Infinities;
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))]
    public Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>> infinities = [];
}

public class ConsumableInfinitiesProvider<TConsumable>(ConsumableInfinity<TConsumable> Infinity) : IConfigProvider<ConsumableInfinities>, IClientConfigProvider<ClientConsumableInfinities> {
    public ConsumableInfinities Config { get; set; } = null!;
    public ClientConsumableInfinities ClientConfig { get; set; } = null!;

    public void OnLoaded(bool created) {
        Infinity._orderedInfinities.Clear();
        List<IInfinity> toRemove = [];
        foreach (var infinity in Infinity._infinities) Config.infinities.GetOrAdd(new(infinity), _ => new(Infinity.Defaults.Enabled));
        foreach ((var key, var value) in Config.infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !Infinity._infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            InfinitySettings.LoadConfig(infinity, value);
            Infinity._orderedInfinities.Add(infinity);
        }
        foreach (var infinity in toRemove) Config.infinities.Remove(new(infinity));

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
        foreach (Infinity<TConsumable> infinity in Infinity._orderedInfinities) ClientConfig.infinities.GetOrAdd(new(infinity), _ => new(default));
        foreach ((InfinityDefinition key, NestedValue<Color, Dictionary<string, object>> value) in ClientConfig.infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !Infinity._infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            InfinityDisplay.LoadConfig(infinity, value);
        }
        foreach (var infinity in toRemove) ClientConfig.infinities.Remove(new(infinity));
        if (created) ClientConfig.displayedInfinities = Infinity.ConsumableDefaults.DisplayedInfinities;
    }
}

public sealed class Custom : MultiChoice {

    [Choice] public Count Global { get; set; } = new();

    [Choice] public Dictionary<InfinityDefinition, Count> Individual { get; set; } = new(); // Count | Count<TCategory>

    public bool TryGetIndividial(IInfinity infinity, [MaybeNullWhen(false)] out Count choice) {
        if (Choice == nameof(Individual)) return Individual.TryGetValue(new(infinity), out choice);
        choice = default;
        return false;
    }
    public bool TryGetGlobal([MaybeNullWhen(false)] out Count count) => TryGet(nameof(Global), out count);
}
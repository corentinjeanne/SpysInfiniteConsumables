using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SPIC.Configs.Presets;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public class InfinityValueWrapper<TValue> : ValueWrapper<InfinityDefinition, TValue> {
    [ColorNoAlpha, ColorHSLSlider]
    public override TValue Value { get; set; } = default!;
    public override void OnBind(ConfigElement element) {
        if (Key.IsUnloaded) return;
        SpikysLib.Reflection.ConfigElement.backgroundColor.SetValue(element, Key.Entity!.Color);
        SpikysLib.Reflection.ConfigElement.TooltipFunction.SetValue(element, () => Key.Tooltip!);
    }
}

public sealed class UsedInfinities : MultiChoice<int> {
    public UsedInfinities() : base() { }
    public UsedInfinities(int value) : base(value) { }

    [Choice] public Text All { get; set; } = new();
    [Choice, Range(1, 9999)] public int Used { get; set; } = 1;

    public override int Value {
        get => Choice == nameof(All) ? 0 : Used;
        set {
            if (value != 0) {
                Choice = nameof(Used);
                Used = value;
            } else Choice = nameof(All);
        }
    }

    public static implicit operator UsedInfinities(int used) => new(used);
}

public sealed class GroupConfig {
    public PresetDefinition Preset {
        get => _preset;
        set {
            _preset = value;
            if (Infinities.Count != 0) PresetLoader.GetPreset(value.Mod, value.Name)?.ApplyCriterias(this);
        }
    }
    private PresetDefinition _preset = new();

    public UsedInfinities UsedInfinities { get; set; } = 0;

    [CustomModConfigItem(typeof(DictionaryValuesElement)), ValueWrapper(typeof(InfinityValueWrapper<>))] public OrderedDictionary/*<InfinityDefinition, Toggle<T>>*/ Infinities { get; set; } = new();

    [JsonProperty] internal Dictionary<InfinityDefinition, Wrapper> Configs { get; set; } = new(); // Compatibility version < v3.1.1


    public Dictionary<ItemDefinition, Custom> Customs { get; set; } = new();

    public bool HasCustomCategory<TConsumable, TCategory>(TConsumable consumable, Infinity<TConsumable, TCategory> infinity, [MaybeNullWhen(false)] out TCategory category) where TConsumable : notnull where TCategory : struct, System.Enum {
        if (Customs.TryGetValue(new(infinity.Group.ToItem(consumable).type), out Custom? custom) && custom.TryGetIndividial(infinity, out Count? count) && count.Value < 0) {
            category = ((Count<TCategory>)count).Category;
            return true;
        }
        category = default;
        return false;
    }
    public bool HasCustomCount<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity, [MaybeNullWhen(false)] out Count count) where TConsumable : notnull {
        if (Customs.TryGetValue(new(infinity.Group.ToItem(consumable).type), out Custom? custom) && custom.TryGetIndividial(infinity, out count) && count.Value >= 0) return true;
        count = default;
        return false;
    }
    public bool HasCustomGlobal<TConsumable>(TConsumable consumable, Group<TConsumable> group, [MaybeNullWhen(false)] out Count count) where TConsumable : notnull {
        if (Customs.TryGetValue(new(group.ToItem(consumable).type), out Custom? custom) && custom.TryGetGlobal(out count)) return true;
        count = default;
        return false;
    }
}

public sealed class GroupColors {
    [CustomModConfigItem(typeof(DictionaryValuesElement)), ValueWrapper(typeof(InfinityValueWrapper<>))] public Dictionary<InfinityDefinition, Color> Colors { get; set; } = new();
}
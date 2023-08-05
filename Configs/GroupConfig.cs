using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Configs.Presets;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public sealed class UsedInfinities : MultyChoice<int> {
    public UsedInfinities() : base() { }
    public UsedInfinities(int value) : base(value) { }

    [Choice] public UI.Text All { get; set; } = new();
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
    [Header("Infinities")]
    public PresetDefinition Preset {
        get => _preset;
        set {
            _preset = value;
            if (Infinities.Count != 0) PresetLoader.GetPreset(value.Mod, value.Name)?.ApplyCriterias(this);
        }
    }
    private PresetDefinition _preset = new();

    [CustomModConfigItem(typeof(UI.CustomDictionaryElement))] public OrderedDictionary /*<InfinityDefinition, bool>*/ Infinities { get; set; } = new();

    public UsedInfinities UsedInfinities { get; set; } = 0; // ? Apply Infinity overrides when effective infinity is mixed

    [Header("Configs")]
    [CustomModConfigItem(typeof(UI.CustomDictionaryElement))] public Dictionary<InfinityDefinition, Wrapper> Configs { get; set; } = new();

    [Header("Customs")]
    public Dictionary<ItemDefinition, Custom> Customs { get; set; } = new();

    public bool HasCustomCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity, [MaybeNullWhen(false)] out TCategory category) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, System.Enum {
        if (Customs.TryGetValue(new(infinity.Group.ToItem(consumable).type), out Custom? custom) && custom.TryGetIndividial(infinity, out Count? count) && count.Value < 0) {
            category = ((Count<TCategory>)count).Category;
            return true;
        }
        category = default;
        return false;
    }
    public bool HasCustomCount<TGroup, TConsumable>(TConsumable consumable, Infinity<TGroup, TConsumable> infinity, [MaybeNullWhen(false)] out Count count) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        if (Customs.TryGetValue(new(infinity.Group.ToItem(consumable).type), out Custom? custom) && custom.TryGetIndividial(infinity, out count) && count.Value >= 0) return true;
        count = default;
        return false;
    }
    public bool HasCustomGlobal<TGroup, TConsumable>(TConsumable consumable, Group<TGroup, TConsumable> group, [MaybeNullWhen(false)] out Count count) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        if (Customs.TryGetValue(new(group.ToItem(consumable).type), out Custom? custom) && custom.TryGetGlobal(out count)) return true;
        count = default;
        return false;
    }
}

public sealed class GroupColors {
    [CustomModConfigItem(typeof(UI.CustomDictionaryElement))] public Dictionary<InfinityDefinition, Color> Colors { get; set; } = new();
}
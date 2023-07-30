using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public sealed class GroupConfig {
    [Header("Infinities")]
    public PresetDefinition Preset {
        get {
            if (Infinities.Count == 0) return new();
            Preset? preset = null;
            foreach (Preset p in PresetLoader.Presets) {
                if (p.MeetsCriterias(this) && (preset is null || p.CriteriasCount >= preset.CriteriasCount)) preset = p;
            }
            return preset is not null ? new(preset.Mod.Name, preset.Name) : new();
        }
        set {
            if (Infinities.Count == 0) return;
            PresetLoader.GetPreset(value.Mod, value.Name)?.ApplyCriterias(this);
        }
    }
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public OrderedDictionary /*<InfinityDefinition, bool>*/ Infinities { get; set; } = new();

    public int UsedInfinities { get; set; } // ? Apply Infinity overrides when effective infinity is mixed

    [Header("Configs")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<InfinityDefinition, Wrapper> Configs { get; set; } = new();

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

    internal GroupConfig() { }
    internal GroupConfig(IGroup group) => SetGroup(group);
    internal void SetGroup(IGroup group) {
        OrderedDictionary /* <InfinityDefinition, bool> */ infinitiesBool = new();
        foreach((string key, bool enabled) in Infinities.Items<string, bool>()) infinitiesBool[new InfinityDefinition(key)] = enabled;
        Infinities = infinitiesBool;
        
        List<IInfinity> infinities = new(group.Infinities);
        IEnumerable<IInfinity> InfinitiesByOrder() {
            foreach((InfinityDefinition def, bool enabled) in Infinities.Items<InfinityDefinition, bool>()) {
                int i = infinities.FindIndex(i => i.Mod.Name == def.Mod && i.Name == def.Name);
                if(i == -1) continue;
                yield return infinities[i];
                infinities[i].Enabled = (bool)Infinities[def]!;
                infinities.RemoveAt(i);
            }
            foreach (IInfinity infinity in infinities) {
                InfinityDefinition def = new(infinity);
                Infinities.TryAdd(def, infinity.DefaultsToOn);
                infinity.Enabled = (bool)Infinities[def]!;
                yield return infinity;
            }
        }
        group.SetInfinities(InfinitiesByOrder());

        foreach ((IInfinity infinity, Wrapper wrapper) in group.InfinityConfigs) {
            InfinityDefinition def = new(infinity);
            Configs[def] = Configs.TryGetValue(def, out var config) ? config.ChangeType(wrapper.Member.Type) : Wrapper.From(wrapper.Member.Type);
            wrapper.Value = Configs[def].Value;
        }

        foreach (Custom custom in Customs.Values) custom.SetGroup(group);
        group.Config = this;
    }

}

public sealed class GroupColors {

    [CustomModConfigItem(typeof(CustomDictionaryElement)), ColorNoAlpha, ColorHSLSlider]
    public Dictionary<InfinityDefinition, Color> Colors { get; set; } = new();

    internal GroupColors() { }
    internal GroupColors(IGroup group) => SetGroup(group);
    internal void SetGroup(IGroup group) {
        foreach (IInfinity infinity in group.Infinities) {
            InfinityDefinition def = new(infinity);
            infinity.Color = Colors[def] = Colors.GetValueOrDefault(def, infinity.DefaultColor);
        }
        group.Colors = this;
    }

}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using SPIC.ConsumableTypes;
using SPIC.Infinities;
using SPIC.Configs.UI;
using System.Collections;

namespace SPIC.Configs;

public class RequirementItems : SingleFieldItem {
    [Range(0, 9999)]
    public int items;
    public object Disabled => null;

    public static implicit operator int(RequirementItems r) => r.items;
    public static implicit operator RequirementItems(long i) {
        RequirementItems r = new();
        if(i == 0) r.Select(nameof(Disabled));
        else {
            r.items = (int)i;
            r.Select(nameof(items));
        }
        return r;
    }
}
public class Requirement : SingleFieldItem {
    [Range(0, 9999)]
    public int items;
    [Range(0, 50)]
    public int stacks;
    public object Disabled => null;

    public static implicit operator int(Requirement r) => r.SelectedMember.Name == nameof(items) ? r.items : -r.stacks;
    public static implicit operator Requirement(long i) {
        Requirement r = new();
        if(i == 0) r.Select(nameof(Disabled));
        else if(i > 0){
            r.items = (int)i;
            r.Select(nameof(items));
        } else {
            r.stacks = (int)-i;
            r.Select(nameof(stacks));
        }
        return r;
    }
    public override string ToString() {
        return $"{(int)this}";
    }
}

// TODO Unloaded infinities
[Label("$Mods.SPIC.Configs.Requirements.name")]
public class RequirementSettings : ModConfig {

    public enum InfinityPreset {
        None,
        Default,
        OneForAll,
        AllOf,
        AllOn,
        JourneyCosts
    }

    [Header("$Mods.SPIC.Configs.Requirements.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Requirements.General.Duplication"), Tooltip("$Mods.SPIC.Configs.Requirements.General.t_duplication")]
    public bool PreventItemDupication { get; set; }

    [Label("$Mods.SPIC.Configs.Requirements.General.Preset")]
    public InfinityPreset ActivePreset { // TODO Rework to be more flexible
        get {
            bool defaults = true;
            int on = 0;
            foreach((InfinityDefinition def, bool state) in Infinities){
                Infinity inf = def.Infinity;
                if (defaults && (inf == null || state != inf.DefaultValue)) defaults = false;
                if (state) on++;
            }
            if (on == 0) return InfinityPreset.AllOf;

            if (MaxConsumableTypes == 0) {
                if (defaults) return InfinityPreset.Default;
                if (on == Infinities.Count) return InfinityPreset.AllOn;
            }

            if (MaxConsumableTypes == 1) {
                if (Requirements.Count > 0 && ((ConsumableTypeDefinition)Requirements.Keys.Index(0)).Name == JourneySacrifice.Instance.Name && Infinities[JourneyResearch.Instance.ToDefinition()]) return InfinityPreset.JourneyCosts;
                return InfinityPreset.OneForAll;
            }

            return InfinityPreset.None;
        }
        set {
            switch (value) {
            case InfinityPreset.Default:
                Infinities = new();
                MaxConsumableTypes = 0;
                break;
            case InfinityPreset.OneForAll:
                MaxConsumableTypes = 1;
                break;
            case InfinityPreset.JourneyCosts:
                Infinities[JourneyResearch.Instance.ToDefinition()] = true;
                ConsumableTypeDefinition journeyDef = JourneySacrifice.Instance.ToDefinition();
                Requirements.Move(journeyDef, 0);
                MaxConsumableTypes = 1;
                break;
            case InfinityPreset.AllOn:
                foreach (InfinityDefinition def in Infinities.Keys) Infinities[def] = true;
                MaxConsumableTypes = 0;
                break;
            case InfinityPreset.AllOf:
                foreach (InfinityDefinition def in Infinities.Keys) Infinities[def] = false;
                break;
            case InfinityPreset.None:
            default:
                break;
            }
        }
    }

    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<InfinityDefinition, bool> Infinities {
        get => _infinities;
        set {
            _infinities.Clear();
            foreach ((InfinityDefinition inf, bool state) in value) {
                if(!inf.IsUnloaded) _infinities.Add(inf, state); // TODO save unloaded infinities
            }
            foreach(Infinity inf in InfinityManager.Infinities) _infinities.TryAdd(inf.ToDefinition(), inf.DefaultValue);
        }
    }
    private readonly Dictionary<InfinityDefinition, bool> _infinities = new();

    [Header("$Mods.SPIC.Configs.Requirements.Requirements.header")]
    [Label("$Mods.SPIC.Configs.Requirements.General.MaxTypes")]
    public int MaxConsumableTypes { get; set; }


    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
    public OrderedDictionary Requirements {
        get => _requirements;
        set {
            _requirements.Clear();
            foreach(DictionaryEntry entry in value) {
                ConsumableTypeDefinition def = new((string)entry.Key);
                if(def.IsUnloaded) continue; // TODO save unloaded consumable types
                ConsumableType type = def.ConsumableType;

                JObject data = entry.Value as JObject;
                _requirements.Add(def, type.ConfigRequirements = data.ToObject(type.CreateRequirements().GetType()));
            }

            // Add the mising ones
            foreach (ConsumableType type in InfinityManager.ConsumableTypes) {
                ConsumableTypeDefinition def = type.ToDefinition();
                if(!_requirements.Contains(def)){
                    _requirements.Add(def, type.ConfigRequirements = type.CreateRequirements());
                }
            }
        }
    }
    private readonly OrderedDictionary _requirements = new();

    // TODO Reimplement customs
    public bool HasCustomCategory(int itemType, int consumableID, out byte category){
        category = ConsumableType.NoCategory;
        return false;
    }

    public bool HasCustomRequirement(int itemType, int consumableID, out int requirement){
        requirement = ConsumableType.NoRequirement;
        return false;
    }

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static RequirementSettings Instance;

    private bool _modifiedInGame = false;
    public void ManualSave() {
        if (_modifiedInGame) this.SaveConfig();
        _modifiedInGame = false;
    }
}

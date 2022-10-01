using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using SPIC.ConsumableTypes;
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
    public object Disabled => null; // TODO find another way using allow null

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
            int count = 0;
            foreach(DictionaryEntry entry in EnabledTypes){
                IToggleable inf = (IToggleable)((ConsumableTypeDefinition)entry.Key).ConsumableType;
                bool state = (bool)entry.Value;
                if(inf is null) continue;
                if (defaults && state != inf.DefaultsToOn) defaults = false;
                if (state) on++;
                count++;
            }
            if (on == 0) return InfinityPreset.AllOf;

            if (MaxConsumableTypes == 0) {
                if (defaults) return InfinityPreset.Default;
                if (on == EnabledTypes.Count) return InfinityPreset.AllOn;
            }

            if (MaxConsumableTypes == 1) {
                if (((ConsumableTypeDefinition)EnabledTypes.Keys.Index(0)).Name == JourneySacrifice.Instance.Name && (bool)EnabledTypes[JourneySacrifice.Instance.ToDefinition()]) return InfinityPreset.JourneyCosts;
                return InfinityPreset.OneForAll;
            }

            return InfinityPreset.None;
        }
        set {
            switch (value) {
            case InfinityPreset.Default:
                EnabledTypes = new();
                MaxConsumableTypes = 0;
                break;
            case InfinityPreset.OneForAll:
                MaxConsumableTypes = 1;
                break;
            case InfinityPreset.JourneyCosts:
                EnabledTypes[JourneySacrifice.Instance.ToDefinition()] = true;
                ConsumableTypeDefinition journeyDef = JourneySacrifice.Instance.ToDefinition();
                EnabledTypes.Move(journeyDef, 0);
                MaxConsumableTypes = 1;
                break;
            case InfinityPreset.AllOn:
                for (int i = 0; i < EnabledTypes.Count; i++) EnabledTypes[i] = true;
                MaxConsumableTypes = 0;
                break;
            case InfinityPreset.AllOf:
                for (int i = 0; i < EnabledTypes.Count; i++) EnabledTypes[i] = false;
                break;
            case InfinityPreset.None:
            default:
                break;
            }
        }
    }

    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
    public OrderedDictionary/*<ConsumableTypeDefinition, bool>*/ EnabledTypes {
        get => _types;
        set {
            _types.Clear();
            foreach (DictionaryEntry entry in value) {
                ConsumableTypeDefinition def = new((string)entry.Key);
                if (def.IsUnloaded || def.ConsumableType is not IToggleable config) continue;
                bool state = entry.Value switch {
                    JObject jobj => (bool)jobj,
                    bool b => b,
                    _ => throw new NotImplementedException()
                };
                _types.Add(def, state);
            }
            foreach (IToggleable type in InfinityManager.ConsumableTypes<IToggleable>(FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                _types.TryAdd(type.ToDefinition(), type.DefaultsToOn);
            }
        }
    }
    private readonly OrderedDictionary _types = new();

    [Label("$Mods.SPIC.Configs.Requirements.General.MaxTypes")]
    public int MaxConsumableTypes { get; set; }

    public Dictionary<ConsumableTypeDefinition, bool> EnabledGlobals {
        get => _globals;
        set {
            foreach (IToggleable type in InfinityManager.ConsumableTypes<IToggleable>(FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                value.TryAdd(type.ToDefinition(), type.DefaultsToOn);
            }
            _globals = value;
        }
    }
    private Dictionary<ConsumableTypeDefinition, bool> _globals = new();


    [Header("$Mods.SPIC.Configs.Requirements.Requirements.header")]
    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, object> Requirements {
        get => _requirements;
        set {
            _requirements.Clear();
            foreach((ConsumableTypeDefinition def, object data) in value) {
                if(def.IsUnloaded || def.ConsumableType is not IConfigurable config) continue;

                if(data is JObject jobj) config.Settings = jobj.ToObject(config.SettingsType);
                else if(data.GetType() == config.SettingsType) config.Settings = data;
                else throw new NotImplementedException();
                
                _requirements.Add(def, config.Settings);
            }
            foreach (IConfigurable type in InfinityManager.ConsumableTypes<IConfigurable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                ConsumableTypeDefinition def = type.ToDefinition();
                if(!_requirements.ContainsKey(def)) _requirements.Add(def, type.Settings = Activator.CreateInstance(type.SettingsType));
            }
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, object> _requirements = new();

    // TODO Reimplement customs

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static RequirementSettings Instance;

    public void UpdateProperties(){
        // EnabledTypes = EnabledTypes;
        // Requirements = Requirements;
        this.SaveConfig();
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using SPIC.ConsumableTypes;
using SPIC.Configs.UI;
using System.Collections;
using System.Linq.Expressions;
using SPIC.Configs.Presets;

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

    [Header("$Mods.SPIC.Configs.Requirements.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Requirements.General.Duplication"), Tooltip("$Mods.SPIC.Configs.Requirements.General.t_duplication")]
    public bool PreventItemDupication { get; set; }


    public static IList GetPresets(){
        List<PresetDefinition> defs = new();
        foreach(Preset preset in PresetManager.Presets())
            defs.Add(preset.ToDefinition());
        return defs;
    }
    public static IList GetChoices() => new List<string>() { "One", "Two", "Three" };

    [Label("$Mods.SPIC.Configs.Requirements.General.Preset")]
    [CustomModConfigItem(typeof(DropDownUI)), ValuesProvider(typeof(RequirementSettings), nameof(GetPresets), "FullName")]
    public PresetDefinition Preset {
        get {
            if(Requirements.Count == 0) return null;
            foreach (Preset preset in PresetManager.Presets()){
                if(preset.MeetsCriterias(this)) return preset.ToDefinition();
            }
            return null;
        } set {
            if (Requirements.Count == 0) return;
            value?.Preset.ApplyCriterias(this);
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

    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
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

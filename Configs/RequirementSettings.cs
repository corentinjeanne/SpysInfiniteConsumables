using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.UI;
using SPIC.Configs.Presets;

namespace SPIC.Configs;

[Label("$Mods.SPIC.Configs.Requirements.name")]
public class RequirementSettings : ModConfig {

    [Header("$Mods.SPIC.Configs.Requirements.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Requirements.General.Duplication"), Tooltip("$Mods.SPIC.Configs.Requirements.General.t_duplication")]
    public bool PreventItemDupication { get; set; }

    public static List<PresetDefinition> GetPresets(){
        List<PresetDefinition> defs = new();
        foreach(Preset preset in PresetManager.Presets())
            defs.Add(preset.ToDefinition());
        return defs;
    }

    [Label("$Mods.SPIC.Configs.Requirements.General.Preset")]
    [CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(RequirementSettings), nameof(GetPresets), nameof(PresetDefinition.Label))]
    public PresetDefinition Preset {
        get {
            if(EnabledTypes.Count == 0) return null;
            foreach (Preset preset in PresetManager.Presets()){
                if(preset.MeetsCriterias(this)) return preset.ToDefinition();
            }
            return null;
        } set {
            if (EnabledTypes.Count == 0) return;
            value?.Preset.ApplyCriterias(this);
        }
    }

    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public OrderedDictionary/*<ConsumableTypeDefinition, bool>*/ EnabledTypes {
        get => _types;
        set {
            _types.Clear();
            foreach (DictionaryEntry entry in value) {
                ConsumableTypeDefinition def = new((string)entry.Key);
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _types.Add(def, entry.Value);
                    continue;
                }
                bool state = entry.Value switch {
                    JObject jobj => (bool)jobj,
                    bool b => b,
                    _ => throw new NotImplementedException()
                };
                _types.Add(def, state);
            }
            foreach (IToggleable type in InfinityManager.ConsumableGroups<IToggleable>(FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                _types.TryAdd(type.ToDefinition(), type.DefaultsToOn);
            }
        }
    }
    private readonly OrderedDictionary _types = new();

    [Label("$Mods.SPIC.Configs.Requirements.General.MaxTypes")]
    public int MaxConsumableTypes { get; set; }

    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, bool> EnabledGlobals {
        get => _globals;
        set {
            _globals.Clear();
            foreach ((ConsumableTypeDefinition def, bool state) in value) {
                if (def.IsUnloaded && ModLoader.HasMod(def.Mod)) continue;
                _globals.Add(def, state);
            }
            foreach (IToggleable type in InfinityManager.ConsumableGroups<IToggleable>(FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                _globals.TryAdd(type.ToDefinition(), type.DefaultsToOn);
            }
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, bool> _globals = new();

    [JsonIgnore]
    public IEnumerable<(IToggleable type, bool enabled, bool global)> LoadedTypes {
        get {
            foreach(DictionaryEntry entry in EnabledTypes){
                ConsumableTypeDefinition def = (ConsumableTypeDefinition)entry.Key;
                if (!def.IsUnloaded) yield return ((IToggleable)def.ConsumableType, (bool)entry.Value, false);
            }
            foreach((ConsumableTypeDefinition def, bool state) in EnabledGlobals){
                if (!def.IsUnloaded) yield return ((IToggleable)def.ConsumableType, state, true);
            }
        }
    }

    [Header("$Mods.SPIC.Configs.Requirements.Requirements.header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, object> Requirements {
        get => _requirements;
        set {
            _requirements.Clear();
            foreach((ConsumableTypeDefinition def, object data) in value) {
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _requirements.Add(def, data);
                    continue;
                }
                if(def.ConsumableType is not IConfigurable config) continue;

                if(data is JObject jobj) config.Settings = jobj.ToObject(config.SettingsType);
                else if(data.GetType() == config.SettingsType) config.Settings = data;
                else throw new NotImplementedException();

                _requirements.Add(def, config.Settings);
            }
            foreach (IConfigurable type in InfinityManager.ConsumableGroups<IConfigurable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
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

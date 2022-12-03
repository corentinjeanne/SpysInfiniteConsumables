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
using SPIC.Config.UI;
using SPIC.Config.Presets;

namespace SPIC.Config;

[Label("$Mods.SPIC.Config.Requirements.name")]
public class RequirementSettings : ModConfig {

    [Header("$Mods.SPIC.Config.Requirements.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Config.Requirements.General.Duplication"), Tooltip("$Mods.SPIC.Config.Requirements.General.t_duplication")]
    public bool PreventItemDupication { get; set; }
    [Label("$Mods.SPIC.Config.Requirements.General.Preset"), CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(RequirementSettings), nameof(GetPresets), nameof(PresetDefinition.Label))]
    public PresetDefinition? Preset {
        get {
            if (EnabledGroups.Count == 0) return null;
            foreach (Preset preset in PresetManager.Presets()) {
                if (preset.MeetsCriterias(this)) return preset.ToDefinition();
            }
            return null;
        }
        set {
            if (EnabledGroups.Count == 0) return;
            value?.Preset.ApplyCriterias(this);
        }
    }
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public OrderedDictionary/*<ConsumableTypeDefinition, bool>*/ EnabledGroups {
        get => _groups;
        set {
            _groups.Clear();
            foreach (DictionaryEntry entry in value) {
                ConsumableGroupDefinition def = new((string)entry.Key);
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _groups.Add(def, entry.Value);
                    continue;
                }
                bool state = entry.Value switch {
                    JObject jobj => (bool)jobj,
                    bool b => b,
                    _ => throw new NotImplementedException()
                };
                _groups.Add(def, state);
            }
            foreach (IToggleable group in InfinityManager.ConsumableGroups<IToggleable>(FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                _groups.TryAdd(group.ToDefinition(), group.DefaultsToOn);
            }
        }
    }
    [Label("$Mods.SPIC.Config.Requirements.General.MaxGroups"), Tooltip("$Mods.SPIC.Config.Requirements.General.t_maxGroups")]
    public int MaxConsumableTypes { get; set; }
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableGroupDefinition, bool> EnabledGlobals {
        get => _globals;
        set {
            _globals.Clear();
            foreach ((ConsumableGroupDefinition def, bool state) in value) {
                if (def.IsUnloaded && ModLoader.HasMod(def.Mod)) continue;
                _globals.Add(def, state);
            }
            foreach (IToggleable group in InfinityManager.ConsumableGroups<IToggleable>(FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                _globals.TryAdd(group.ToDefinition(), group.DefaultsToOn);
            }
        }
    }


    [Header("$Mods.SPIC.Config.Requirements.Requirements.header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableGroupDefinition, object> Requirements {
        get => _requirements;
        set {
            _requirements.Clear();
            foreach ((ConsumableGroupDefinition def, object data) in value) {
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _requirements.Add(def, data);
                    continue;
                }
                if (def.ConsumableType is not IConfigurable config) continue;

                if (data is JObject jobj) config.Settings = jobj.ToObject(config.SettingsType)!;
                else if (data.GetType() == config.SettingsType) config.Settings = data;
                else throw new NotImplementedException();

                _requirements.Add(def, config.Settings);
            }
            foreach (IConfigurable group in InfinityManager.ConsumableGroups<IConfigurable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                ConsumableGroupDefinition def = group.ToDefinition();
                if (!_requirements.ContainsKey(def)) _requirements.Add(def, group.Settings = Activator.CreateInstance(group.SettingsType)!);
            }
        }
    }


    [Header("$Mods.SPIC.Config.Requirements.Blacklists.header")]
    [Label("$Mods.SPIC.Config.Requirements.Blacklists.Items")]
    public HashSet<ItemDefinition> BlackListedItems { get; set; } = new();
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableGroupDefinition,HashSet<string>> BlackListedConsumables { 
        get => _blackListedConsumables;
        set {
            _blackListedConsumables.Clear();
            foreach ((ConsumableGroupDefinition def, HashSet<string> consumables) in value) {
                if (def.IsUnloaded && ModLoader.HasMod(def.Mod)) continue;
                _blackListedConsumables.Add(def, consumables);
            }
            foreach (IConsumableGroup group in InfinityManager.ConsumableGroups(FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                _blackListedConsumables.TryAdd(group.ToDefinition(), new());
            }
        }
    }


    public static List<PresetDefinition> GetPresets() {
        List<PresetDefinition> defs = new();
        foreach (Preset preset in PresetManager.Presets()) defs.Add(preset.ToDefinition());
        return defs;
    }
    
    
    [JsonIgnore]
    public IEnumerable<(IToggleable group, bool enabled, bool global)> LoadedToggleableGroups {
        get {
            foreach (DictionaryEntry entry in EnabledGroups) {
                ConsumableGroupDefinition def = (ConsumableGroupDefinition)entry.Key;
                if (!def.IsUnloaded) yield return ((IToggleable)def.ConsumableType, (bool)entry.Value!, false);
            }
            foreach ((ConsumableGroupDefinition def, bool state) in EnabledGlobals) {
                if (!def.IsUnloaded) yield return ((IToggleable)def.ConsumableType, state, true);
            }
        }
    }

    private readonly OrderedDictionary _groups = new();
    private readonly Dictionary<ConsumableGroupDefinition, bool> _globals = new();
    private readonly Dictionary<ConsumableGroupDefinition, object> _requirements = new();
    private readonly Dictionary<ConsumableGroupDefinition, HashSet<string>> _blackListedConsumables = new();

    public override ConfigScope Mode => ConfigScope.ServerSide;
    
#nullable disable
    public static RequirementSettings Instance;
#nullable restore

}

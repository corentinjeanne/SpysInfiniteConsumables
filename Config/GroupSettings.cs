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

[Label($"${Localization.Keys.GroupSettings}.Name")]
public class GroupSettings : ModConfig {

    [Header($"${Localization.Keys.GroupSettings}.General.Header")]
    [DefaultValue(true), Label($"${Localization.Keys.GroupSettings}.General.Duplication.Label"), Tooltip($"${Localization.Keys.GroupSettings}.General.Duplication.Tooltip")]
    public bool PreventItemDupication { get; set; }
    [Label($"${Localization.Keys.GroupSettings}.General.Preset.Label")]
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
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
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
    [Label($"${Localization.Keys.GroupSettings}.General.MaxGroups.Label"), Tooltip($"${Localization.Keys.GroupSettings}.General.MaxGroups.Tooltip")]
    public int MaxConsumableTypes { get; set; }
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
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

    [Header($"${Localization.Keys.GroupSettings}.Settings.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ConsumableGroupDefinition, object> Settings {
        get => _settings;
        set {
            _settings.Clear();
            foreach ((ConsumableGroupDefinition def, object data) in value) {
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _settings.Add(def, data);
                    continue;
                }
                if (def.ConsumableType is not IConfigurable configurable) continue;

                object settings;
                if (data is JObject jobj) settings = jobj.ToObject(configurable.SettingsType)!;
                else if(data.GetType() == configurable.SettingsType) settings = data;
                else throw new NotImplementedException();

                _settings.Add(def, settings);
            }

            foreach (IConfigurable group in InfinityManager.ConsumableGroups<IConfigurable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                ConsumableGroupDefinition def = group.ToDefinition();
                if (!_settings.ContainsKey(def)) _settings.Add(def, Activator.CreateInstance(group.SettingsType)!);
            }
        }
    }


    [Header($"${Localization.Keys.GroupSettings}.Blacklists.Header")]
    // TODO add customs
    [Label($"${Localization.Keys.GroupSettings}.Blacklists.Items.Label")]
    public HashSet<ItemDefinition> BlackListedItems { get; set; } = new();
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ConsumableGroupDefinition,HashSet<string>> BlackListedConsumables { 
        get => _blackListedConsumables;
        set {
            _blackListedConsumables.Clear();
            foreach ((ConsumableGroupDefinition def, HashSet<string> consumables) in value) {
                if (def.IsUnloaded && ModLoader.HasMod(def.Mod)) continue;
                _blackListedConsumables.Add(def, consumables);
            }
            foreach (IConsumableGroup group in InfinityManager.ConsumableGroups(FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                if(group is VanillaGroups.Mixed) continue;
                _blackListedConsumables.TryAdd(group.ToDefinition(), new());
            }
        }
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
    private readonly Dictionary<ConsumableGroupDefinition, object> _settings = new();
    private readonly Dictionary<ConsumableGroupDefinition, HashSet<string>> _blackListedConsumables = new();


    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static GroupSettings Instance = null!;
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.UI;
using SPIC.Configs.Presets;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Configs;

public class GroupSettings : ModConfig {

    [Header($"${Localization.Keys.GroupSettings}.General.Header")]
    [DefaultValue(true)]
    public bool PreventItemDupication { get; set; }
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
                if (def.ConsumableGroup is not IConfigurable configurable) continue;

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

    [Header($"${Localization.Keys.GroupSettings}.Customs.Header")]
    public Dictionary<ItemDefinition, Custom> Customs { get; set; } = new();


    public bool IsBlacklisted<TConsumable, TCount>(TConsumable consumable, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> {
        int id = group.CacheID(consumable);
        foreach ((ItemDefinition def, Custom custom) in Customs) {
            if (group.CacheID(group.ToConsumable(new(def.Type))) == id) return custom.Choice.Name == nameof(Custom.Blacklisted);
        }
        return false;
    }
    public bool HasCustomRequirement<TConsumable, TCount>(TConsumable consumable, [NotNullWhen(true)] out TCount? customCount, IConsumableGroup<TConsumable, TCount> group) where TConsumable : notnull where TCount : struct, ICount<TCount> {
        int id = group.CacheID(consumable);
        foreach ((ItemDefinition def, Custom custom) in Customs) {
            if (group.CacheID(group.ToConsumable(new(def.Type))) != id) continue;
            if (custom.Choice.Name != nameof(Custom.CustomRequirements) || !custom.CustomRequirements.TryGetValue(group.ToDefinition(), out UniversalCountWrapper? wrapper)) break;
            customCount = wrapper.As<TCount>();
            return true;
        }
        customCount = null;
        return false;
    }

    private readonly OrderedDictionary _groups = new();
    private readonly Dictionary<ConsumableGroupDefinition, bool> _globals = new();
    private readonly Dictionary<ConsumableGroupDefinition, object> _settings = new();

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static GroupSettings Instance = null!;
}
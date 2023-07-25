using System.Collections;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class MetaConfig {

    [JsonIgnore] public IMetaGroup MetaGroup {
        get => _metaGroup;
        internal set {
            _metaGroup = value;
            foreach (IModGroup group in _metaGroup.Groups) _groups.TryAdd(new ModGroupDefinition(group), group.DefaultsToOn);
        }
    }

    public PresetDefinition Preset {
        get {
            if (EnabledGroups.Count == 0) return new();
            ModPreset? preset = null;
            foreach (ModPreset p in PresetLoader.Presets) {
                if (!p.MeetsCriterias(this) || preset is not null && p.CriteriasCount < preset.CriteriasCount) continue;
                preset = p;
            }
            return preset is not null ? new(preset) : new();
        }
        set {
            if (EnabledGroups.Count == 0) return;
            PresetLoader.GetPreset(value.Mod, value.Name)?.ApplyCriterias(this);
        }
    }
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public OrderedDictionary /*<ConsumableTypeDefinition, bool>*/ EnabledGroups {
        get => _groups;
        set {
            _groups.Clear();
            foreach (DictionaryEntry entry in value) {
                ModGroupDefinition def = new((string)entry.Key);
                if (def.IsUnloaded) continue;
                bool state = entry.Value switch {
                    JObject jobj => (bool)jobj,
                    bool b => b,
                    _ => throw new System.NotImplementedException()
                };
                _groups.Add(def, state);
            }
        }
    }
    public int MaxConsumableTypes { get; set; }

    private readonly OrderedDictionary _groups = new();
    private IMetaGroup _metaGroup = null!;
}
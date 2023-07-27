using System.Collections;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class GroupConfig {

    [JsonIgnore] public IGroup Group {
        get => _group;
        internal set {
            _group = value;
            foreach (IInfinity infinity in _group.Infinities) _infinities.TryAdd(new InfinityDefinition(infinity), infinity.DefaultsToOn);
        }
    }

    public PresetDefinition Preset {
        get {
            if (EnabledInfinities.Count == 0) return new();
            Preset? preset = null;
            foreach (Preset p in PresetLoader.Presets) {
                if (p.MeetsCriterias(this) && (preset is null || p.CriteriasCount >= preset.CriteriasCount)) preset = p;
            }
            return preset is not null ? new(preset.Mod.Name, preset.Name) : new();
        }
        set {
            if (EnabledInfinities.Count == 0) return;
            PresetLoader.GetPreset(value.Mod, value.Name)?.ApplyCriterias(this);
        }
    }
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public OrderedDictionary /*<InfinityDefinition, bool>*/ EnabledInfinities {
        get => _infinities;
        set {
            _infinities.Clear();
            foreach (DictionaryEntry entry in value) {
                InfinityDefinition def = new((string)entry.Key);
                if (def.IsUnloaded) continue;
                bool state = entry.Value switch {
                    JObject jobj => (bool)jobj,
                    bool b => b,
                    _ => throw new System.NotImplementedException()
                };
                _infinities.Add(def, state);
            }
        }
    }
    public int MaxConsumableTypes { get; set; }

    private readonly OrderedDictionary _infinities = new();
    private IGroup _group = null!;
}
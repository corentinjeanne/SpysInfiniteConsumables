using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SPIC.Configs.Presets;

namespace SPIC.Configs;

public static class PresetManager {

    static PresetManager(){
        Reset();
    }

    public static void Register<TImplementation>(StaticPreset<TImplementation> preset) where TImplementation : StaticPreset<TImplementation> {
        if (preset.UID != 0) throw new System.ArgumentException("This preset has already been registered", nameof(preset));
        SpysInfiniteConsumables.Instance.Logger.Debug($"Preset {preset.InternalName} registered by {preset.Mod.Name}");
        int id = preset.UID = s_presets.Count+1;
        s_presets[id] = preset;
    }

    public static Preset Preset(int id) => s_presets[id];
    public static Preset? Preset(string mod, string Name) => s_presets.FindValue(kvp => kvp.Value.Mod.Name == mod && kvp.Value.DisplayName == Name);
    
    public static IEnumerable<Preset> Presets(bool noOrdering = false) {
        if (noOrdering) {
            foreach ((int _, Preset preset) in s_presets) yield return preset;
        } else {
            SortedDictionary<int, List<Preset>> sorted = new(new Utility.DescendingComparer<int>());
            foreach ((int _, Preset preset) in s_presets) {
                sorted.TryAdd(preset.CriteriasCount, new());
                sorted[preset.CriteriasCount].Add(preset);
            }
            foreach((int _, List<Preset> presets) in sorted){
                foreach(Preset preset in presets) yield return preset;
            }
        }
    }

    public static PresetDefinition ToDefinition(this Preset preset) => new(preset.Mod, preset.DisplayName);

    [MemberNotNull(nameof(s_presets))]
    public static void Reset() {
        if(s_presets?.Count > 0) SpysInfiniteConsumables.Instance.Logger.Debug($"Removed {s_presets.Count} presets");

        s_presets = new();
    }


    private static Dictionary<int, Preset> s_presets;
}
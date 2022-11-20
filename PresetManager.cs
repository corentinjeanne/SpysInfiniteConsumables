using System;
using System.Collections.Generic;
using SPIC.Configs.Presets;

namespace SPIC;

public static class PresetManager {


    public static void Register<TImplementation>(StaticPreset<TImplementation> type) where TImplementation : StaticPreset<TImplementation>, new() {

        if (type.UID != 0) throw new ArgumentException("This preset has already been registered", nameof(type));
        int id = type.UID = s_nextPresetID++;
        s_presets[id] = type;

    }

    public static Preset Preset(int id) => s_presets[ValidatePreset(id)];
    public static Preset Preset(string mod, string Name) => s_presets.FindValue(kvp => kvp.Key > 0 && kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);

    private class DescendingComparer<T> : IComparer<T> where T : IComparable<T> {
        public int Compare(T x, T y) {
            return y.CompareTo(x);
        }
    }

    public static IEnumerable<Preset> Presets(bool noOrdering = false) {
        if (noOrdering) {
            foreach ((int _, Preset preset) in s_presets) yield return preset;
        }else {
            SortedDictionary<int, List<Preset>> sorted = new(new DescendingComparer<int>());
            foreach ((int _, Preset preset) in s_presets) {
                sorted.TryAdd(preset.CriteriasCount, new());
                sorted[preset.CriteriasCount].Add(preset);
            }
            foreach((int _, List<Preset> presets) in sorted){
                foreach(Preset preset in presets) yield return preset;
            }
        }
    }

    private static int ValidatePreset(int id) => s_presets.ContainsKey(id) ? id :
        throw new ArgumentOutOfRangeException(nameof(id), id, "No Preset with this id exists");


    private static int s_nextPresetID = 1;
    private static readonly Dictionary<int, Preset> s_presets = new();

    public static Configs.PresetDefinition ToDefinition(this Preset type) => new(type.Mod, type.Name);

}
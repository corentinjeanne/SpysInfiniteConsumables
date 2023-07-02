using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SPIC.Configs.Presets;

public static class PresetLoader {
    internal static void Add(ModPreset preset) {
        _presets.Add(preset);
    }

    internal static void Unload() {
        _presets.Clear();
    }

    public static ModPreset? GetPreset(string mod, string name) {
        return _presets.Find(p => p.Mod.Name == mod && p.Name == name);
    }

    public static ReadOnlyCollection<ModPreset> Presets => _presets.AsReadOnly();

    private readonly static List<ModPreset> _presets = new();
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;

namespace SPIC;

public static class DisplayLoader {
    internal static void Register(Display display) => _displays.Add(display);
    public static Wrapper<T> AddConfig<T>(Display display) where T : new() {
        Wrapper<T> wrapper = new();
        _configs[display] = wrapper;
        return wrapper;
    }
    internal static void Unload(){
        _displays.Clear();
        _configs.Clear();
    }

    public static Display? GetDisplay(string mod, string name) => _displays.Find(p => p.Mod.Name == mod && p.Name == name);

    public static ReadOnlyCollection<Display> Displays => _displays.AsReadOnly();
    public static ReadOnlyDictionary<Display, Wrapper> Configs => new(_configs);

    private readonly static Dictionary<Display, Wrapper> _configs = new();
    private readonly static List<Display> _displays = new();
}
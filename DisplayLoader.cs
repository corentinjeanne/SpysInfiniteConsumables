using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;

namespace SPIC;

public static class DisplayLoader {
    internal static void Register(Display display) {
        s_displays.Add(display);
        s_defaultStates[display] = display.Enabled;
    }

    public static Wrapper<T> AddConfig<T>(Display display) where T : new() {
        Wrapper<T> wrapper = new();
        s_configs[display] = wrapper;
        return wrapper;
    }
    internal static void Unload(){
        s_displays.Clear();
        s_configs.Clear();
    }

    public static Display? GetDisplay(string mod, string name) => s_displays.Find(p => p.Mod.Name == mod && p.Name == name);

    public static bool DefaultState(this Display display) => s_defaultStates[display];

    public static ReadOnlyCollection<Display> Displays => s_displays.AsReadOnly();
    public static ReadOnlyDictionary<Display, Wrapper> Configs => new(s_configs);

    private readonly static List<Display> s_displays = new();
    private static readonly Dictionary<Display, bool> s_defaultStates = new();

    private readonly static Dictionary<Display, Wrapper> s_configs = new();
}
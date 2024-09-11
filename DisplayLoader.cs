using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpikysLib.Configs;

namespace SPIC;

public static class DisplayLoader {
    internal static void Register(Display display) {
        ConfigHelper.SetInstance(display);
        s_displays.Add(display);
    }

    internal static void Unload() {
        foreach (Display d in s_displays) ConfigHelper.SetInstance(d, true);
        s_displays.Clear();
    }

    public static Display? GetDisplay(string mod, string name) => s_displays.Find(p => p.Mod.Name == mod && p.Name == name);

    public static ReadOnlyCollection<Display> Displays => s_displays.AsReadOnly();

    private readonly static List<Display> s_displays = [];
}
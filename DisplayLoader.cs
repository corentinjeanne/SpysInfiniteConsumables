using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpikysLib.Extensions;

namespace SPIC;

public static class DisplayLoader {
    internal static void Register(Display display) {
        ModConfigExtensions.SetInstance(display);
        s_displays.Add(display);
        s_defaultEnabled[display] = display.Enabled;
    }

    internal static void Unload(){
        foreach (Display d in s_displays) {
            ModConfigExtensions.SetInstance(d, true);
            Utility.SetConfig(d, null);
        }
        s_displays.Clear();
    }

    public static Display? GetDisplay(string mod, string name) => s_displays.Find(p => p.Mod.Name == mod && p.Name == name);

    public static bool DefaultEnabled(this Display display) => s_defaultEnabled[display];

    public static ReadOnlyCollection<Display> Displays => s_displays.AsReadOnly();

    private readonly static List<Display> s_displays = new();
    private static readonly Dictionary<Display, bool> s_defaultEnabled = new();
}
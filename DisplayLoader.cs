using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SPIC.Configs;
using SPIC.Configs.UI;
using SpikysLib.Configs;
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

    public static void LoadConfig(InfinityDisplay config) {
        foreach (Display display in Displays) {
            DisplayDefinition def = new(display.Mod.Name, display.Name);
            
            FieldInfo? configField = display.GetType().GetField("Config", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            Type type = typeof(Toggle<>).MakeGenericType(configField?.FieldType ?? typeof(Empty));
            bool contained = config.Displays.TryGetValue(def, out object? val);
            INestedValue value;
            if(config.Display.TryGetValue(def, out bool enabled)) { // Compatibility version < v3.1.1
                object inner = config.Configs.TryGetValue(def, out Wrapper? w) ? ((JObject)w.Value!).ToObject(configField!.FieldType)! : new Empty();
                value = (INestedValue)Activator.CreateInstance(type, enabled, inner)!;
            } else if (!contained) value = (INestedValue)Activator.CreateInstance(type, display.DefaultEnabled(), null)!;
            else if (val is JToken token) value = (INestedValue)token.ToObject(type)!;
            else continue;

            config.Displays[def] = value;
            display.Enabled = (bool)value.Parent;
            configField?.SetValue(display, value.Value);
        }
        config.Display.Clear(); config.Configs.Clear(); // Compatibility version < v3.1.1
    }

    public static Display? GetDisplay(string mod, string name) => s_displays.Find(p => p.Mod.Name == mod && p.Name == name);

    public static bool DefaultEnabled(this Display display) => s_defaultEnabled[display];

    public static ReadOnlyCollection<Display> Displays => s_displays.AsReadOnly();

    private readonly static List<Display> s_displays = new();
    private static readonly Dictionary<Display, bool> s_defaultEnabled = new();
}
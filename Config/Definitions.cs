using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using System.Collections.Generic;
using System.Linq;

namespace SPIC.Configs;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(PresetDefinition), nameof(GetPresets), nameof(Label))]
public class PresetDefinition : EntityDefinition {
    public PresetDefinition() {}
    public PresetDefinition(int id) : base(PresetManager.Preset(id).Mod.Name, PresetManager.Preset(id).DisplayName) {}
    public PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public override int Type => Preset?.UID ?? -1;

    [JsonIgnore]
    public Preset Preset => PresetManager.Preset(Mod, Name)!;
    public static ConsumableGroupDefinition FromString(string s) => new(s);

    public string Label() { // TODO loc
        // Preset preset = Preset;
        // return System.Attribute.GetCustomAttribute(preset.GetType(), typeof(LabelKeyAttribute), true) is not LabelKeyAttribute label ? Name;
        return Name;
    }

    public static List<PresetDefinition> GetPresets() {
        List<PresetDefinition> defs = new();
        foreach (Preset preset in PresetManager.Presets()) defs.Add(preset.ToDefinition());
        return defs;
    }
}

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(ConsumableGroupDefinition), nameof(GetAllGroups), nameof(Label))]
[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ConsumableGroupDefinition]")]
public class ConsumableGroupDefinition : EntityDefinition {
    public ConsumableGroupDefinition() {}
    public ConsumableGroupDefinition(int id) : base(InfinityManager.ConsumableGroup(id).Mod.Name, InfinityManager.ConsumableGroup(id).InternalName) {}
    public ConsumableGroupDefinition(string fullName) : base(fullName) {}
    public ConsumableGroupDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public new bool IsUnloaded => Type == 0;
    public override int Type => ConsumableGroup?.UID ?? 0;

    [JsonIgnore]
    public IConsumableGroup ConsumableGroup => InfinityManager.ConsumableGroup(Mod, Name)!;
    
    public string Label() {
        if(IsUnloaded) return $"(Unloaded) {this}";
        IConsumableGroup group = ConsumableGroup;
        return $"[i:{group.IconType}] {group.Name}";
    }

    public static ConsumableGroupDefinition FromString(string s) => new(s);

    public static List<ConsumableGroupDefinition> GetAllGroups() => GetGroups(FilterFlags.NonGlobal | FilterFlags.Global);
    public static List<ConsumableGroupDefinition> GetGroups(FilterFlags flags) {
        List<ConsumableGroupDefinition> groups = new();
        groups.AddRange(InfinityManager.ConsumableGroups(flags | FilterFlags.Disabled | FilterFlags.Enabled)
            .Where(group => group != VanillaGroups.Mixed.Instance)
            .Select(group => group.ToDefinition()));
        return groups;
    }
}
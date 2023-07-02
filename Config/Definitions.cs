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

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { } // TODO test if can remove

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(PresetDefinition), nameof(GetPresets), nameof(Label), true)]
public class PresetDefinition : EntityDefinition {
    public PresetDefinition() : base(){}
    public PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(ModPreset preset) : base(preset.Mod.Name, preset.Name) {}

    public override int Type => PresetLoader.GetPreset(Mod, Name) == null ? -1 : 1;

    public string Label() => PresetLoader.GetPreset(Mod, Name)?.DisplayName.Value ?? $"(Unloaded) {this}";

    public static List<PresetDefinition> GetPresets() {
        List<PresetDefinition> defs = new();
        foreach (ModPreset preset in PresetLoader.Presets) defs.Add(new(preset));
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
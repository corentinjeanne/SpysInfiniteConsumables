using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPIC.Configs;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }

[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.PresetDefinition]")]
[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(PresetDefinition), nameof(GetPresets), nameof(Label), true)]
public class PresetDefinition : EntityDefinition {
    public PresetDefinition() : base(){}
    public PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(ModPreset preset) : base(preset.Mod.Name, preset.Name) {}

    public new bool IsUnloaded => Type == -1;
    public override int Type => PresetLoader.GetPreset(Mod, Name) is null ? -1 : 1;

    public string Label() => PresetLoader.GetPreset(Mod, Name)?.DisplayName.Value ?? $"(Unloaded) {this}";

    public List<PresetDefinition> GetPresets() {
        List<PresetDefinition> defs = new();
        foreach (ModPreset preset in PresetLoader.Presets) defs.Add(new(preset));
        return defs;
    }

    public static PresetDefinition FromString(string s) => new(s);
}

[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.MetaGroupDefinition]")]
[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(MetaGroupDefinition), nameof(GetMetaGroups), nameof(Label))]
public class MetaGroupDefinition : EntityDefinition {
    public MetaGroupDefinition() : base(){}
    public MetaGroupDefinition(string fullName) : base(fullName) {}
    public MetaGroupDefinition(IMetaGroup metaGroup) : base(metaGroup.Mod.Name, metaGroup.Name) {}

    public new bool IsUnloaded => Type == -1;
    public override int Type => InfinityManager.GetMetaGroup(Mod, Name) is null ? -1 : 1;

    public string Label() => InfinityManager.GetMetaGroup(Mod, Name)?.DisplayName.Value ?? $"(Unloaded) {this}";

    public List<MetaGroupDefinition> GetMetaGroups() {
        List<MetaGroupDefinition> defs = new();
        foreach (IMetaGroup metaGroup in InfinityManager.MetaGroups) defs.Add(new(metaGroup));
        return defs;
    }

    public static MetaGroupDefinition FromString(string s) => new(s);
}


[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(typeof(ModGroupDefinition), nameof(GetGroups), nameof(Label))]
[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ModGroupDefinition]")]
public class ModGroupDefinition : EntityDefinition {
    public ModGroupDefinition() : base() {}
    public ModGroupDefinition(string fullName) : base(fullName) {}
    public ModGroupDefinition(Mod mod, string name) : base(mod.Name, name) {}
    public ModGroupDefinition(IModGroup group) : base(group.Mod.Name, group.Name) { }

    [JsonIgnore] public IMetaGroup? MetaGroup { get; set; }


    public new bool IsUnloaded => Type == -1;
    public override int Type => InfinityManager.GetModGroup(Mod, Name) is null ? -1 : 1;


    public string Label() {
        IModGroup? group = InfinityManager.GetModGroup(Mod, Name);
        return group is null ? $"(Unloaded) {this}" : $"[i:{group.IconType}] {group.DisplayName}";
    }

    public static ModGroupDefinition FromString(string s) => new(s);

    public List<ModGroupDefinition> GetGroups() {
        List<ModGroupDefinition> defs = new();
        if(MetaGroup is null) foreach (IModGroup group in InfinityManager.Groups) defs.Add(new(group));
        else foreach (IModGroup group in MetaGroup.Groups) defs.Add(new(group));
        return defs;
    }
}
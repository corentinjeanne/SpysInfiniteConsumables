using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using System.Linq;
using System;

namespace SPIC.Configs;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(nameof(GetPresets), nameof(Label), true)]
[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.PresetDefinition]")]
public class PresetDefinition : EntityDefinition {
    public PresetDefinition() : base(){}
    public PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(ModPreset preset) : base(preset.Mod.Name, preset.Name) {}

    public override int Type => PresetLoader.GetPreset(Mod, Name) is null ? -1 : 1;

    public string Label() => PresetLoader.GetPreset(Mod, Name)?.DisplayName.Value ?? $"(Unloaded) {this}";

    public static PresetDefinition[] GetPresets() => PresetLoader.Presets.Select(preset => new PresetDefinition(preset)).ToArray();

    public static PresetDefinition FromString(string s) => new(s);
}

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(nameof(GetMetaGroups), nameof(Label))]
[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.MetaGroupDefinition]")]
public class MetaGroupDefinition : EntityDefinition {
    public MetaGroupDefinition() : base(){}
    public MetaGroupDefinition(string fullName) : base(fullName) {}
    public MetaGroupDefinition(IMetaGroup metaGroup) : base(metaGroup.Mod.Name, metaGroup.Name) {}

    public override int Type => InfinityManager.GetMetaGroup(Mod, Name) is null ? -1 : 1;

    public string Label() => InfinityManager.GetMetaGroup(Mod, Name)?.DisplayName.Value ?? $"(Unloaded) {this}";

    public static MetaGroupDefinition[] GetMetaGroups() => InfinityManager.MetaGroups.Select(meta => new MetaGroupDefinition(meta)).ToArray();

    public static MetaGroupDefinition FromString(string s) => new(s);
}


[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(nameof(GetAllGroups), nameof(Label))]
[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ModGroupDefinition]")]
public class ModGroupDefinition : EntityDefinition {
    public ModGroupDefinition() : base() {}
    public ModGroupDefinition(string fullName) : base(fullName) {}
    public ModGroupDefinition(string mod, string name) : base(mod, name) {}
    public ModGroupDefinition(IModGroup group) : this(group.Mod.Name, group.Name) { }

    public override int Type => InfinityManager.GetModGroup(Mod, Name) is null ? -1 : 1;

    public string Label() {
        IModGroup? group = InfinityManager.GetModGroup(Mod, Name);
        return group is null ? $"(Unloaded) {this}" : $"[i:{group.IconType}] {group.DisplayName}";
    }

    public static ModGroupDefinition FromString(string s) => new(s);

    public static ModGroupDefinition[] GetAllGroups() => InfinityManager.Groups.Select(group => new ModGroupDefinition(group)).ToArray();

    public ModGroupDefinition MakeForMetagroup(IMetaGroup metaGroup) => (ModGroupDefinition)Activator.CreateInstance(typeof(ModGroupDefinition<>).MakeGenericType(metaGroup.GetType()), Mod, Name)!;
}

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(nameof(GetGroups), nameof(Label))]
public class ModGroupDefinition<TMetaGroup> : ModGroupDefinition where TMetaGroup : class, IMetaGroup {
    public ModGroupDefinition(string mod, string name) : base(mod, name) { }
    public ModGroupDefinition(IModGroup group) : this(group.Mod.Name, group.Name) { }

    public static ModGroupDefinition[] GetGroups() => ModContent.GetInstance<TMetaGroup>().Groups.Select(group => new ModGroupDefinition<TMetaGroup>(group)).ToArray();
}
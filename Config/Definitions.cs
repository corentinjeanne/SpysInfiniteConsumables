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

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(nameof(GetModConsumables), nameof(Label))]
[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ModConsumableDefinition]")]
public class ModConsumableDefinition : EntityDefinition {
    public ModConsumableDefinition() : base(){}
    public ModConsumableDefinition(string fullName) : base(fullName) {}
    public ModConsumableDefinition(IModConsumable modConsumable) : base(modConsumable.Mod.Name, modConsumable.Name) {}

    public override int Type => InfinityManager.GetModConsumable(Mod, Name) is null ? -1 : 1;

    public string Label() => InfinityManager.GetModConsumable(Mod, Name)?.DisplayName.Value ?? $"(Unloaded) {this}";

    public static ModConsumableDefinition[] GetModConsumables() => InfinityManager.ModConsumables.Select(consumable => new ModConsumableDefinition(consumable)).ToArray();

    public static ModConsumableDefinition FromString(string s) => new(s);
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

    public ModGroupDefinition MakeForModConsumable(IModConsumable modConsumable) => (ModGroupDefinition)Activator.CreateInstance(typeof(ModGroupDefinition<>).MakeGenericType(modConsumable.GetType()), Mod, Name)!;
}

[CustomModConfigItem(typeof(DropDownElement)), ValuesProvider(nameof(GetGroups), nameof(Label))]
public class ModGroupDefinition<TModConsumable> : ModGroupDefinition where TModConsumable : class, IModConsumable {
    public ModGroupDefinition() : base() { }
    public ModGroupDefinition(string mod, string name) : base(mod, name) { }
    public ModGroupDefinition(IModGroup group) : this(group.Mod.Name, group.Name) { }

    public static ModGroupDefinition[] GetGroups() => ModContent.GetInstance<TModConsumable>().Groups.Select(group => new ModGroupDefinition<TModConsumable>(group)).ToArray();
}
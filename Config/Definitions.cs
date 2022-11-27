using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Config.Presets;

namespace SPIC.Config;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }

[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.PresetDefinition]")]
public class PresetDefinition : EntityDefinition {
    public PresetDefinition() {}
    public PresetDefinition(int type) : base(PresetManager.Preset(type).Mod.Name, PresetManager.Preset(type).Name) {}
    internal PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(Mod mod, string name) : base(mod.Name, name) {}


    public override int Type => Preset?.UID ?? -1;

    [JsonIgnore]
    public Preset Preset => PresetManager.Preset(Mod, Name)!;
    public static ConsumableTypeDefinition FromString(string s) => new(s);

    public string Label() {
        Preset preset = Preset;
        return System.Attribute.GetCustomAttribute(preset.GetType(), typeof(LabelAttribute), true) is not LabelAttribute label ? Name : label.Label;
    }
}

[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ConsumableTypeDefinition]")]
public class ConsumableTypeDefinition : EntityDefinition {
    public ConsumableTypeDefinition() {}
    public ConsumableTypeDefinition(int type) : base(InfinityManager.ConsumableGroup(type).Mod.Name, InfinityManager.ConsumableGroup(type).Name) {}
    internal ConsumableTypeDefinition(string fullName) : base(fullName) {}
    public ConsumableTypeDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public new bool IsUnloaded => Type == 0;
    public override int Type => ConsumableType?.UID ?? 0;

    [JsonIgnore]
    public IConsumableGroup ConsumableType => InfinityManager.ConsumableGroup(Mod, Name)!;
    public string Label() {
        IConsumableGroup type = ConsumableType;
        if(IsUnloaded) return $"(Unloaded) {this}";
        return $"[i:{type.IconType}] {(System.Attribute.GetCustomAttribute(type.GetType(), typeof(LabelAttribute), true) is not LabelAttribute label ? Name : label.Label)}";
    }

    public static ConsumableTypeDefinition FromString(string s) => new(s);
}
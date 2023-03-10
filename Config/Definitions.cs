using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.Presets;

namespace SPIC.Configs;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }


public class PresetDefinition : EntityDefinition {
    public PresetDefinition() {}
    public PresetDefinition(int id) : base(PresetManager.Preset(id).Mod.Name, PresetManager.Preset(id).Name) {}
    public PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public override int Type => Preset?.UID ?? -1;

    [JsonIgnore]
    public Preset Preset => PresetManager.Preset(Mod, Name)!;
    public static ConsumableGroupDefinition FromString(string s) => new(s);

    public string Label() {
        Preset preset = Preset;
        return System.Attribute.GetCustomAttribute(preset.GetType(), typeof(LabelAttribute), true) is not LabelAttribute label ? Name : label.Label;
    }
}


[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ConsumableGroupDefinition]")]
public class ConsumableGroupDefinition : EntityDefinition {
    public ConsumableGroupDefinition() {}
    public ConsumableGroupDefinition(int id) : base(InfinityManager.ConsumableGroup(id).Mod.Name, InfinityManager.ConsumableGroup(id).InternalName) {}
    public ConsumableGroupDefinition(string fullName) : base(fullName) {}
    public ConsumableGroupDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public new bool IsUnloaded => Type == 0;
    public override int Type => ConsumableType?.UID ?? 0;

    [JsonIgnore]
    public IConsumableGroup ConsumableType => InfinityManager.ConsumableGroup(Mod, Name)!;
    
    public string Label() {
        if(IsUnloaded) return $"(Unloaded) {this}";
        IConsumableGroup group = ConsumableType;
        return $"[i:{group.IconType}] {group.Name}";
    }

    public static ConsumableGroupDefinition FromString(string s) => new(s);
}
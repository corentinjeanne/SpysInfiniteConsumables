
using System.ComponentModel;
using Newtonsoft.Json;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;
using SPIC.Configs.Presets;

namespace SPIC.Configs;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }

[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.PresetDefinition]")]
public class PresetDefinition : EntityDefinition {
    public PresetDefinition() {}
    public PresetDefinition(int type) : base(PresetManager.Preset(type).Mod.Name, PresetManager.Preset(type).Name) {}
    internal PresetDefinition(string fullName) : base(fullName) {}
    public PresetDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public override int Type => Preset?.UID ?? -1;

    [JsonIgnore]
    public Preset Preset => PresetManager.Preset(Mod, Name);
    public static ConsumableTypeDefinition FromString(string s) => new(s);

    public string FullName() => Name;
}

[TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.ConsumableTypeDefinition]")]
public class ConsumableTypeDefinition : EntityDefinition {
    public ConsumableTypeDefinition() {}
    public ConsumableTypeDefinition(int type) : base(InfinityManager.ConsumableType(type).Mod.Name, InfinityManager.ConsumableType(type).Name) {}
    internal ConsumableTypeDefinition(string fullName) : base(fullName) {}
    public ConsumableTypeDefinition(Mod mod, string name) : base(mod.Name, name) {}

    public override int Type => ConsumableType?.UID ?? -1;

    [JsonIgnore]
    public IConsumableType ConsumableType => InfinityManager.ConsumableType(Mod, Name);

    public static ConsumableTypeDefinition FromString(string s) => new(s);
}
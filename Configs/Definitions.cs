
using System.ComponentModel;
using Newtonsoft.Json;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;

namespace SPIC.Configs;

public class ToFromStringConverterFix<T> : ToFromStringConverter<T> { }

// [TypeConverter("SPIC.Configs.ToFromStringConverterFix`1[SPIC.Configs.InfinityDefinition]")]
// public class InfinityDefinition : EntityDefinition {
//     public InfinityDefinition() {}

//     internal InfinityDefinition(string fullName) : base(fullName) {}

//     public InfinityDefinition(Mod mod, string name) : base(mod.Name, name) {}

//     public override int Type => Infinity?.UID ?? -1;

//     [JsonIgnore]
//     public Infinity Infinity => InfinityManager.Infinity(Mod, Name);

//     public static InfinityDefinition FromString(string s) => new(s);
// }

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
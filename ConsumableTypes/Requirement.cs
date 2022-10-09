using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPIC.ConsumableTypes;

// TODO save category type
public class CategoryConverter : JsonConverter<Category> {

    public override Category ReadJson(JsonReader reader, Type objectType, Category existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue category = (JValue)JToken.Load(reader);
        return new(Convert.ToByte(category.Value));
    }

    public override void WriteJson(JsonWriter writer, Category value, JsonSerializer serializer) {
        writer.WriteValue(value.Byte);
    }
}

[Terraria.ModLoader.Config.CustomModConfigItem(typeof(Configs.UI.CategoryElement))]
[JsonConverter(typeof(CategoryConverter))]
public sealed class Category {
    public byte Byte { get; init; }
    public Enum Enum { get; init; }

    public bool IsEnum => Enum is not null;
    public bool IsNone => Byte == None;
    public bool IsUnknown => Byte == Unknown;

    public Category() {
        Byte = 0;
        Enum = null;
    }
    public Category(Enum value) {
        Enum = value;
        Byte = Convert.ToByte(value);
    }
    public Category(byte value) {
        Byte = value;
        Enum = null;
    }

    // TODO >>> loc
    public override string ToString() => Enum?.ToString() ?? Byte.ToString();

    public static implicit operator byte(Category value) => value.Byte;
    public static implicit operator Category(byte value) => new(value);

    public static implicit operator Category(Enum value) => new(value);
    public static implicit operator Enum(Category value) => value.Enum;

    public const byte None = 0;
    public const byte Unknown = 255;

    public override bool Equals(object obj) => obj is Category c && c.GetHashCode() == GetHashCode();
    public override int GetHashCode() => HashCode.Combine(Byte, Enum);
}


// TODO >>> implement Requirement and infinity

using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

public class CategoryConverter : JsonConverter<Category> {

    public override Category ReadJson(JsonReader reader, Type objectType, Category existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue category = (JValue)JToken.Load(reader);
        if (category.Value is string fullName) {
            string[] parts = fullName.Split(", ", 3);
            existingValue = (Enum)Enum.Parse(Assembly.Load(parts[0]).GetType(parts[1]), parts[2]);
            existingValue.SaveEnumType = true;
            return existingValue;
        }
        return Convert.ToByte(category.Value);
    }

    public override void WriteJson(JsonWriter writer, Category value, JsonSerializer serializer) {
        if (!value.IsEnum || !value.SaveEnumType) {
            writer.WriteValue(value.Byte);
            return;
        }
        string[] parts = value.Enum.GetType().AssemblyQualifiedName.Split(", ");
        string fullID = $"{parts[1]}, {parts[0]}, {value.Enum}";
        writer.WriteValue(fullID);
    }
}

[CustomModConfigItem(typeof(Configs.UI.CategoryElement))]
[JsonConverter(typeof(CategoryConverter))]
public sealed class Category {
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
    
    public byte Byte { get; init; }
    public Enum Enum { get; init; }

    [JsonIgnore] public bool SaveEnumType { get; set; } = true;

    public bool IsEnum => Enum is not null;
    public bool IsNone => Byte == None;
    public bool IsUnknown => Byte == Unknown;

    public string Label() {
        if (!IsEnum) return Byte.ToString();
        MemberInfo enumFieldMemberInfo = Enum.GetType().GetMember(Enum.ToString())[0];
        LabelAttribute labelAttribute = (LabelAttribute)Attribute.GetCustomAttribute(enumFieldMemberInfo, typeof(LabelAttribute));
        return labelAttribute?.Label ?? Enum.ToString();
    }
    public override string ToString() => Enum?.ToString() ?? Byte.ToString();

    public static implicit operator byte(Category value) => value.Byte;
    public static implicit operator Category(byte value) => new(value);

    public static implicit operator Category(Enum value) => new(value);
    public static implicit operator Enum(Category value) => value.Enum;

    public const byte None = 0;
    public const byte Unknown = 255;

    public static bool operator ==(Category l, Category r) => l.Equals(r);
    public static bool operator !=(Category l, Category r) => !(l == r);

    public override bool Equals(object obj) => obj is Category c && Enum == c.Enum && Byte == c.Byte;
    public override int GetHashCode() => HashCode.Combine(Byte, Enum);
}

using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableGroup;

public class CategoryConverter : JsonConverter<CategoryWrapper> {

    public override CategoryWrapper ReadJson(JsonReader reader, Type objectType, CategoryWrapper existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue category = (JValue)JToken.Load(reader);
        if(!hasExistingValue) existingValue = new(byte.MinValue);
        if (category.Value is string fullName) {
            string[] parts = fullName.Split(", ", 3);
            existingValue.value = Enum.Parse(Assembly.Load(parts[0]).GetType(parts[1]), parts[2]);
            existingValue.SaveEnumType = true;
        }
        else existingValue.value = Convert.ToByte(category.Value);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, CategoryWrapper value, JsonSerializer serializer) {
        if (!value.IsEnum || !value.SaveEnumType) {
            writer.WriteValue(value.value);
            return;
        }
        string[] parts = value.value.GetType().AssemblyQualifiedName.Split(", ");
        string fullID = $"{parts[1]}, {parts[0]}, {value.value}";
        writer.WriteValue(fullID);
    }
}

[CustomModConfigItem(typeof(Configs.UI.CategoryElement))]
[JsonConverter(typeof(CategoryConverter))]
public sealed class CategoryWrapper {

    public CategoryWrapper() => value = byte.MinValue;
    public CategoryWrapper(Enum value) => this.value = value;
    public CategoryWrapper(byte value) => this.value = value;
    internal CategoryWrapper(object value) => this.value = value is byte or Enum ? value : throw new ArgumentException("The type of value must be byte or enum");


    public object value;
    public bool IsEnum => value is Enum;
    public bool SaveEnumType { get; set; } = true;

    public string Label() => ((Category)this).Label();

    public static implicit operator Category(CategoryWrapper category) => new(category.value);
}


/// <summary>
/// DO NOT use for config, use <see cref="CategoryWrapper"/> instead
/// </summary>
// ? replace by System.Enum
public struct Category {

    public Category(Enum value) => Value = value;
    public Category(byte value) => Value = value;
    internal Category(object value) => Value = value is byte or System.Enum ? value : throw new ArgumentException("The type of value must be byte or enum");

    public object Value { get; init; }

    public byte Byte => Convert.ToByte(Value);
    public Enum Enum => Value as Enum;

    public bool IsEnum => Value is Enum;
    public bool IsNone => Byte == None;
    public bool IsUnknown => Byte == Unknown;

    public override string ToString() => Enum?.ToString() ?? Byte.ToString();

    public static implicit operator byte(Category value) => value.Byte;
    public static implicit operator Category(byte value) => new(value);

    public static implicit operator Category(Enum value) => new(value);
    public static implicit operator Enum(Category value) => value.Enum;

    public const byte None = 0;
    public const byte Unknown = 255;

    public string Label() {
        if (!IsEnum) return Byte.ToString();
        MemberInfo enumFieldMemberInfo = Enum.GetType().GetMember(Enum.ToString())[0];
        LabelAttribute labelAttribute = (LabelAttribute)Attribute.GetCustomAttribute(enumFieldMemberInfo, typeof(LabelAttribute));
        return labelAttribute?.Label ?? Enum.ToString();
    }
}

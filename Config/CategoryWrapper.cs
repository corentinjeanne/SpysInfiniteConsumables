using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;

namespace SPIC.Config;

public class CategoryConverter : JsonConverter<CategoryWrapper> {

    public override CategoryWrapper ReadJson(JsonReader reader, Type objectType, CategoryWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue category = (JValue)JToken.Load(reader);
        existingValue ??= new(byte.MinValue);
        if (category.Value is string fullName) {
            string[] parts = fullName.Split(", ", 3);
            existingValue.value = Enum.Parse(Assembly.Load(parts[0]).GetType(parts[1])!, parts[2]);
            existingValue.SaveEnumType = true;
        } else existingValue.value = Convert.ToByte(category.Value);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, CategoryWrapper? value, JsonSerializer serializer) {
        if (value is null) return;
        if (!value.IsEnum || !value.SaveEnumType) {
            writer.WriteValue(value.value);
            return;
        }
        string[] parts = value.value.GetType().AssemblyQualifiedName!.Split(", ");
        string fullID = $"{parts[1]}, {parts[0]}, {value.value}";
        writer.WriteValue(fullID);
    }
}

[CustomModConfigItem(typeof(UI.CategoryElement))]
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

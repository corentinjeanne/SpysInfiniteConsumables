using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;

namespace SPIC.Configs;

public class CategoryConverter : JsonConverter<CategoryWrapper> {

    public override CategoryWrapper ReadJson(JsonReader reader, System.Type objectType, CategoryWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue category = (JValue)JToken.Load(reader);
        if (category.Value is string fullName) {
            string[] parts = fullName.Split(", ", 3);
            existingValue = new(byte.Parse(parts[2]), Assembly.Load(parts[0]).GetType(parts[1])) {
                SaveEnumType = true
            };
        } else existingValue = new(System.Convert.ToByte(category.Value), null);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, CategoryWrapper? value, JsonSerializer serializer) {
        if (value is null) return;
        if (value.type is null || !value.SaveEnumType) {
            writer.WriteValue(value.value);
            return;
        }
        string[] parts = value.type.AssemblyQualifiedName!.Split(", ");
        string fullName = $"{parts[1]}, {parts[0]}, {value.value}";
        writer.WriteValue(fullName);
    }
}


[CustomModConfigItem(typeof(UI.CategoryElement))]
[JsonConverter(typeof(CategoryConverter))]
public sealed class CategoryWrapper {

    public CategoryWrapper(){
        value = CategoryHelper.None;
        type = null;
    }
    public CategoryWrapper(byte value, System.Type? type) {
        this.value = value;
        this.type = type;
    }

    [JsonIgnore] public byte value;
    [JsonIgnore] public System.Type? type;
    [JsonIgnore] public bool SaveEnumType { get; set; }
    [JsonIgnore] public System.Enum? Enum => type is null ? null : (System.Enum)System.Enum.ToObject(type, value);

    public static CategoryWrapper From<TCategory>(TCategory category) where TCategory : System.Enum  => new(System.Convert.ToByte(category), typeof(TCategory));
    public TCategory As<TCategory>() where TCategory : System.Enum => (TCategory)System.Enum.ToObject(typeof(TCategory), value);
}

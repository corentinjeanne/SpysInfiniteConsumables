using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, Inherited = false)]
public class ChoiceAttribute : System.Attribute { }

public class MultyChoiceConverter : JsonConverter<MultyChoice> {
    public override MultyChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] MultyChoice existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (MultyChoice)System.Activator.CreateInstance(objectType)!;
        JObject obj = (JObject)JToken.Load(reader);
        JProperty value = (JProperty)obj.First!;
        existingValue.Select(value.Name);
        existingValue.Value = value.Value.ToObject(existingValue.Choice.Type);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] MultyChoice value, JsonSerializer serializer) {
        if(value is null) return;
        writer.WriteStartObject();
        writer.WritePropertyName(value.Choice.Name);
        using JsonTextReader reader = new(new StringReader(JsonConvert.SerializeObject(value.Value, serializer.Formatting)));
        writer.WriteToken(reader);
        writer.WriteEndObject();
    }
}

[JsonConverter(typeof(MultyChoiceConverter))]
[CustomModConfigItem(typeof(UI.MultyChoiceElement))]
public abstract class MultyChoice {

    [JsonIgnore] public IReadOnlyList<PropertyFieldWrapper> Choices => choices.AsReadOnly();
    [JsonIgnore] public int ChoiceIndex {
        get => _index;
        set => _index = (value + Choices.Count) % Choices.Count;
    }
    [JsonIgnore] public PropertyFieldWrapper Choice => Choices[ChoiceIndex];

    [JsonIgnore] public object? Value {
        get => Choice.GetValue(this);
        set => Choice.SetValue(this, value);
    }
    public void Select(string property) {
        for (int i = 0; i < Choices.Count; i++) {
            if (Choices[i].Name != property) continue;
            ChoiceIndex = i;
            return;
        }
    }

    public MultyChoice(){
        choices = new();
        choices.AddRange(GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.GetCustomAttribute<ChoiceAttribute>() != null).Select(p => new PropertyFieldWrapper(p)));
        choices.AddRange(GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.GetCustomAttribute<ChoiceAttribute>() != null).Select(f => new PropertyFieldWrapper(f)));
    }

    protected List<PropertyFieldWrapper> choices;
    private int _index;
}

public class MultyChoiceSimpleConverter : JsonConverter<MultyChoice> {
    public override MultyChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] MultyChoice existingValue, bool hasExistingValue, JsonSerializer serializer){
        existingValue ??= (MultyChoice)System.Activator.CreateInstance(objectType)!;
        return Read(reader, (dynamic?)existingValue);
    }
    private static MultyChoice<T> Read<T>(JsonReader reader, MultyChoice<T> existingValue) {
        JToken token = JToken.Load(reader);
        JValue value = (JValue)token;
        existingValue.Value = value.ToObject<T>()!;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] MultyChoice value, JsonSerializer serializer) {
        if (value is null) return;
        Write(writer, (dynamic?)value, serializer);
    }
    private static void Write<T>(JsonWriter writer, MultyChoice<T> value, JsonSerializer serializer) {
        JValue val = new(value.Value);
        writer.WriteRawValue(val.ToString(serializer.Formatting));
    }
}

[JsonConverter(typeof(MultyChoiceSimpleConverter))]
public abstract class MultyChoice<T> : MultyChoice {
    public new abstract T? Value { get; set; }
}
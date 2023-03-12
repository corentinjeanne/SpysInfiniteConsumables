using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SPIC.Configs;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class ChoiceAttribute : System.Attribute { }

public class MultyChoiceConverter : JsonConverter<MultyChoice> {
    public override MultyChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] MultyChoice existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (MultyChoice)System.Activator.CreateInstance(objectType)!;

        JProperty value = (JProperty)JToken.Load(reader).First!;
        foreach (PropertyInfo prop in existingValue.Choices) {
            if (prop.Name != value.Name) continue;
            if (prop.CanWrite) prop.SetValue(existingValue, value.Value.ToObject(prop.PropertyType));
            break;
        }
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] MultyChoice value, JsonSerializer serializer) {
        if(value is null) return;
        JObject wrapper = new(){
            new JProperty(value.Choices[value.ChoiceIndex].Name, value.Choices[value.ChoiceIndex].GetValue(value))
        };
        writer.WriteRawValue(wrapper.ToString(serializer.Formatting));
    }
}

[JsonConverter(typeof(MultyChoiceConverter))]
[CustomModConfigItem(typeof(UI.MultyChoiceElement))]
public abstract class MultyChoice {

    public ReadOnlyCollection<PropertyInfo> Choices => _choices.AsReadOnly();
    public int ChoiceIndex {
        get => _index;
        set => _index = (value + Choices.Count) % Choices.Count;
    }

    public MultyChoice(){
        _choices = new(GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.GetCustomAttribute<ChoiceAttribute>() != null));
    }

    protected void Select(string property) {
        for (int i = 0; i < Choices.Count; i++) {
            if (Choices[i].Name != property) continue;
            ChoiceIndex = i;
            return;
        }
    }

    protected List<PropertyInfo> _choices;
    private int _index;
}

public class MultyChoiceSimpleConverter : JsonConverter<MultyChoice> {
    public override MultyChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] MultyChoice existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (MultyChoice)System.Activator.CreateInstance(objectType)!;

        JValue value = (JValue)JToken.Load(reader);
        return Read((dynamic)existingValue, value);
    }

    private static MultyChoice Read<T>(MultyChoice<T> existingValue, JValue value) {
        T t = value.ToObject<T>()!;
        existingValue.Value = t;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] MultyChoice value, JsonSerializer serializer) {
        if (value is null) return;
        Write(writer, (dynamic)value, serializer);
    }
    private static void Write<T>(JsonWriter writer, MultyChoice<T> value, JsonSerializer serializer) {
        string json = JsonConvert.SerializeObject(value.Value, serializer.Formatting);
        writer.WriteRawValue(json);
    }
}

[JsonConverter(typeof(MultyChoiceSimpleConverter))]
public abstract class MultyChoice<T> : MultyChoice {

    public virtual T Value {
        get => (T)Choices[ChoiceIndex].GetValue(this)!;
        set => Choices[ChoiceIndex].SetValue(this, value);
    }
}
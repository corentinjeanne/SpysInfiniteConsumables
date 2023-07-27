using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Terraria.ModLoader.Config.UI;
using System.Collections.ObjectModel;

namespace SPIC.Configs;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, Inherited = false)]
public class ChoiceAttribute : System.Attribute {} 

public class MultyChoiceConverter : JsonConverter<MultyChoice> {
    public override MultyChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] MultyChoice existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (MultyChoice)System.Activator.CreateInstance(objectType)!;
        if(objectType.IsSubclassOfGeneric(typeof(MultyChoice<>), out System.Type? type)) {
            existingValue.Data = serializer.Deserialize(reader, type.GenericTypeArguments[0]);
        } else {
            JObject jObject = serializer.Deserialize<JObject>(reader)!;
            JProperty property = (JProperty)jObject.First!;
            existingValue.Choice = property.Name;
            existingValue.Data = property.Value.ToObject(existingValue.Choices[existingValue.ChoiceIndex].Type);
        }
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] MultyChoice value, JsonSerializer serializer) {
        if(value is null) return;
        if (value.GetType().IsSubclassOfGeneric(typeof(MultyChoice<>), out _)) {
            serializer.Serialize(writer, value?.Data);
        } else {
            writer.WriteStartObject();
            writer.WritePropertyName(value.Choice);
            serializer.Serialize(writer, value.Data);
            writer.WriteEndObject();
        }
    }
}

[JsonConverter(typeof(MultyChoiceConverter))]
[CustomModConfigItem(typeof(UI.MultyChoiceElement))]
public abstract class MultyChoice {

    [JsonIgnore] public ReadOnlyCollection<PropertyFieldWrapper> Choices { get; }
    [JsonIgnore] public int ChoiceIndex {
        get => _index;
        set => _index = (value + Choices.Count) % Choices.Count;
    }
    [JsonIgnore] public string Choice {
        get => Choices[ChoiceIndex].Name;
        set {
            for (int i = 0; i < Choices.Count; i++) {
                if (Choices[i].Name != value) continue;
                ChoiceIndex = i;
                return;
            }
        }
    }

    internal virtual object? Data {
        get => Choices[ChoiceIndex].GetValue(this);
        set => Choices[ChoiceIndex].SetValue(this, value);
    }

    public MultyChoice(){
        List<PropertyFieldWrapper> choices = new();
        choices.AddRange(GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.GetCustomAttribute<ChoiceAttribute>() != null).Select(p => new PropertyFieldWrapper(p)));
        choices.AddRange(GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(field => field.GetCustomAttribute<ChoiceAttribute>() != null).Select(f => new PropertyFieldWrapper(f)));
        Choices = choices.AsReadOnly();
    }

    private int _index;
}

public abstract class MultyChoice<T> : MultyChoice {
    internal override object? Data { get => Value; set => Value = (T?)value; }
    [JsonIgnore] public abstract T? Value { get; set; }

    public static implicit operator T?(MultyChoice<T> value) => value.Value;
}
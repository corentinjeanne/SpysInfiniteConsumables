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
public sealed class ChoiceAttribute : System.Attribute {} 

public sealed class MultiChoiceConverter : JsonConverter<MultiChoice> {
    public override MultiChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] MultiChoice existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (MultiChoice)System.Activator.CreateInstance(objectType)!;
        if(objectType.IsSubclassOfGeneric(typeof(MultiChoice<>), out System.Type? type)) {
            existingValue.Data = serializer.Deserialize(reader, type.GenericTypeArguments[0]);
        } else {
            JObject jObject = serializer.Deserialize<JObject>(reader)!;
            JProperty property = (JProperty)jObject.First!;
            existingValue.Choice = property.Name;
            existingValue.Data = property.Value.ToObject(existingValue.Choices[existingValue.ChoiceIndex].Type);
        }
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] MultiChoice value, JsonSerializer serializer) {
        if(value is null) return;
        if (value.GetType().IsSubclassOfGeneric(typeof(MultiChoice<>), out _)) {
            serializer.Serialize(writer, value?.Data);
        } else {
            writer.WriteStartObject();
            writer.WritePropertyName(value.Choice);
            serializer.Serialize(writer, value.Data);
            writer.WriteEndObject();
        }
    }
}

[JsonConverter(typeof(MultiChoiceConverter))]
[CustomModConfigItem(typeof(UI.MultiChoiceElement))]
public abstract class MultiChoice {

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

    public bool TryGet<T>(string name, [MaybeNullWhen(false)] out T? value){
        if(Choice == name) {
            value = (T?)Data;
            return true;
        }
        value = default;
        return false;
    }

    internal virtual object? Data {
        get => Choices[ChoiceIndex].GetValue(this);
        set => Choices[ChoiceIndex].SetValue(this, value);
    }

    public MultiChoice(){
        List<PropertyFieldWrapper> choices = new();
        choices.AddRange(GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.GetCustomAttribute<ChoiceAttribute>() != null).Select(p => new PropertyFieldWrapper(p)));
        choices.AddRange(GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(field => field.GetCustomAttribute<ChoiceAttribute>() != null).Select(f => new PropertyFieldWrapper(f)));
        Choices = choices.AsReadOnly();
    }

    private int _index;
}

public abstract class MultiChoice<T> : MultiChoice {
    public MultiChoice() : base() {}
    public MultiChoice(T value) : base() => Value = value;
    internal override object? Data { get => Value; set => Value = (T?)value; }
    [JsonIgnore] public abstract T? Value { get; set; }

    public static implicit operator T?(MultiChoice<T> value) => value.Value;
}
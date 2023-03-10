using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPIC.Configs;


public class MultyChoiceConverter : JsonConverter<IMultyChoice> {
    public override IMultyChoice ReadJson(JsonReader reader, System.Type objectType, [AllowNull] IMultyChoice existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue value = (JValue)JToken.Load(reader);

        existingValue ??= (IMultyChoice)System.Activator.CreateInstance(objectType)!;
        existingValue.Value = value.Value;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] IMultyChoice value, JsonSerializer serializer) {
        if(value is null) return;
        writer.WriteValue(value.Value);
    }
}

public interface IMultyChoice {
    public object? Value { get; set; }
    public PropertyInfo[] Choices { get; }
    public string ChooseProperty();
    public void ChoiceChange(string from, string to);
}
[JsonConverter(typeof(MultyChoiceConverter))]
[CustomModConfigItem(typeof(UI.MultyChoiceElement))]
public abstract class MultyChoice<T> : IMultyChoice {

    public PropertyInfo[] Choices { get; }

    public MultyChoice(){
        Choices = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.GetCustomAttribute<ChoiceAttribute>() != null).ToArray();
    }
    public T? Value { get; set; }
    object? IMultyChoice.Value { get => Value; set => Value = (T?)System.Convert.ChangeType(value, typeof(T)); }

    public abstract string ChooseProperty();
    public abstract void ChoiceChange(string from, string to);
}

[System.AttributeUsage(System.AttributeTargets.Property)]
public class ChoiceAttribute : System.Attribute {}

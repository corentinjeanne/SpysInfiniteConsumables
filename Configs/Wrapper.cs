using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public sealed class WrapperSerializer : JsonConverter<Wrapper> {
    public override Wrapper ReadJson(JsonReader reader, System.Type objectType, Wrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (Wrapper)System.Activator.CreateInstance(objectType)!;
        existingValue.Value = serializer.Deserialize(reader, objectType == typeof(Wrapper) ? typeof(JToken) : existingValue.Member.Type)!;
        return existingValue;
    }
    public override void WriteJson(JsonWriter writer, [AllowNull] Wrapper value, JsonSerializer serializer) => serializer.Serialize(writer, value?.Value);
}

public sealed class WrapperStringConverter : TypeConverter {
    public WrapperStringConverter(System.Type type) => ParentType = type;
    public override bool CanConvertTo(ITypeDescriptorContext? context, System.Type? destinationType) => destinationType != typeof(string) && InnerConvertor.CanConvertTo(context, destinationType);
    public override bool CanConvertFrom(ITypeDescriptorContext? context, System.Type sourceType) => (sourceType == typeof(string) && InnerConvertor.CanConvertFrom(context, sourceType)) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => value is string ? base.ConvertFrom(context, culture, value) : System.Activator.CreateInstance(ParentType, InnerConvertor.ConvertFrom(context, culture, value));

    public System.Type ParentType { get; }
    public TypeConverter InnerConvertor => TypeDescriptor.GetConverter(ParentType.GenericTypeArguments[0]);
}

[JsonConverter(typeof(WrapperSerializer))]
[CustomModConfigItem(typeof(UI.WrapperElement))]
[TypeConverter("SPIC.Configs.WrapperStringConverter")]
public class Wrapper {
    public Wrapper() => Value = default;
    public Wrapper(object? value) => Value = value;


    [JsonIgnore] public virtual PropertyFieldWrapper Member => new(GetType().GetProperty(GetType() == typeof(Wrapper) ? nameof(SaveBeforeEdit) : nameof(Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    [JsonIgnore] public object? Value { get; set; }

    public UI.Text SaveBeforeEdit { get; set; } = new();

    public Wrapper ChangeType(System.Type type) {
        System.Type genericType = typeof(Wrapper<>).MakeGenericType(type);
        if (GetType() == genericType) return this;
        return (Wrapper)System.Activator.CreateInstance(genericType, Value switch {
            JObject { Count: 0 } or JValue { Value: null } => System.Activator.CreateInstance(type),
            JToken token => token.ToObject(type),
            _ => System.Convert.ChangeType(Value, type),
        })!;
    }


    public override bool Equals(object? obj) => obj is Wrapper other && Value is not null && Value.Equals(other.Value);
    public override int GetHashCode() => Value!.GetHashCode();
    public override string? ToString() => Value?.ToString();

    public static Wrapper From(System.Type type) => (Wrapper)System.Activator.CreateInstance(typeof(Wrapper<>).MakeGenericType(type))!;
    public static Wrapper From(object value) => (Wrapper)System.Activator.CreateInstance(typeof(Wrapper<>).MakeGenericType(value!.GetType()), value)!;
}

public class Wrapper<T> : Wrapper where T : new() {
    public Wrapper() => Value = new();
    public Wrapper(T value) => Value = value;

    [JsonIgnore] new public T Value { get => (T)base.Value!; set => base.Value = value; }

    public static implicit operator T(Wrapper<T> wrapper) => wrapper.Value;
}
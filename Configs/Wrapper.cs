using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public sealed class WrapperSerializer : JsonConverter<IWrapper> {

    public override IWrapper ReadJson(JsonReader reader, System.Type objectType, IWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (IWrapper)System.Activator.CreateInstance(objectType)!;
        System.Type type = existingValue.GetType().GenericTypeArguments[0];
        existingValue.Value = serializer.Deserialize(reader, type == typeof(object) ? typeof(JToken) : type);
        return existingValue;
    }
    public override void WriteJson(JsonWriter writer, [AllowNull] IWrapper value, JsonSerializer serializer) => serializer.Serialize(writer, value?.Value);
}

public sealed class WrapperStringConverter : TypeConverter {
    public WrapperStringConverter(System.Type type) => ParentType = type;
    public override bool CanConvertTo(ITypeDescriptorContext? context, System.Type? destinationType) => (destinationType != typeof(string) && InnerConvertor.CanConvertTo(context, destinationType));
    public override bool CanConvertFrom(ITypeDescriptorContext? context, System.Type sourceType) => (sourceType == typeof(string) && InnerConvertor.CanConvertFrom(context, sourceType)) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => value is string ? base.ConvertFrom(context, culture, value) : System.Activator.CreateInstance(ParentType, InnerConvertor.ConvertFrom(context, culture, value));

    public System.Type ParentType { get; }
    public TypeConverter InnerConvertor => TypeDescriptor.GetConverter(ParentType.GenericTypeArguments[0]);
}

public interface IWrapper {
    PropertyFieldWrapper Member { get; }
    object? Value { get; set; }
}

[JsonConverter(typeof(WrapperSerializer))]
[CustomModConfigItem(typeof(UI.WrapperElement))]
[TypeConverter("SPIC.Configs.WrapperStringConverter")]
public class WrapperBase<TBase> : IWrapper where TBase : new() {
    public WrapperBase() => Value = new();
    public WrapperBase(TBase value) => Value = value;

    [JsonIgnore] public virtual PropertyFieldWrapper Member => new(typeof(WrapperBase<TBase>).GetField(nameof(SaveBeforeEdit), BindingFlags.Public | BindingFlags.Instance));
    [JsonIgnore] public TBase Value { get; set; }

    public UI.Text SaveBeforeEdit = new();

    public WrapperBase<TBase> ChangeType(System.Type type) {
        System.Type genericType = typeof(Wrapper<,>).MakeGenericType(type, typeof(TBase));
        if (GetType() == genericType) return this;
        return (WrapperBase<TBase>)System.Activator.CreateInstance(genericType, Value switch {
            JObject { Count: 0 } or JValue { Value: null } => System.Activator.CreateInstance(type),
            JToken token => token.ToObject(type),
            _ => System.Convert.ChangeType(Value, type),
        })!;
    }

    public override bool Equals(object? obj) => obj is WrapperBase<TBase> other && Value is not null && Value.Equals(other.Value);
    public override int GetHashCode() => Value!.GetHashCode();
    public override string? ToString() => Value?.ToString();

    object? IWrapper.Value { get => Value; set => Value = (TBase)value!; }

    public static implicit operator TBase(WrapperBase<TBase> wrapper) => wrapper.Value;

    public static WrapperBase<TBase> From(System.Type type) => (WrapperBase<TBase>)System.Activator.CreateInstance(typeof(Wrapper<,>).MakeGenericType(type, typeof(TBase)))!;
    public static WrapperBase<TBase> From(TBase value) => (WrapperBase<TBase>)System.Activator.CreateInstance(typeof(Wrapper<,>).MakeGenericType(value!.GetType(), typeof(TBase)), value)!;
}

public class Wrapper<T, TBase> : WrapperBase<TBase> where T : TBase, new() where TBase : new() {
    public Wrapper() => Value = new();
    public Wrapper(T value) => Value = value;
    public override PropertyFieldWrapper Member => new(typeof(Wrapper<T, TBase>).GetProperty(nameof(Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!);
    [JsonIgnore] new public T Value { get => (T)base.Value!; set => base.Value = value; }

    public static implicit operator T(Wrapper<T, TBase> wrapper) => wrapper.Value;
}

public sealed class Wrapper<T> : Wrapper<T, object?> where T : new() {
    public Wrapper() : base() { }
    public Wrapper(T value) : base(value) { }
}
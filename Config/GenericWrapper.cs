using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public interface IGenericWrapperBase {
    PropertyFieldWrapper Member { get; }
    object? Value { get; set; }
}

public class GenericWrapperSerializer : JsonConverter<IGenericWrapperBase> {

    public override IGenericWrapperBase ReadJson(JsonReader reader, System.Type objectType, IGenericWrapperBase? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        existingValue ??= (IGenericWrapperBase)System.Activator.CreateInstance(objectType)!;
        existingValue.Value = serializer.Deserialize(reader, existingValue.Member.Type);
        return existingValue;
    }
    public override void WriteJson(JsonWriter writer, [AllowNull] IGenericWrapperBase value, JsonSerializer serializer) => serializer.Serialize(writer, value?.Value);
}

public class GenericWrapperConverter : TypeConverter {
    public GenericWrapperConverter(System.Type type) => ParentType = type;
    public override bool CanConvertTo(ITypeDescriptorContext? context, System.Type? destinationType) => InnerConvertor.CanConvertTo(context, destinationType);
    public override bool CanConvertFrom(ITypeDescriptorContext? context, System.Type sourceType) => InnerConvertor.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => System.Activator.CreateInstance(ParentType, InnerConvertor.ConvertFrom(context, culture, value));
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, System.Type destinationType) => InnerConvertor.ConvertTo(context, culture, ((IGenericWrapperBase?)value)?.Value, destinationType);
    public System.Type ParentType { get; }
    public TypeConverter InnerConvertor => TypeDescriptor.GetConverter(ParentType.GenericTypeArguments[0]);
}

[JsonConverter(typeof(GenericWrapperSerializer))]
[CustomModConfigItem(typeof(UI.GenericWrapperElement))]
[TypeConverter("SPIC.Configs.GenericConverter")]
public class GenericWrapper<TBase> : IGenericWrapperBase where TBase: new() {
    public GenericWrapper() => Value = new();
    public GenericWrapper(TBase value) => Value = value;
    public static GenericWrapper<TBase> From(System.Type type) => (GenericWrapper<TBase>)System.Activator.CreateInstance(typeof(GenericWrapper<,>).MakeGenericType(type, typeof(TBase)))!;
    public static GenericWrapper<TBase> From(TBase value) => (GenericWrapper<TBase>)System.Activator.CreateInstance(typeof(GenericWrapper<,>).MakeGenericType(value.GetType(), typeof(TBase)), value)!;

    [JsonIgnore] public virtual PropertyFieldWrapper Member => new(typeof(GenericWrapper<TBase>).GetProperty(nameof(Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    public TBase Value { get; set; }

    public GenericWrapper<TValue, TBase> MakeGeneric<TValue>() where TValue : TBase, new() => (MakeGeneric(typeof(TValue)) as GenericWrapper<TValue, TBase>)!;
    public GenericWrapper<TBase> MakeGeneric(System.Type type) {
        System.Type genericType = typeof(GenericWrapper<,>).MakeGenericType(type, typeof(TBase));
        if (GetType() == genericType) return this;
        return (GenericWrapper<TBase>)System.Activator.CreateInstance(genericType, Value switch {
            JObject { Count: 0 } or JValue { Value: null } => System.Activator.CreateInstance(type),
            JToken token => token.ToObject(type),
            _ => System.Convert.ChangeType(Value, type), // TODO
        })!;
    }

    public override bool Equals(object? obj) => obj is GenericWrapper<TBase> other && (Value?.Equals(other.Value) ?? other is null);
    public override int GetHashCode() => Value!.GetHashCode();
    public override string? ToString() => Value?.ToString();

    object? IGenericWrapperBase.Value { get => Value; set => Value = (TBase)value!; }
}

public sealed class GenericWrapper<T, TBase> : GenericWrapper<TBase> where T : TBase, new() where TBase : new() {
    public GenericWrapper() => Value = new();
    public GenericWrapper(T value) => Value = value;
    public override PropertyFieldWrapper Member => new(typeof(GenericWrapper<T, TBase>).GetProperty(nameof(Value), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!);
    [JsonIgnore] new public T Value { get => (T)base.Value!; set => base.Value = value; }
}
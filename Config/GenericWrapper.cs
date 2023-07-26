using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public class GenericWrapperConverter : JsonConverter<GenericWrapper> {

    public override GenericWrapper ReadJson(JsonReader reader, System.Type objectType, GenericWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JToken value = JToken.Load(reader);
        existingValue ??= new();
        existingValue.Value = existingValue.GetType() == typeof(GenericWrapper) ? value : value.ToObject(existingValue.Member.Type);
        return existingValue;
    }
    public override void WriteJson(JsonWriter writer, [AllowNull] GenericWrapper value, JsonSerializer serializer) => serializer.Serialize(writer, value?.Value);
}

[JsonConverter(typeof(GenericWrapperConverter))]
[CustomModConfigItem(typeof(UI.GenericWrapperElement))]
public class GenericWrapper {

    public GenericWrapper() => Value = null;
    public GenericWrapper(object? value) => Value = value;

    [JsonIgnore] public virtual PropertyFieldWrapper Member => new(typeof(GenericWrapper).GetField(nameof(Value), BindingFlags.Public | BindingFlags.Instance));
    [JsonIgnore] public object? Value = null;
    
    public GenericWrapper<T?> MakeGeneric<T>() where T : new() => new(Value is JToken token ? token.ToObject<T>() : (T?)Value ?? default);
    public GenericWrapper MakeGeneric(System.Type type) {
        object value = (Value switch {
            JObject { Count: 0}   => System.Activator.CreateInstance(type),
            JValue { Value: null} => System.Activator.CreateInstance(type),
            JToken token          => token.ToObject(type),
            _                     => System.Convert.ChangeType(Value, type),
        })!;
        return (GenericWrapper)System.Activator.CreateInstance(typeof(GenericWrapper<>).MakeGenericType(type), value)!;
    }
}

public sealed class GenericWrapper<T> : GenericWrapper where T : new() {
    public GenericWrapper() : base(new T()) {}
    public GenericWrapper(T value) : base(value) {}
    public override PropertyFieldWrapper Member => new(typeof(GenericWrapper<T>).GetProperty(nameof(Value), BindingFlags.Public | BindingFlags.Instance)!);
    [JsonIgnore] new public T Value { get => (T)base.Value!; set => base.Value = value; }
}
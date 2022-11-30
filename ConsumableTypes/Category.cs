using System;
using System.Reflection;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableGroup;

/// <summary>
/// DO NOT use for config, use <see cref="Config.CategoryWrapper"/> instead
/// </summary>
public struct Category {

    public Category() => Value = 0;
    public Category(Enum value) => Value = value;
    public Category(byte value) => Value = value;
    internal Category(object value) => Value = value is byte or System.Enum ? value : throw new ArgumentException("The type of value must be byte or enum");

    public object Value { get; init; }

    public byte Byte => Convert.ToByte(Value);
    public Enum? Enum => Value as Enum;

    public bool IsEnum => Value is Enum;
    public bool IsNone => Byte == None;
    public bool IsUnknown => Byte == Unknown;

    public string Label() {
        if (!IsEnum) return Byte.ToString();
        MemberInfo enumFieldMemberInfo = Enum!.GetType().GetMember(Enum.ToString())[0];
        LabelAttribute? labelAttribute = (LabelAttribute?)Attribute.GetCustomAttribute(enumFieldMemberInfo, typeof(LabelAttribute));
        return labelAttribute?.Label ?? Enum.ToString();
    }
    public override string ToString() => Enum?.ToString() ?? Byte.ToString();

    public static implicit operator byte(Category value) => value.Byte;
    public static implicit operator Category(byte value) => new(value);

    public static implicit operator Category(Enum value) => new(value);
    public static implicit operator Enum?(Category value) => value.Enum;

    public const byte None = 0;
    public const byte Unknown = 255;

}

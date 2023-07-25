using System;
using System.Reflection;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs.UI;

public class CategoryElement : ConfigElement<CategoryWrapper> {

    public class EnumProp<TEnum> where TEnum : Enum {

        private readonly Func<byte> _getter;
        private readonly Action<byte> _setter;

        public EnumProp(Func<byte> getter, Action<byte> setter) {
            _getter = getter;
            _setter = setter;
        }
        public TEnum Enum { get => (TEnum)System.Enum.ToObject(typeof(TEnum), _getter()); set => _setter(System.Convert.ToByte(value)); }
    }

    private static readonly PropertyInfo s_byteProp = typeof(CategoryElement).GetProperty(nameof(Byte), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private byte Byte { get => Value.value; set => Value.value = value; }
    private object? _enum;

    public override void OnBind() {
        base.OnBind();
        CategoryWrapper value = (CategoryWrapper)MemberInfo.GetValue(Item);

        int top = 0;
        UIElement container, element;
        string label = Label ?? MemberInfo.Name;
        if (value.type is not null) {
            Type genType = typeof(EnumProp<>).MakeGenericType(value.type);
            PropertyInfo enumProp = genType.GetProperty(nameof(EnumProp<Enum>.Enum), BindingFlags.Public | BindingFlags.Instance)!;
            Func<byte> getter = () => value.value;
            Action<byte> setter = (byte b) => Value.value = b;
            _enum = Activator.CreateInstance(genType, new object[] { getter, setter })!;
            (container, element) = ConfigManager.WrapIt(this, ref top, new(enumProp), _enum, 0);
            TextDisplayFunction = () => $"{Label ?? MemberInfo.Name}: {Value.Enum!.Label()}";
        } else {
            (container, element) = ConfigManager.WrapIt(this, ref top, new(s_byteProp), this, 0);
            TextDisplayFunction = () => $"{Label ?? MemberInfo.Name}: {Byte}";
        }

        ReflectionHelper.ConfigElement_DrawLabel.SetValue(element, false);
        ReflectionHelper.ConfigElement_backgroundColor.SetValue(element, new Microsoft.Xna.Framework.Color(0, 0, 0, 0));
        container.Left.Pixels -= 20;
        container.Width.Pixels += 20;
        Height = container.Height;
    }
}

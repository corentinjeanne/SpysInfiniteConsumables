using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using SPIC.ConsumableGroup;

namespace SPIC.Configs.UI;

public class CategoryElement : ConfigElement<CategoryWrapper> {

    public class EnumProp<TEnum> where TEnum : Enum {

        private readonly Func<Enum> _getter;
        private readonly Action<Enum> _setter;

        public EnumProp(Func<Enum> getter, Action<Enum> setter) {
            _getter = getter;
            _setter = setter;
        }
        public TEnum Enum { get => (TEnum)_getter(); set => _setter(value); }
    }

    private static readonly PropertyInfo s_byteProp = typeof(CategoryElement).GetProperty(nameof(Byte), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private byte Byte { get => (byte)Value.value; set => Value.value = value; }
    private object? _enum;

    public override void OnBind() {
        base.OnBind();
        CategoryWrapper value = (CategoryWrapper)MemberInfo.GetValue(Item);

        int top = 0;
        UIElement container;
        if (value.IsEnum) {
            Type genType = typeof(EnumProp<>).MakeGenericType(((Enum)value.value).GetType());
            PropertyInfo enumProp = genType.GetProperty(nameof(EnumProp<Enum>.Enum), BindingFlags.Public | BindingFlags.Instance)!;
            Func<Enum> getter = () => (Enum)value.value;
            Action<Enum> setter = (Enum e) => Value = new(e){SaveEnumType = Value.SaveEnumType};
            _enum = Activator.CreateInstance(genType, new object[] { getter, setter });
            (container, UIElement element) = ConfigManager.WrapIt(this, ref top, new(enumProp), _enum, 0);
            string label = LabelAttribute?.Label ?? MemberInfo.Name;
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => label + ": " + Value.Label());
        } else {
            (container, UIElement element) = ConfigManager.WrapIt(this, ref top, new(s_byteProp), this, 0);
            string label = LabelAttribute?.Label ?? MemberInfo.Name;
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => label + ": " + Byte.ToString());
        }
        container.Left.Pixels -= 20;
        container.Width.Pixels += 20;
        Height = container.Height;
    }
    public override void Draw(SpriteBatch spriteBatch) => DrawChildren(spriteBatch);
}

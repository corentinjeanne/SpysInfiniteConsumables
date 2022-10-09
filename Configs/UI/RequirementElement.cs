using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using SPIC.ConsumableTypes;

namespace SPIC.Configs.UI;

public class CategoryElement : ConfigElement<Category> {

    public class EnumProp<TEnum> where TEnum : Enum {

        private readonly Func<Enum> _getter;
        private readonly Action<Enum> _setter;

        public EnumProp(Func<Enum> getter, Action<Enum> setter) {
            _getter = getter;
            _setter = setter;
        }
        public TEnum Enum { get => (TEnum)_getter(); set => _setter(value); }
    }

    private static readonly PropertyInfo s_byteProp = typeof(CategoryElement).GetProperty(nameof(Byte), BindingFlags.NonPublic | BindingFlags.Instance);

    private byte Byte { get => Value; set => Value = new(value); }
    private object _enum;

    public override void OnBind() {
        base.OnBind();
        Category value = (Category)MemberInfo.GetValue(Item);

        int top = 0;
        UIElement container;
        if (value.IsEnum) {
            Type genType = typeof(EnumProp<>).MakeGenericType(value.Enum.GetType());
            PropertyInfo enumProp = genType.GetProperty(nameof(Enum), BindingFlags.Public | BindingFlags.Instance);
            Func<Enum> getter = () => Value.Enum;
            Action<Enum> setter = (Enum e) => Value = new(e);
            _enum = Activator.CreateInstance(genType, new object[] { getter, setter });
            (container, UIElement element) = ConfigManager.WrapIt(this, ref top, new(enumProp), _enum, 0);
            string label = LabelAttribute?.Label ?? MemberInfo.Name;
            Configs.UI.ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => label + ": " + Value.ToString());
        } else {
            (container, UIElement element) = ConfigManager.WrapIt(this, ref top, new(s_byteProp), this, 0);
            string label = LabelAttribute?.Label ?? MemberInfo.Name;
            Configs.UI.ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => label + ": " + Byte.ToString());
        }
        container.Left.Pixels -= 20;
        container.Width.Pixels += 20;
        Height = container.Height;
    }
    public override void Draw(SpriteBatch spriteBatch) => DrawChildren(spriteBatch);
}

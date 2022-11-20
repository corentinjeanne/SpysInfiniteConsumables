using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Microsoft.Xna.Framework;
using SPIC.ConsumableGroup;
using System.Reflection;

namespace SPIC.Configs.UI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class NoSwappingAttribute : Attribute { }

public class ItemCountElement : ConfigElement<ItemCountWrapper> {

    private static readonly PropertyInfo ItemProp = typeof(ItemCountElement).GetProperty(nameof(Items), BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly PropertyInfo StacksProp = typeof(ItemCountElement).GetProperty(nameof(Stacks), BindingFlags.Instance | BindingFlags.NonPublic);
    private int Items {
        get => (int)(long)Value.value;
        set => Value.value = (long)value;
    }
    private int Stacks {
        get => (int)(float)Value.value;
        set => Value.value = (float)value;
    }
    private Func<string> _parentDisplayFunction;
    private Func<string> _childTextDisplayFunction;
    private UIElement _selectedElement;

    public override void OnBind() {
        base.OnBind();

        _parentDisplayFunction = TextDisplayFunction;
        TextDisplayFunction = () => $"{_parentDisplayFunction()} ({_childTextDisplayFunction()})";

        if (ConfigManager.GetCustomAttribute<NoSwappingAttribute>(MemberInfo, Value.GetType()) is null) {
            OnClick += (UIMouseEvent a, UIElement b) => {
                Value.SwapItemsAndStacks();
                SetupMember();
                ConfigManager.SetPendingChanges();
            };
        }
        SetupMember();
    }

    public void SetupMember() {
        RemoveAllChildren();

        bool items = Value.UseItems;
        int top = 0;
        (_selectedElement, UIElement element) = ConfigManager.WrapIt(this, ref top, new(items ? ItemProp : StacksProp), this, 0);
        _selectedElement.Left.Pixels -= 20;
        _selectedElement.Width.Pixels += 20;

        _childTextDisplayFunction = (Func<string>)ReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(element);
        ReflectionHelper.ConfigElement_DrawLabel.SetValue(element, false);
        ReflectionHelper.ConfigElement_backgroundColor.SetValue(element, new Color(0, 0, 0, 0));

        Recalculate();
    }
    public override void Recalculate() {
        base.Recalculate();
        Height.Set(_selectedElement.Height.Pixels, 0f);
    }
}
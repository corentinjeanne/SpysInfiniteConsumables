using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace SPIC.Config.UI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class NoSwappingAttribute : Attribute { }

public class ItemCountElement : ConfigElement<ItemCountWrapper> {

    private static readonly PropertyInfo ValueProp = typeof(ItemCountElement).GetProperty(nameof(CountValue), BindingFlags.Instance | BindingFlags.NonPublic)!;
    [Range(0,9999)]
    private int CountValue {
        get => Value.value;
        set => Value.value = value;
    }

#nullable disable
    private Func<string> _parentDisplayFunction;
    private UIElement _selectedElement;
#nullable restore
    public override void OnBind() {
        base.OnBind();

        _parentDisplayFunction = TextDisplayFunction;
        TextDisplayFunction = () => $"{_parentDisplayFunction()} ({(Value.useStacks ? "Stacks" : "Items")})";

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

        int top = 0;
        (_selectedElement, UIElement element) = ConfigManager.WrapIt(this, ref top, new(ValueProp), this, 0);
        _selectedElement.Left.Pixels -= 20;
        _selectedElement.Width.Pixels += 20;

        ReflectionHelper.ConfigElement_DrawLabel.SetValue(element, false);
        ReflectionHelper.ConfigElement_backgroundColor.SetValue(element, new Color(0, 0, 0, 0));

        Recalculate();
    }
    public override void Recalculate() {
        base.Recalculate();
        Height.Set(_selectedElement.Height.Pixels, 0f);
    }
}
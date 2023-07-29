using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.GameContent.UI.States;
using Microsoft.Xna.Framework;

namespace SPIC.Configs.UI;

public sealed class WrapperElement : ConfigElement<Wrapper> {

    public override void OnBind() {
        base.OnBind();
        Wrapper wrapper = Value;

        int top = 0;
        PropertyFieldWrapper member = wrapper.Member;
        (UIElement container, child) = ConfigManager.WrapIt(this, ref top, member, wrapper, 0);
        container.Left.Pixels -= 20;
        container.Width.Pixels += 20;

        ReflectionHelper.ConfigElement_backgroundColor.SetValue(child, Color.Transparent);
        Func<string> childText = (Func<string>)ReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(child)!;
        ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(child, wrapper.Member.Name == nameof(Wrapper.Value) ? () => $"{TextDisplayFunction()}{childText()[member.Name.Length..]}" : () => $"{TextDisplayFunction()} ({childText()})");
        ReflectionHelper.ConfigElement_TooltipFunction.SetValue(child, TooltipFunction);
        DrawLabel = false;
        TooltipFunction = null;
        MaxHeight.Pixels = int.MaxValue;
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        Height = child.Height;
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(Height.Pixels, 0f);
    }

    private UIElement child = null!;
}

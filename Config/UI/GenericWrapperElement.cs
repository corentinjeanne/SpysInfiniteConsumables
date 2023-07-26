using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.GameContent.UI.States;
using Microsoft.Xna.Framework;

namespace SPIC.Configs.UI;

public class GenericWrapperElement : ConfigElement<GenericWrapper> {

    private UIElement child = null!;

    public override void OnBind() {
        base.OnBind();
        GenericWrapper wrapper = Value;

        int top = 0;
        (UIElement container, child) = ConfigManager.WrapIt(this, ref top, wrapper.Member, wrapper, 0);
        container.Left.Pixels -= 20;
        container.Width.Pixels += 20;

        ReflectionHelper.ConfigElement_backgroundColor.SetValue(child, Color.Transparent);
        DrawLabel = false;
        Func<string> childText = (Func<string>)ReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(child)!;
        ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(child, () => $"{TextDisplayFunction()}{childText()[5..]}");
        ReflectionHelper.ConfigElement_TooltipFunction.SetValue(child, TooltipFunction);
        MaxHeight.Pixels = int.MaxValue;
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        Height = child.Height;
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(Height.Pixels, 0f);
    }
}

using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;

namespace SPIC.Configs.UI;

public class MultyChoiceElement : ConfigElement<MultyChoice> {

    public override void OnBind() {
        base.OnBind();
        SetupMember();
    }

    public void SetupMember(){
        RemoveAllChildren();

        MultyChoice value = Value ??= (MultyChoice)Activator.CreateInstance(MemberInfo.Type)!;
        PropertyFieldWrapper selectedProp = value.Choices[value.ChoiceIndex];

        int top = 0;
        (UIElement container, _selectedElement) = ConfigManager.WrapIt(this, ref top, selectedProp, value, 0);
        container.Left.Pixels -= 20;
        container.Width.Pixels -= 7;

        _tooltip = TooltipFunction;
        DrawLabel = false;
        TooltipFunction = null;
        
        MaxHeight.Pixels = int.MaxValue;
        ReflectionHelper.ConfigElement_backgroundColor.SetValue(_selectedElement, Color.Transparent);

        Func<string> elementLabel = (Func<string>)ReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(_selectedElement)!;
        Func<string>? elementTooltip = (Func<string>?)ReflectionHelper.ConfigElement_TooltipFunction.GetValue(_selectedElement);
        ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(_selectedElement, () => $"{TextDisplayFunction()}: {elementLabel()}");
        ReflectionHelper.ConfigElement_TooltipFunction.SetValue(_selectedElement, () => {
            List<string> parts = new();
            string? p = null;
            if ((p = _tooltip?.Invoke()) is not null && p.Length != 0) parts.Add(p);
            if ((p = elementTooltip?.Invoke()) is not null && p.Length != 0) parts.Add(p);
            return string.Join('\n', parts);
        });

        int count = value.Choices.Count;
        UIImage swapButton;
        if(count == 2){
            swapButton = new HoverImage(PlayTexture, Language.GetTextValue($"{Localization.Keys.UI}.Change", value.Choices[(value.ChoiceIndex+1) % count].Name));
            swapButton.OnLeftClick += (UIMouseEvent a, UIElement b) => ChangeChoice(value.ChoiceIndex + 1);
        }
        else {
            swapButton = new HoverImageSplit(UpDownTexture, Language.GetTextValue($"{Localization.Keys.UI}.Change", value.Choices[(value.ChoiceIndex+1) % count].Name), Language.GetTextValue($"{Localization.Keys.UI}.Change", value.Choices[(value.ChoiceIndex-1 + count) % count].Name));
            swapButton.OnLeftClick += (UIMouseEvent a, UIElement b) => ChangeChoice(value.ChoiceIndex + (((HoverImageSplit)swapButton).HoveringUp ? 1 : -1));
        }
        swapButton.VAlign = 0.5f;
        swapButton.Left.Set(-30 + 5, 1);
        Append(swapButton);
        Recalculate();
    }

    public void ChangeChoice(int index){
        Value.ChoiceIndex = index;
        SetupMember();
        ConfigManager.SetPendingChanges();
    }

    public override void Recalculate() {
        base.Recalculate();
        Height = _selectedElement.Height;
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(Height.Pixels, 0f);
    }

    private UIElement _selectedElement = null!;
    private Func<string>? _tooltip = null;

}
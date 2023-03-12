using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;

namespace SPIC.Configs.UI;

public class MultyChoiceElement : ConfigElement<MultyChoice> {

    public UIElement selectedElement = null!;

    public override void OnBind() {
        base.OnBind();
        SetupMember();
    }

    public void SetupMember(){
        RemoveAllChildren();

        MultyChoice value = Value;
        value ??= Value = (MultyChoice)Activator.CreateInstance(MemberInfo.Type)!;
        PropertyFieldWrapper selectedProp = value.Choices[value.ChoiceIndex];

        int top = 0;
        (UIElement coutainer, selectedElement) = ConfigManager.WrapIt(this, ref top, selectedProp, value, 0);
        if (selectedProp.Type == typeof(object) && !selectedProp.CanWrite) {
            selectedElement.RemoveAllChildren();
            ReflectionHelper.ObjectElement_pendindChanges.SetValue(selectedElement, false);
        }
        coutainer.Left.Pixels -= 20;
        coutainer.Width.Pixels -= 7;

        DrawLabel = false;
        MaxHeight.Pixels = int.MaxValue;
        ReflectionHelper.ConfigElement_backgroundColor.SetValue(selectedElement, Color.Transparent);

        Func<string> elementLabel = (Func<string>)ReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(selectedElement)!;
        Func<string>? elementTooltip = (Func<string>?)ReflectionHelper.ConfigElement_TooltipFunction.GetValue(selectedElement);
        ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(selectedElement, () => $"{TextDisplayFunction()} ({(Value is ItemCountWrapper w && selectedProp.Name == nameof(ItemCountWrapper.Stacks) ? string.Format(elementLabel(), w.MaxStack) : elementLabel())})");
        ReflectionHelper.ConfigElement_TooltipFunction.SetValue(selectedElement, () => {
            List<string> parts = new();
            if (TooltipFunction is not null) parts.Add(TooltipFunction());
            if (elementTooltip is not null) parts.Add(elementTooltip());
            return string.Join('\n', parts);
        });

        int count = value.Choices.Count;
        UIImage swapButton;
        if(count == 2){
            swapButton = new HoverImage(PlayTexture, $"Change to {Value.Choices[(value.ChoiceIndex+1) % count].Name}");
            swapButton.OnClick += (UIMouseEvent a, UIElement b) => ChangeChoice(value.ChoiceIndex + 1);
        }
        else {
            swapButton = new HoverImageSplit(UpDownTexture, $"Change to {Value.Choices[(value.ChoiceIndex+1) % count].Name}", $"Change to {Value.Choices[(value.ChoiceIndex-1 + count) % count].Name}");
            swapButton.OnClick += (UIMouseEvent a, UIElement b) => ChangeChoice(value.ChoiceIndex + (((HoverImageSplit)swapButton).HoveringUp ? 1 : -1));
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
        Height = selectedElement.Height;
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(Height.Pixels, 0f);
    }
}
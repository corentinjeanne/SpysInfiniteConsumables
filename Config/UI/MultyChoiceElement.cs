using System;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace SPIC.Configs.UI;

public class MultyChoiceElement : ConfigElement<IMultyChoice> {

    public int selectedPropIndex = -1;
    public PropertyInfo selectedProp = null!;
    public UIElement selectedElement = null!;

    public override void OnBind() {
        base.OnBind();
        SetupMember();
    }

    public void SetupMember(){
        RemoveAllChildren();

        IMultyChoice value = Value;
        int top = 0;
        string sel = value.ChooseProperty();
        selectedPropIndex = Array.FindIndex(value.Choices, p => p.Name == sel)!;
        selectedProp = value.Choices[selectedPropIndex];

        (UIElement coutainer, selectedElement) = ConfigManager.WrapIt(this, ref top, new(selectedProp), value, 0);
        if (selectedProp.PropertyType == typeof(object) && !selectedProp.CanWrite) {
            selectedElement.RemoveAllChildren();
            ReflectionHelper.ObjectElement_pendindChanges.SetValue(selectedElement, false);
        }
        coutainer.Left.Pixels -= 20;
        coutainer.Width.Pixels -= 3;

        DrawLabel = false;
        ReflectionHelper.ConfigElement_backgroundColor.SetValue(selectedElement, Color.Transparent);

        Func<string> elementLabel = (Func<string>)ReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(selectedElement)!;
        Func<string>? elementTooltip = (Func<string>?)ReflectionHelper.ConfigElement_TooltipFunction.GetValue(selectedElement);
        ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(selectedElement, () => $"{TextDisplayFunction()} ({(Value is ItemCountWrapper w && selectedProp.Name == nameof(ItemCountWrapper.Stacks) ? string.Format(elementLabel(), w.maxStack) : elementLabel())})");
        ReflectionHelper.ConfigElement_TooltipFunction.SetValue(selectedElement, () => {
            List<string> parts = new();
            if (TooltipFunction is not null) parts.Add(TooltipFunction());
            if (elementTooltip is not null) parts.Add(elementTooltip());
            return string.Join('\n', parts);
        });

        int count = value.Choices.Length;
        UIImage swapButton;
        if(count == 2){
            swapButton = new HoverImage(PlayTexture, $"Change to {Value.Choices[(selectedPropIndex + 1) % count].Name}");
            swapButton.OnClick += (UIMouseEvent a, UIElement b) => {
                Value.ChoiceChange(selectedProp.Name, Value.Choices[(selectedPropIndex+1) % count].Name);
                SetupMember();
                ConfigManager.SetPendingChanges();
            };
        }
        else {
            swapButton = new HoverImageSplit(UpDownTexture, $"Change to {Value.Choices[(selectedPropIndex + 1) % count].Name}", $"Change to {Value.Choices[(selectedPropIndex - 1 + count) % count].Name}");
            swapButton.OnClick += (UIMouseEvent a, UIElement b) => {
                Value.ChoiceChange(selectedProp.Name, Value.Choices[(selectedPropIndex + (((HoverImageSplit)swapButton).HoveringUp ? 1 : -1) + count) % count].Name);
                SetupMember();
                ConfigManager.SetPendingChanges();
            };
        }
        swapButton.Left.Set(-30 + 3, 1);
        swapButton.Top.Pixels += 4;
        Append(swapButton);
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        Height = selectedElement.Height;
    }
}
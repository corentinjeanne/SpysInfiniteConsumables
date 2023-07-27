using System;
using System.Collections;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace SPIC.Configs.UI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
sealed class ValuesProviderAttribute : Attribute {

    public readonly string Values;
    public readonly string Label;
    public readonly bool NoneAllowed;

    public ValuesProviderAttribute(string values, string label, bool allowNone = false){
        Values = values;
        Label = label;
        NoneAllowed = allowNone;
        
    }
}

public class EmptyClass {}

public class DropDownElement : ConfigElement {

    public override void OnBind() {
        base.OnBind();
        object value = MemberInfo.GetValue(Item);

        _provider = (MemberInfo.MemberInfo.GetCustomAttribute<ValuesProviderAttribute>()
            ?? MemberInfo.Type.GetCustomAttribute<ValuesProviderAttribute>())
            ?? throw new MissingMemberException($"Drop down element requires the Atrribute {nameof(ValuesProviderAttribute)}");
        
        _label = value.GetType().GetMethod(_provider.Label, BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) ?? throw new ArgumentException($"Could not find instanced method {_provider.Label}");
       
        MethodInfo valueGetter = value.GetType().GetMethod(_provider.Values, BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes) ?? throw new ArgumentException($"Could not find static method {_provider.Values}");
        if (!valueGetter.ReturnType.ImplementsInterface(typeof(IList), out _)) throw new ArgumentException($"The return type of {_provider.Values} must be {typeof(IList)}");

        _values = (IList)valueGetter.Invoke(null, null)!;
        _index = _values.IndexOf(value);

        Func<string> label = TextDisplayFunction;
        TextDisplayFunction = () => $"{label()}: {(_index == -1 ? Language.GetTextValue($"{Localization.Keys.UI}.None") : (string)_label.Invoke(_values[_index], null)!)}";
        OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => {
            if(_expanded) CloseDropDownField(_index);
            else OpenDropDownField();
        };

        _dataList.Top = new(30, 0f);
        _dataList.Left = new(7, 0f);
        _dataList.Height = new(-7, 1f);
        _dataList.Width = new(-7*2, 1f);
        _dataList.ListPadding = 7f;
        MaxHeight.Pixels = int.MaxValue;

        SetupList();
    }

    public void SetupList(){
        _dataList.Clear();
        for (int i = 0; i < _values.Count; i++) {
            int top = 0;
            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, new(s_dummyField), this, i);
            int index = i;
            container.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => CloseDropDownField(index);
            string? name = (string?)_label.Invoke(_values[i], null);
            element.RemoveAllChildren();
            ReflectionHelper.ObjectElement_pendindChanges.SetValue(element, false);
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => name);
        }
    }

    public void OpenDropDownField() {
        _expanded = true;
        Append(_dataList);
        Recalculate();
    }

    public void CloseDropDownField(int index) {
        if(!_provider.NoneAllowed && index < 0) return;
        if(_index != index) MemberInfo.SetValue(Item, _values[index]);
        _index = index;

        RemoveChild(_dataList);
        _expanded = false;
        ConfigManager.SetPendingChanges();
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        float height = (_dataList.Parent != null) ? (_dataList.GetTotalHeight() + 30) : 30;
        Height.Set(height, 0f);
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(height, 0f);
    }

    private ValuesProviderAttribute _provider = null!;
    private MethodInfo _label = null!;

    private int _index;
    private IList _values = null!;

    private bool _expanded;
    private readonly UIList _dataList = new();

    private static readonly FieldInfo s_dummyField = typeof(DropDownElement).GetField(nameof(_dummy), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private readonly EmptyClass _dummy = new();
}
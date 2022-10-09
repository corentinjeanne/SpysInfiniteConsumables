using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace SPIC.Configs.UI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
sealed class ValuesProviderAttribute : Attribute {

    public IList Values() => (IList)_provider.Invoke(null, null);
    public readonly string ParseToString;

    public ValuesProviderAttribute(Type host, string providerName, string toSTring = "ToString"){
        MethodInfo method =  host.GetMethod(providerName, BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>());
        if(method is null) throw new ArgumentException("No public static method with this name was found");
        if(method.ReturnType != typeof(IList) && Array.IndexOf(method.ReturnType.GetInterfaces(), typeof(IList)) == -1) throw new ArgumentException("The return type of the method must be object[]");
        _provider = method;
        ParseToString = toSTring;
    }

    private readonly MethodInfo _provider;
}

public class EmptyClass {}

public class DropDownUI : ConfigElement {

    private ValuesProviderAttribute _provider;
    private MethodInfo _toString;


    private static readonly FieldInfo s_choicesField = typeof(DropDownUI).GetField(nameof(_choices), BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo s_dummyField = typeof(DropDownUI).GetField(nameof(_dummy), BindingFlags.NonPublic | BindingFlags.Instance);
    private EmptyClass _dummy = new();
    private bool _expanded;

    private int _index;
    private IList _choices;

    public override void OnBind() {

        base.OnBind();
        object value = MemberInfo.GetValue(Item);

        _provider = (ValuesProviderAttribute)Attribute.GetCustomAttribute(MemberInfo.MemberInfo, typeof(ValuesProviderAttribute));
        _toString = (value?.GetType() ?? MemberInfo.Type).GetMethod(_provider.ParseToString, BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>());
        _choices = _provider.Values();
        _index = _choices.IndexOf(value);
        TextDisplayFunction = () => (LabelAttribute?.Label ?? MemberInfo.Name) + ": " + (_index == -1 ? "None" : _toString.Invoke(_choices[_index], null));
        OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
            if(_expanded) CloseDropDownField(_index);
            else OpenDropDownField();
        };
    }

    private void CloseDropDownField(UIElement selectedElement) => CloseDropDownField(Children.IndexOf(selectedElement));

    public void CloseDropDownField(int index) {
        if(index < 0) return;
        RemoveAllChildren();
        _index = index;
        _expanded = false;
        MemberInfo.SetValue(Item, _choices[index]);
        ConfigManager.SetPendingChanges();
        Recalculate();
        // Add the selected value
    }
    public void OpenDropDownField() {
        RemoveAllChildren();
        _expanded = true;

        _choices = _provider.Values();
        int top = 30;
        for (int i = 0; i < _choices.Count; i++) {
            (UIElement container, UIElement element) = ConfigManager.WrapIt(this, ref top, new(s_dummyField), this, i);
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => "");
            container.OnClick += (UIMouseEvent evt, UIElement listeningElement) => CloseDropDownField(listeningElement);
            string name = (string)_toString.Invoke(_choices[i], null);
            element.RemoveAllChildren();
            ReflectionHelper.ObjectElement_pendindChanges.SetValue(element, false);
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => name);
        }
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        int h = !_expanded ? 30 : 30 + 34 * _choices.Count;
        Height.Set(h, 0f);
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(h, 0f);
        MaxHeight.Pixels = int.MaxValue;
    }

    // public override void Draw(SpriteBatch spriteBatch) {
    //     DrawChildren(spriteBatch);
    // }
}
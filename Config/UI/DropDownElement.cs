using System;
using System.Collections;
using System.Reflection;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace SPIC.Configs.UI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
sealed class ValuesProviderAttribute : Attribute {

    public IList Values() => (IList)_provider.Invoke(null, null)!;
    public readonly string ParseToString;

    public ValuesProviderAttribute(Type host, string providerName, string toSTring = "ToString"){
        MethodInfo? method =  host.GetMethod(providerName, BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>());
        if(method is null) throw new ArgumentException("No public static method with this name was found");
        if(method.Invoke(null, null) is not IList) throw new ArgumentException("The return type of the method must be IList");
        _provider = method;
        ParseToString = toSTring;
    }

    private readonly MethodInfo _provider;
}

public class EmptyClass {}

public class DropDownElement : ConfigElement {

#nullable disable
    private ValuesProviderAttribute _provider;
    private MethodInfo _toString;
    private IList _choices;
#nullable restore

    private static readonly FieldInfo s_dummyField = typeof(DropDownElement).GetField(nameof(_dummy), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private readonly EmptyClass _dummy = new();
    private bool _expanded;

    private int _index;

    public override void OnBind() {
        base.OnBind();
        object value = MemberInfo.GetValue(Item);

        _provider = (ValuesProviderAttribute?)Attribute.GetCustomAttribute(MemberInfo.MemberInfo, typeof(ValuesProviderAttribute));
        if(_provider is null) throw new MissingMemberException($"Drop down element requires the Atrribute {nameof(ValuesProviderAttribute)}");
        _toString = (value?.GetType() ?? MemberInfo.Type).GetMethod(_provider.ParseToString, BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>());
        if(_toString is null) throw new ArgumentException($"Reflexion failed");
        _choices = _provider.Values();
        _index = _choices.IndexOf(value);
        TextDisplayFunction = () => (LabelAttribute?.Label ?? MemberInfo.Name) + ": " + (_index == -1 ? "None" : _toString.Invoke(_choices[_index], null));
        OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
            if(_expanded) CloseDropDownField(_index);
            else OpenDropDownField();
        };
    }

    public void OpenDropDownField() {
        RemoveAllChildren();
        _expanded = true;

        _choices = _provider.Values();
        int top = 30;
        for (int i = 0; i < _choices.Count; i++) {
            (UIElement container, UIElement element) = ConfigManager.WrapIt(this, ref top, new(s_dummyField), this, i);
            int index = i;
            container.OnClick += (UIMouseEvent evt, UIElement listeningElement) => CloseDropDownField(index);
            string? name = (string?)_toString.Invoke(_choices[i], null);
            element.RemoveAllChildren();
            ReflectionHelper.ObjectElement_pendindChanges.SetValue(element, false);
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => name);
        }
        Recalculate();
    }

    public void CloseDropDownField(int index) {
        if (index < 0) return;
        RemoveAllChildren();
        _index = index;
        _expanded = false;
        MemberInfo.SetValue(Item, _choices[index]);
        ConfigManager.SetPendingChanges();
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
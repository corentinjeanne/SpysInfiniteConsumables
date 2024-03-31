using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using SpikysLib.Configs.UI;
using SpikysLib.Extensions;
using SpikysLib.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace SPIC.Configs.UI;

public sealed class DefinitionConverter : TypeConverter {
    public DefinitionConverter(Type type) {
        if (!type.IsSubclassOfGeneric(typeof(Definition<>), out Type? gen)) throw new ArgumentException($"The type {type} does not derive from the type {typeof(Definition<>)}.");
        GenericType = gen;
        MethodInfo? fromString = GenericType.GetMethod("FromString", BindingFlags.Static | BindingFlags.Public, new[] { typeof(string) });
        if (fromString is null || fromString.ReturnType != GenericType.GenericTypeArguments[0]) throw new ArgumentException($"The type {GenericType} does not have a public static FromString(string) method that returns a {GenericType.GenericTypeArguments[0]}");
        FromString = fromString;
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value) => value is string ? FromString.Invoke(null, new[] { value }) : base.ConvertFrom(context, culture, value);

    public Type GenericType { get; }
    public MethodInfo FromString { get; }
}

public interface IDefinition {
    bool AllowNull { get; }
    string DisplayName { get; }
    string? Tooltip { get; }
    IList<IDefinition> GetValues();
}

[CustomModConfigItem(typeof(DefinitionElement))]
[TypeConverter("SPIC.Configs.DefinitionConverter")]
public abstract class Definition<TDefinition> : EntityDefinition, IDefinition where TDefinition : Definition<TDefinition> {
    public Definition() : base() { }
    public Definition(string key) : base(key) { }
    public Definition(string mod, string name) : base(mod, name) { }

    [JsonIgnore] public override string DisplayName => $"{Name} [{Mod}]{(IsUnloaded ? $" ({Language.GetTextValue("Mods.ModLoader.Unloaded")})" : string.Empty)}";
    [JsonIgnore] public virtual string? Tooltip => null;

    [JsonIgnore] public virtual bool AllowNull => false;
    public abstract TDefinition[] GetValues();
    IList<IDefinition> IDefinition.GetValues() => GetValues();

    public static TDefinition FromString(string s) => (TDefinition)Activator.CreateInstance(typeof(TDefinition), s)!;
}


public sealed class DefinitionElement : ConfigElement<IDefinition> {
    public override void OnBind() {
        base.OnBind();
        IDefinition definition = Value;

        _values = definition.GetValues();

        _values = definition.GetValues();
        _index = _values.IndexOf(definition);

        Func<string> label = TextDisplayFunction;
        TextDisplayFunction = () => $"{label()}: {(_index == -1 ? Language.GetTextValue($"{Localization.Keys.UI}.None") : _values[_index].DisplayName)}";
        OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => {
            if (_expanded) CloseDropDownField(_index);
            else OpenDropDownField();
        };

        _dataList.Top = new(30, 0f);
        _dataList.Left = new(7, 0f);
        _dataList.Height = new(-7, 1f);
        _dataList.Width = new(-7 * 2, 1f);
        _dataList.ListPadding = 7f;
        MaxHeight.Pixels = int.MaxValue;

        SetupList();
    }

    public void SetupList() {
        _dataList.Clear();
        _elements.Clear();
        for (int i = 0; i < _values.Count; i++) {
            Wrapper<Text> wrapper = new(new(new StringLine(_values[i].DisplayName), _values[i].Tooltip is not null ? new StringLine(_values[i].Tooltip!) : null));
            _elements.Add(wrapper);
            int top = 0;
            int index = i;
            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, wrapper.Member, wrapper, index);
            container.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => CloseDropDownField(index);
        }
    }

    public void OpenDropDownField() {
        _expanded = true;
        Append(_dataList);
        Recalculate();
    }

    public void CloseDropDownField(int index) {
        if (!Value.AllowNull && index < 0) return;
        _expanded = false;
        if (_index != index) {
            MemberInfo.SetValue(Item, _values[index]);
            _index = index;
            ConfigManager.SetPendingChanges();
        }
        RemoveChild(_dataList);
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        float height = (_dataList.Parent != null) ? (_dataList.GetTotalHeight() + 30) : 30;
        Height.Set(height, 0f);
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(height, 0f);
    }


    private int _index;
    private IList<IDefinition> _values = null!;

    private bool _expanded;
    private readonly UIList _dataList = new();

    private readonly List<Wrapper<Text>> _elements = new();

}
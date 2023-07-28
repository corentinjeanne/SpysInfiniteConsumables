using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using System.Reflection;
using Terraria.Localization;

namespace SPIC.Configs.UI;

public interface IDictionaryEntryWrapper {
    [Newtonsoft.Json.JsonIgnore] public PropertyFieldWrapper Member { get; }
    object Key { get; set; }
    object? Value { get; set; }
}

public sealed class DictionaryEntryWrapper<Tkey, Tvalue> : IDictionaryEntryWrapper where Tkey : notnull{
    public DictionaryEntryWrapper(IDictionary dict, Tkey key) {
        _key = key;
        _dict = dict;
    }

    public Tkey Key {
        get => _key;
        set {
            object? val = _dict[_key];
            if (_dict is IOrderedDictionary ordered) {
                int index;
                for (index = 0; index < _dict.Count; index++) if (_dict[index] == _dict[_key]) break;
                ordered.Remove(_key);
                ordered.Insert(index, value, val);
            } else {
                _dict.Remove(_key);
                _dict.Add(value, val);
            }
            _key = value;
        }
    }

    [ColorNoAlpha, ColorHSLSlider]
    public Tvalue? Value { get => (Tvalue?)_dict[_key]; set => _dict[_key] = value; }

    public PropertyFieldWrapper Member => new(typeof(DictionaryEntryWrapper<Tkey, Tvalue>).GetProperty(nameof(Value), BindingFlags.Instance | BindingFlags.Public)!);

    private readonly IDictionary _dict;
    private Tkey _key;

    object IDictionaryEntryWrapper.Key { get => Key; set => Key = (Tkey)value; }
    object? IDictionaryEntryWrapper.Value { get => Value; set => Value = (Tvalue?)value; }
}

public sealed class CustomDictionaryElement : ConfigElement<IDictionary> {

    public override void OnBind() {
        base.OnBind();

        if(Value is null) throw new ArgumentNullException("This config element only supports IDictionaries");

        _dataList.Top = new(0, 0f);
        _dataList.Left = new(0, 0f);
        _dataList.Height = new(-5, 1f);
        _dataList.Width = new(0, 1f);
        _dataList.ListPadding = 5f;
        _dataList.PaddingBottom = -5f;
        SetupList();

        Append(_dataList);
    }
 
    public void SetupList(){
        _dataList.Clear();
        _dictWrappers.Clear();

        int unloaded = 0;

        IDictionary dict = Value;
        int top = 0;
        int i = -1;
        foreach ((object key, object? value) in dict.Items()) {
            i++;
            if(value is null) continue;
            if(key is EntityDefinition entity && entity.IsUnloaded){
                unloaded++;
                continue;
            }
            IDictionaryEntryWrapper wrapper = (IDictionaryEntryWrapper)Activator.CreateInstance(typeof(DictionaryEntryWrapper<,>).MakeGenericType(key.GetType(), value.GetType()), dict, key)!;
            _dictWrappers.Add(wrapper);

            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, wrapper.Member, wrapper, i);
        
            if(dict is IOrderedDictionary){
                element.Width.Pixels -= 25;
                element.Left.Pixels += 25;


                int index = i;
                HoverImageSplit moveButton = new(UpDownTexture, Language.GetTextValue($"{Localization.Keys.UI}.Up"), Language.GetTextValue($"{Localization.Keys.UI}.Down")) {
                    VAlign = 0.5f, Left = new(2, 0f),
                };
                moveButton.OnLeftClick += (UIMouseEvent a, UIElement b) => {
                    if (moveButton.HoveringUp ? index <= 0 : index >= dict.Count - 1) return;
                    ((IOrderedDictionary)Value).Move(index, index + (moveButton.HoveringUp ? -1 : 1));
                    SetupList();
                    ConfigManager.SetPendingChanges();
                };
                container.Append(moveButton);
            }

            string? name = key switch {
                IDefinition preset => preset.DisplayName,
                ItemDefinition item => $"[i:{item.Type}] {item.Name}",
                _ => key.ToString()
            };
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => name);
        }
        if(unloaded > 0){
            _dummy = new($"{unloaded} unloaded items");
            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, new(s_dummyField), this, i);
        }
        MaxHeight.Pixels = int.MaxValue;
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        int defaultHeight = _dataList.Count > 1 ? -5 : 0;
        float h = (_dataList.Parent != null) ? (_dataList.GetTotalHeight() + defaultHeight) : defaultHeight;
        Height.Set(h, 0f);
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(h, 0f);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawChildren(spriteBatch);
    }

    private static readonly FieldInfo s_dummyField = typeof(CustomDictionaryElement).GetField(nameof(_dummy), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private Text _dummy = new();

    private readonly List<IDictionaryEntryWrapper> _dictWrappers = new();
    private readonly UIList _dataList = new();
}
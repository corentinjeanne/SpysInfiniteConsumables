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

namespace SPIC.Configs.UI;



public interface IDictionaryEntryWrapper {
    [Newtonsoft.Json.JsonIgnore] public PropertyInfo ValueProp { get; }
    object Key { get; set; }
    object? Value { get; set; }
}

public class DictionaryEntryWrapper<Tkey, Tvalue> : IDictionaryEntryWrapper where Tkey : notnull{
    public PropertyInfo ValueProp => typeof(DictionaryEntryWrapper<Tkey, Tvalue>).GetProperty(nameof(Value), BindingFlags.Instance | BindingFlags.Public)!;

    public Tkey Key {
        get => _key;
        set {
            if (_dict is IOrderedDictionary ordered) {
                int index = 0;
                foreach (DictionaryEntry entry in ordered) {
                    if (entry.Key.Equals(_key)) break;
                    index++;
                }
                ordered.RemoveAt(index);
                ordered.Insert(index, value, _dict[_key]);
            } else {
                _dict.Remove(_key);
                _dict.Add(value, _dict[_key]);
            }

            _key = value;
        }
    }

    [ColorNoAlpha, ColorHSLSlider]
    public Tvalue? Value {
        get => (Tvalue?)_dict[_key];
        set {
            _dict[_key] = value;
        }
    }

    object IDictionaryEntryWrapper.Key {
        get => Key;
        set => Key = (Tkey)value;
    }
    object? IDictionaryEntryWrapper.Value {
        get => Value;
        set => Value = (Tvalue?)value;
    }

    public DictionaryEntryWrapper(IDictionary dict, Tkey key) {
        _key = key;
        _dict = dict;
    }

    private readonly IDictionary _dict;
    private Tkey _key;
}

public class CustomDictionaryElement : ConfigElement<IDictionary> {

    public override void OnBind() {

        base.OnBind();

        if(Value is null) throw new ArgumentNullException("This config element only supports IDictionaries");

        _dataList.Top = new(0, 0f);
        _dataList.Left = new(0, 0f);
        _dataList.Height = new(-5, 1f);
        _dataList.Width = new(0, 1f);
        _dataList.ListPadding = 5f;
        _dataList.PaddingBottom = -5f;
        MaxHeight.Pixels = int.MaxValue;

        Append(_dataList);

        SetupList();
    }
 
    public void SetupList(){
        _dataList.Clear();
        _dictWrappers.Clear();

        int unloaded = 0;

        IDictionary dict = Value;
        int top = 0;
        int i = -1;
        foreach (DictionaryEntry entry in dict) {
            i++;
            (object key, object? value) = entry;
            if(value is null) continue;
            if(key is ConsumableGroupDefinition entity && entity.IsUnloaded){
                unloaded++;
                continue;
            }
            Type genericType = typeof(DictionaryEntryWrapper<,>).MakeGenericType(key.GetType(), value.GetType());
            IDictionaryEntryWrapper wrapper = (IDictionaryEntryWrapper)Activator.CreateInstance(genericType, dict, key)!;

            _dictWrappers.Add(wrapper);
            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, new(wrapper.ValueProp), wrapper, i);
            
            
            if(dict is IOrderedDictionary){
                
                element.Width.Pixels -= 25;
                element.Left.Pixels += 25;

                if (element.GetType() == ReflectionHelper.ObjectElement) {
                    ReflectionHelper.ObjectElement_expanded.SetValue(element, false);
                    ReflectionHelper.ObjectElement_pendindChanges.SetValue(element, false);
                }

                int index = i;

                HoverImageSplit moveButton = new(UpDownTexture, "Move up", "Move down") {
                    VAlign = 0.5f,
                    Left = new(2, 0f)
                };
                moveButton.OnClick += (UIMouseEvent a, UIElement b) => {
                    IOrderedDictionary ordered = (IOrderedDictionary)Value;
                    if (moveButton.HoveringUp ? index <= 0 : index >= dict.Count - 1) return;
                    ordered.Move(index, index + (moveButton.HoveringUp ? -1 : 1));
                    SetupList();
                    ConfigManager.SetPendingChanges();
                };
                container.Append(moveButton);
            }

            string? name = key switch {
                ConsumableGroupDefinition group => group.Label(),
                ItemDefinition item => $"[i:{item.Type}] {item.Name}",
                EntityDefinition def => def.Name,
                _ => key.ToString()
            };
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => name);
        }
        if(unloaded > 0){
            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, new(s_dummyField), this, i);
            string text = $"{unloaded} unloaded consumable types";
            element.RemoveAllChildren();
            ReflectionHelper.ObjectElement_pendindChanges.SetValue(element, false);
            ReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => text);

        }
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
    private readonly EmptyClass _dummy = new();

    private readonly List<IDictionaryEntryWrapper> _dictWrappers = new();
    private readonly UIList _dataList = new();
}
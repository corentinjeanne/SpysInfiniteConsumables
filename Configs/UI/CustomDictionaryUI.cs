using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using System.Reflection;
using System.ComponentModel;

namespace SPIC.Configs.UI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class ConstantKeys : Attribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class ValuesAsConfigItemsAttribute : Attribute { }


public class CustomDictionaryUI : ConfigElement<IDictionary> {

    // TODO implement attributes
    // private ConstantKeys _constKeys; // - add, clear, remove
    // private ValuesAsConfigItemsAttribute _asConfigItems; // - collapse, size button, (add, clear, remove)

    // private FieldInfo _dictWrappersField = typeof(CustomDictionaryUI).GetField(nameof(dictWrappers), BindingFlags.Instance | BindingFlags.Public);

    // private Attribute[] _memberAttributes;

    public List<IDictionaryEntryWrapper> dictWrappers = new();
    private UIList _dataList;

    public override void OnBind() {
        object value = Value;
        if(value is null) throw new ArgumentNullException("This config element only supports IDictionaries");
        
        // _memberAttributes = Attribute.GetCustomAttributes(MemberInfo.MemberInfo).Where(attrib => attrib is not CustomModConfigItemAttribute).ToArray();
        
        // _asConfigItems = ConfigManager.GetCustomAttribute<ValuesAsConfigItemsAttribute>(MemberInfo, value.GetType());
        // _constKeys = ConfigManager.GetCustomAttribute<ConstantKeys>(MemberInfo, value.GetType());
        // _sortable = ConfigManager.GetCustomAttribute<SortableAttribute>(MemberInfo, value.GetType());

        _dataList = new UIList() {
            Top = new(0, 0f),
            Left = new(0, 0f),
            Height = new(-5, 1f),
            Width = new(0, 1f),
            ListPadding = 5f,
            PaddingBottom = -5f
        };

        Append(_dataList);

        SetupList();
    }
 
    // TODO Add attributes to childrens
    public void SetupList(){
        _dataList.Clear();
        dictWrappers.Clear();

        IDictionary dict = Value;
        int top = 0;
        int i = 0;
        foreach (DictionaryEntry entry in dict) {
            (object key, object value) = entry;
            Type genericType = typeof(DictionaryEntryWrapper<,>).MakeGenericType(key.GetType(), value.GetType());
            IDictionaryEntryWrapper wrapper = (IDictionaryEntryWrapper)Activator.CreateInstance(genericType, dict, key, value);

            dictWrappers.Add(wrapper);
            (UIElement container, UIElement element) = ConfigManager.WrapIt(_dataList, ref top, new(wrapper.ValueProp), wrapper, i);
                // ConfigManager.WrapIt(_dataList, ref top, new(_dictWrappersField), this, 0, _dictWrappers, genericType, i);
                
            if(dict is IOrderedDictionary){
                
                element.Width.Pixels -= 25;
                element.Left.Pixels += 25;

                if (element.GetType() == ConfigReflectionHelper.ObjectElement) ConfigReflectionHelper.ObjectElement_expanded.SetValue(element, false);

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
            string name = key switch {
                // InfinityDefinition inf => $"[i:{inf.Infinity.IconType}] {inf.Infinity.LocalizedName}",
                ConsumableTypeDefinition type => $"[i:{type.ConsumableType.IconType}] {type.ConsumableType.Name}",
                ItemDefinition item => $"[i:{item.Type}] {item.Name}",
                EntityDefinition def => def.Name,
                _ => key.ToString()
            };
            ConfigReflectionHelper.ConfigElement_TextDisplayFunction.SetValue(element, () => name);
            
            i++;
        }
        Recalculate();
    }

    public override void Recalculate() {
        base.Recalculate();
        int defaultHeight = -5;
        float h = (_dataList.Parent != null) ? (_dataList.GetTotalHeight() + defaultHeight) : defaultHeight;
        Height.Set(h, 0f);
        if (Parent != null && Parent is UISortableElement) Parent.Height.Set(h, 0f);
        MaxHeight.Pixels = int.MaxValue;
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawChildren(spriteBatch);
    }
}
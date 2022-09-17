using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Microsoft.Xna.Framework;

namespace SPIC.Configs.UI;

public class SingleFieldItemConverter : JsonConverter<SingleFieldItem> {

    public override SingleFieldItem ReadJson(JsonReader reader, Type objectType, SingleFieldItem existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if(!hasExistingValue) existingValue = (SingleFieldItem)Activator.CreateInstance(objectType);
        JProperty member = (JProperty)JObject.Load(reader).First;
        object value = ((JValue)member.Value).Value;
        existingValue.Select(member.Name);
        if(value is not null && value.GetType() != existingValue.SelectedMember.Type) value = Convert.ChangeType(value, existingValue.SelectedMember.Type);
        existingValue.SelectedValue = value;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, SingleFieldItem value, JsonSerializer serializer) {
        writer.WriteStartObject();
        writer.WritePropertyName(value.SelectedMember.Name);
        writer.WriteValue(value.SelectedValue);
        writer.WriteEndObject();
    }
}

[CustomModConfigItem(typeof(SingleFieldItemElement))]
[JsonConverter(typeof(SingleFieldItemConverter))]
public abstract class SingleFieldItem {

    public SingleFieldItem() {
        _members = ConfigManager.GetFieldsAndProperties(this).Where(member => member.MemberInfo.DeclaringType != typeof(SingleFieldItem)).ToArray();
        SelectedIndex = 0;
    }
    public int SelectedIndex {
        get => _selectedIndex;
        set => _selectedIndex = Loop(value);
    }
    public void Select(string name) => SelectedIndex = Array.FindIndex(_members, prop => prop.Name == name);

    public object SelectedValue {
        get => _members[SelectedIndex].GetValue(this);
        set => _members[SelectedIndex].SetValue(this, value);
    }
    public PropertyFieldWrapper SelectedMember => Member(SelectedIndex);
    public PropertyFieldWrapper Member(int index) => _members[Loop(index)];


    private int _selectedIndex;
    private readonly PropertyFieldWrapper[] _members;
    private int Loop(int index) => (index + _members.Length) % _members.Length;
}



public class SingleFieldItemElement : ConfigElement<SingleFieldItem> {


    private SingleFieldItem _single;
    private HoverImageSplit _changeButton;
    private Func<string> _parentDisplayFunction;
    private Func<string> _childTextDisplayFunction;
    private UIElement _selectedElement;

    public override void OnBind() {
        base.OnBind();

        _single = Value;

        _parentDisplayFunction = TextDisplayFunction;
        TextDisplayFunction = () => $"{_parentDisplayFunction()} ({_childTextDisplayFunction()})";

        _changeButton = new(UpDownTexture, null, null) {
            VAlign = 0.5f,
            Left = StyleDimension.FromPixelsAndPercent(-26, 1f),
        };
        _changeButton.OnClick += (UIMouseEvent a, UIElement b) => {
            if (_changeButton.HoveringUp) Value.SelectedIndex++;
            else Value.SelectedIndex--;
            SetupMember();
            ConfigManager.SetPendingChanges();
        };

        SetupMember();
    }

    public void SetupMember() {
        RemoveAllChildren();
        
        int top = 0;
        (_selectedElement, UIElement element) = ConfigManager.WrapIt(this, ref top, _single.SelectedMember, _single, 0);
        _selectedElement.Left.Pixels -= 20;
        Append(_changeButton);

        if(!_single.SelectedMember.CanWrite && _single.SelectedValue is null){
            ConfigReflectionHelper.ObjectElement_pendindChanges.SetValue(element, false);
            element.RemoveAllChildren();
        }

        _childTextDisplayFunction = (Func<string>)ConfigReflectionHelper.ConfigElement_TextDisplayFunction.GetValue(element);
        ConfigReflectionHelper.ConfigElement_DrawLabel.SetValue(element, false);
        ConfigReflectionHelper.ConfigElement_backgroundColor.SetValue(element, new Color(0, 0, 0, 0));

        _changeButton.HoverTextUp = $"Change to {_single.Member(_single.SelectedIndex+1).Name}";
        _changeButton.HoverTextDown = $"Change to {_single.Member(_single.SelectedIndex-1).Name}";

        Recalculate();
    }
    public override void Recalculate() {
        base.Recalculate();

        Height.Set(_selectedElement.Height.Pixels, 0f);
    }
}
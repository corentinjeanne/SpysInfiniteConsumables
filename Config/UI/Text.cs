using Newtonsoft.Json;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs.UI;

[CustomModConfigItem(typeof(TextElement))]
public class Text {
    public Text() {}
    public Text(string? label = null, string? tooltip = null) {
        Label = label;
        Tooltip = tooltip;
    }

    [JsonIgnore] public string? Label { get; }
    [JsonIgnore] public string? Tooltip { get; }
}

public class TextElement : ConfigElement<Text> {

    public override void OnBind() {
        base.OnBind();
        Text value = Value ??= new();
        if(value.Label is not null) TextDisplayFunction = () => Value.Label;
        if(value.Tooltip is not null) TooltipFunction = () => Value.Tooltip;
        Height.Set(30, 0);
    }
}
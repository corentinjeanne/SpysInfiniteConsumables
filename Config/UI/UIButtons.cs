using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace SPIC.Configs.UI;

public class HoverImage : UIImage {

    public HoverImage(Asset<Texture2D> texture, string hoverText) : base(texture) {
        HoverText = hoverText;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);
        if (IsMouseHovering) ReflectionHelper.UIModConfig_Tooltip.SetValue(null, HoverText);
    }
    public string HoverText {get; set;}
}

public class HoverImageSplit : UIImage {

    public bool HoveringUp => Main.mouseY < GetDimensions().Y + GetDimensions().Height / 2;

    public HoverImageSplit(Asset<Texture2D> texture, string hoverTextUp, string hoverTextDown) : base(texture) {
        HoverTextUp = hoverTextUp;
        HoverTextDown = hoverTextDown;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);
        if (IsMouseHovering) ReflectionHelper.UIModConfig_Tooltip.SetValue(null, HoveringUp ? HoverTextUp : HoverTextDown);
    }

    public string HoverTextUp {get; set;}
    public string HoverTextDown {get; set;}
}

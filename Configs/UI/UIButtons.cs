using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace SPIC.Configs.UI;

public class HoverImageSplit : UIImage {

    public bool HoveringUp { get; protected set; }

    public HoverImageSplit(Asset<Texture2D> texture, string hoverTextUp, string hoverTextDown) : base(texture) {
        HoverTextUp = hoverTextUp;
        HoverTextDown = hoverTextDown;
    }
    public override void Update(GameTime gameTime) {
        Rectangle r = GetDimensions().ToRectangle();
        if(IsMouseHovering) HoveringUp = Main.mouseY < r.Y + r.Height / 2;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);
        if (IsMouseHovering) {
            ReflectionHelper.UIModConfig_Tooltip.SetValue(null, HoveringUp ? HoverTextUp : HoverTextDown);
        }
    }
    public string HoverTextUp {get; set;}
    public string HoverTextDown {get; set;}
}

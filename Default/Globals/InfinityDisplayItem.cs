using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Displays;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SPIC.Default.Globals;

public sealed class InfinityDisplayItem : GlobalItem {

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (Tooltip.Instance.Enabled) Tooltip.ModifyTooltips(item, tooltips);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Main.gameMenu && Glow.Instance.Enabled) Glow.PreDrawInInventory(item, spriteBatch, position, frame, origin, scale);
        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Main.gameMenu && Dots.Instance.Enabled) Dots.PostDrawInInventory(item, spriteBatch, position);
    }
}
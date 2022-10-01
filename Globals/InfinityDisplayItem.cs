using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using SPIC.ConsumableTypes;

namespace SPIC.Globals;

public class InfinityDisplayItem : GlobalItem {

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!Main.PlayerLoaded) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.toopltip_ShowTooltip) return;

        foreach (IConsumableType type in InfinityManager.ConsumableTypes(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled)) {
            type.ModifyTooltip(item, tooltips);

            if (type is IAmmunition ammoType && ammoType.ConsumesAmmo(item) && ammoType.HasAmmo(Main.LocalPlayer, item, out Item ammo))
                ammoType.ModifyTooltip(item, ammo, tooltips);
            
        }
    }


    private static int s_frame;
    private static int s_focusType;
    public static void IncrementCounters(){
        s_frame++;
        if(s_frame < Configs.InfinityDisplay.Instance.dot_PulseTime) return;
        s_frame = 0;
        s_focusType++;
        if(s_focusType < InfinityManager.LCM) return;
        s_focusType = 0;
    }

    internal record struct DisplayInfo(Color Color, DisplayFlags DisplayFlags, long Infinity, long MaxInfinity);
    internal static List<DisplayInfo> s_wouldDisplayDot;
    internal static List<DisplayInfo> s_wouldDisplayGlow;

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if(!Main.PlayerLoaded) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.dots_ShowDots && !display.glow_ShowGlow) return;

        s_wouldDisplayDot = new();
        s_wouldDisplayGlow = new();


        foreach(IConsumableType type in InfinityManager.ConsumableTypes(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled)){
            if (display.dots_ShowDots) type.DrawInInventorySlot(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            if (display.glow_ShowGlow) type.DrawOnItemSprite(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

            // TODO consumable and weapon (somehow) => only 1 dot
            if (type is IAmmunition ammoType && ammoType.ConsumesAmmo(item) && ammoType.HasAmmo(Main.LocalPlayer, item, out Item ammo)) {
                if (display.dots_ShowDots) ammoType.DrawInInventorySlot(item, ammo, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
                if (display.glow_ShowGlow) ammoType.DrawOnItemSprite(item, ammo, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            }
        }

        if (display.dots_ShowDots && s_wouldDisplayDot.Count > 0) {
            Vector2 delta = Configs.InfinityDisplay.Instance.dots_Start switch {
                Configs.InfinityDisplay.Corner.TopLeft =>     new( 1, 1),
                Configs.InfinityDisplay.Corner.TopRight =>    new(-1, 1),
                Configs.InfinityDisplay.Corner.BottomLeft =>  new( 1,-1),
                Configs.InfinityDisplay.Corner.BottomRight => new(-1,-1),
                _ =>                                          new( 0, 0)
            };
            Vector2 dotPosition = position + frame.Size() / 2f * scale - DefaultImplementation.SmallDot.Size() / 2 * Main.inventoryScale - (Terraria.GameContent.TextureAssets.InventoryBack.Value.Size() * Main.inventoryScale / 2 - DefaultImplementation.SmallDot.Size() * 2 / 3) * delta;
            Vector2 dotDelta = DefaultImplementation.SmallDot.Size() * (Configs.InfinityDisplay.Instance.dots_vertical ? new Vector2(0, delta.Y) : new Vector2(delta.X, 0)) * Main.inventoryScale;
            
            int startingDot = s_focusType % ((s_wouldDisplayDot.Count+display.dots_PerPage-1)/display.dots_PerPage * display.dots_PerPage) / display.dots_PerPage * display.dots_PerPage;
            for (int i = startingDot; i < startingDot + display.dots_PerPage && i < s_wouldDisplayDot.Count; i++) {
                DefaultImplementation.DisplayDot(spriteBatch, dotPosition, s_wouldDisplayDot[i].Color, s_wouldDisplayDot[i].DisplayFlags, s_wouldDisplayDot[i].Infinity, s_wouldDisplayDot[i].MaxInfinity);
                dotPosition += dotDelta;
            }
        }

        if (display.glow_ShowGlow && s_wouldDisplayGlow.Count > 0) {
            int i = s_focusType % s_wouldDisplayGlow.Count;

            if (i < s_wouldDisplayGlow.Count) {
                DefaultImplementation.DisplayGlow(spriteBatch, item, position, origin, frame, scale, s_wouldDisplayGlow[i].Color, (float)s_frame / display.glow_PulseTime, s_wouldDisplayGlow[i].DisplayFlags, s_wouldDisplayGlow[i].Infinity, s_wouldDisplayGlow[i].MaxInfinity);
            }
        }
    }
}

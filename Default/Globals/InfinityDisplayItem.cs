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

    // public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
    //     InfinityDisplays config = InfinityDisplays.Instance;

    //     if (Main.gameMenu || !Dots.Instance.Enabled || !(config.ShowInfinities || config.ShowRequirement)) return;

    //     Vector2 cornerDirection = InfinityDisplays.Get(Dots.Instance).Start switch {
    //         Corner.TopLeft => new(-1, -1),
    //         Corner.TopRight => new(1, -1),
    //         Corner.BottomLeft => new(-1, 1),
    //         Corner.BottomRight => new(1, 1),
    //         _ => new(0, 0)
    //     };

    //     Vector2 slotCenter = position;
    //     Vector2 dotPosition = slotCenter + (TextureAssets.InventoryBack.Value.Size() / 2f * Main.inventoryScale - Borders) * cornerDirection - DotSize / 2f * Main.inventoryScale;
    //     Vector2 dotDelta = DotSize * (InfinityDisplays.Get(Dots.Instance).Direction == Direction.Vertical ? new Vector2(0, -cornerDirection.Y) : new Vector2(-cornerDirection.X, 0)) * Main.inventoryScale;


    //     ItemDisplay itemDisplay = item.GetLocalItemDisplay();
    //     if (itemDisplay.DisplayedInfinities.Length == 0) return;

    //     foreach ((IInfinity infinity, _, FullInfinity display) in itemDisplay.InfinitiesByGroups(s_groupIndex % itemDisplay.Groups)) {
    //         if (display.Count == 0) continue;
    //         DisplayDot(spriteBatch, dotPosition, infinity, display);
    //         dotPosition += dotDelta;
    //     }

    // }

    // public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, IInfinity infinity, FullInfinity display) {
    //     InfinityDisplays config = InfinityDisplays.Instance;
    //     float scale = DotScale * Main.inventoryScale;

    //     float maxAlpha = Main.mouseTextColor / 255f;
    //     float ratio;
    //     if (config.ShowInfinities) {
    //         if (display.Infinity > 0) {
    //             Color borderColor = Color.Black * maxAlpha;
    //             for (int i = 0; i < s_outerPixels.Length; i++) {
    //                 spriteBatch.Draw(
    //                     TextureAssets.MagicPixel.Value,
    //                     position + s_outerPixels[i] * scale,
    //                     new Rectangle(0, 0, 1, 1),
    //                     borderColor,
    //                     0f,
    //                     Vector2.Zero,
    //                     scale,
    //                     SpriteEffects.None,
    //                     0
    //                 );
    //             }
    //             ratio = 1f;
    //         } else if (!config.ShowRequirement) return;
    //         else {
    //             maxAlpha *= 0.9f;
    //             ratio = (float)display.Count / display.Requirement.Count;
    //         }
    //     } else ratio = 0;

    //     Color color = InfinityDisplays.GetColor(infinity) * maxAlpha;
    //     for (int i = 0; i < s_innerPixels.Length; i++) {
    //         float alpha = ratio >= (i + 1f) / s_innerPixels.Length ? 1f : 0.5f;
    //         spriteBatch.Draw(
    //             TextureAssets.MagicPixel.Value,
    //             position + s_innerPixels[i] * scale,
    //             new Rectangle(0, 0, 1, 1),
    //             color * alpha,
    //             0f,
    //             Vector2.Zero,
    //             scale,
    //             SpriteEffects.None,
    //             0
    //         );
    //     }
    // }


    // public static void IncrementCounters() {
    //     if (Main.GlobalTimeWrappedHourly >= s_groupTimer) {
    //         s_groupTimer = (s_groupTimer + InfinityDisplays.Get(Dots.Instance).GroupTime) % 3600;
    //         s_groupIndex = (s_groupIndex + 1) % InfinityManager.GroupsLCM;
    //     }
    //     if (Main.GlobalTimeWrappedHourly >= s_infinityTimer) {
    //         s_infinityTimer = (int)(Main.GlobalTimeWrappedHourly / InfinityDisplays.Get(Glow.Instance).AnimationLength + 1) * InfinityDisplays.Get(Glow.Instance).AnimationLength % 3600;
    //         s_InfinityIndex = (s_InfinityIndex + 1) % InfinityManager.InfinitiesLCM;
    //     }
    // }

    // public const int MaxDots = 8;
    // public const int DotScale = 2;
    // public static readonly Vector2 DotSize = new Vector2(4, 4) * DotScale;
    // public static readonly Vector2 Borders = DotSize * 2f / 3f;

    // private static int s_groupIndex = 0, s_InfinityIndex = 0;
    // private static float s_groupTimer = 0, s_infinityTimer = 0;

    // private static readonly Vector2[] s_innerPixels = new Vector2[] { new(1, 1), new(2, 1), new(1, 2), new(2, 2) };
    // private static readonly Vector2[] s_outerPixels = new Vector2[] { new(1, 0), new(0, 1), new(0, 2), new(1, 3), new(2, 3), new(3, 2), new(3, 1), new(2, 0) };
}
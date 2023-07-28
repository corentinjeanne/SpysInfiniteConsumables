using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using System;

namespace SPIC.Globals;

// TODO test lag magic storage 1k+ items

public sealed class InfinityDisplayItem : GlobalItem {

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.toopltip_ShowTooltip || !(display.general_ShowInfinities || display.general_ShowRequirement || display.general_ShowCategories)) return;

        ItemDisplay itemDisplay = item.GetLocalItemDisplay();
        foreach (IInfinity infinity in itemDisplay.Infinities) {
            (TooltipLine lineToFind, TooltipLineID? position) = infinity.GetTooltipLine(item);
            bool added = false;
            TooltipLine? line = display.toopltip_AddMissingLines ? tooltips.FindorAddLine(lineToFind, out added, position) : tooltips.FindLine(lineToFind.Name);
            
            if (line is null) continue;
            (FullInfinity fullInfinity, long consumed) = itemDisplay[infinity];
            DisplayOnLine(line, item, infinity, fullInfinity, consumed);
            if (added) line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
        }
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;

        if (Main.gameMenu || !display.glow_ShowGlow || !display.general_ShowInfinities) return true;


        ItemDisplay itemDisplay = item.GetLocalItemDisplay();

        List<IInfinity> withDisplay = new();
        foreach(IInfinity g in itemDisplay.Infinities) {
            (FullInfinity fullInfinity, long consumed) = itemDisplay[g];
            if (consumed != 0 && fullInfinity.Infinity >= consumed) withDisplay.Add(g);
        }

        if (withDisplay.Count == 0) return true;
        IInfinity infinity = withDisplay[s_InfinityIndex % withDisplay.Count];
        DisplayGlow(spriteBatch, item, position, frame, origin, scale, infinity);

        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;

        if (Main.gameMenu || !display.dots_ShowDots || !(display.general_ShowInfinities || display.general_ShowRequirement)) return;


        Vector2 cornerDirection = display.dots_Start switch {
            Configs.InfinityDisplay.Corner.TopLeft => new(-1, -1),
            Configs.InfinityDisplay.Corner.TopRight => new(1, -1),
            Configs.InfinityDisplay.Corner.BottomLeft => new(-1, 1),
            Configs.InfinityDisplay.Corner.BottomRight => new(1, 1),
            _ => new(0, 0)
        };

        Vector2 slotCenter = position;
        Vector2 dotPosition = slotCenter + (TextureAssets.InventoryBack.Value.Size() / 2f * Main.inventoryScale - Borders) * cornerDirection - DotSize / 2f * Main.inventoryScale;
        Vector2 dotDelta = DotSize * (display.dots_Direction == Configs.InfinityDisplay.Direction.Vertical ? new Vector2(0, -cornerDirection.Y) : new Vector2(-cornerDirection.X, 0)) * Main.inventoryScale;


        ItemDisplay itemDisplay = item.GetLocalItemDisplay();
        if(itemDisplay.Infinities.Count == 0) return;
        
        foreach (IInfinity infinity in itemDisplay.InfinitiesByGroup[s_groupIndex % itemDisplay.InfinitiesByGroup.Count]) {
            (FullInfinity fullInfinity, long consumed) = itemDisplay[infinity];
            if(consumed == 0) continue;
            DisplayDot(spriteBatch, dotPosition, infinity, fullInfinity, consumed);
            dotPosition += dotDelta;
        }

    }

    public static void DisplayOnLine(TooltipLine line, Item item, IInfinity infinity, FullInfinity fullInfinity, long consumed) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        IGroup group = infinity.Group;

        bool canDisplayInfinity = consumed != 0 && display.general_ShowInfinities;
        long count = canDisplayInfinity ? fullInfinity.Count : 0;
        string extra = display.general_ShowCategories ? string.Join(", ", fullInfinity.Extras) : string.Empty;
        
        void AddExtra() {
            if (extra.Length != 0) line.Text += $" ({extra})";
        }


        if (canDisplayInfinity && fullInfinity.Infinity >= consumed) {
            line.OverrideColor = infinity.Color;
            line.Text = fullInfinity.Requirement.Multiplier >= 1 || consumed == -1 ?
                Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Infinite", line.Text) :
                Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.PartialyInfinite", line.Text, group.CountToString(item, 0, fullInfinity.Infinity, display.tooltip_RequirementStyle));
            AddExtra();
        }
        else if (display.general_ShowRequirement) {
            long requirement = fullInfinity.Requirement.CountForInfinity(consumed);
            line.Text += extra.Length == 0 ? $" ({group.CountToString(item, count, requirement, display.tooltip_RequirementStyle)})" : $" ({group.CountToString(item, count, requirement, display.tooltip_RequirementStyle)}, {extra})";
        }
        else AddExtra();

    }

    public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, IInfinity infinity, FullInfinity fullInfinity, long consumed) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        float scale = DotScale * Main.inventoryScale;
        
        float maxAlpha = Main.mouseTextColor / 255f;
        float ratio;
        if(display.general_ShowInfinities && fullInfinity.Infinity >= consumed){
            for (int i = 0; i < s_outerPixels.Length; i++) {
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    position+s_outerPixels[i]*scale,
                    new Rectangle(0,0,1,1),
                    Color.Black * maxAlpha,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
            ratio = 1f;
        } else {
            maxAlpha *= 0.9f;
            if (display.general_ShowInfinities && display.general_ShowRequirement) {
                long requirement = fullInfinity.Requirement.CountForInfinity(consumed);
                ratio = (float)fullInfinity.Count / requirement;
            } else ratio = 0;
        }
        
        Color color = infinity.Color * maxAlpha;
        for (int i = 0; i < s_innerPixels.Length; i++) {
            float alpha = ratio >= (i + 1f) / s_innerPixels.Length ? 1f : 0.5f;
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                position + s_innerPixels[i] * scale,
                new Rectangle(0,0,1,1),
                color * alpha,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0
            );
        }
    }
    
    public static void DisplayGlow(SpriteBatch spriteBatch, Item item, Vector2 position, Rectangle frame, Vector2 origin, float scale, IInfinity infinity) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        Texture2D texture = TextureAssets.Item[item.type].Value;

        float angle = Main.GlobalTimeWrappedHourly % display.glow_InfinityTime/display.glow_InfinityTime; // 0->1
        float alpha = angle <= 0.5f ? angle : (1 - angle); // 0->0.5->0
        alpha *= 2; // 0->1->0

        angle += item.type % 16 / 16;
        Color color = infinity.Color;

        for (float f = 0f; f < 1f; f += 1 / 3f) spriteBatch.Draw(texture, position + new Vector2(0f, 1.5f + 1.5f * alpha).RotatedBy((f*2 + angle) * Math.PI), new Rectangle?(frame), color * (alpha * 0.75f), 0, origin, scale, 0, 0f);
        for (float f = 0f; f < 1f; f += 1 / 4f) spriteBatch.Draw(texture, position + new Vector2(0f, 4f * alpha).RotatedBy((f + angle) * -2 * Math.PI), new Rectangle?(frame), color * (alpha * 0.5f), 0, origin, scale, 0, 0f);
    }


    public static void IncrementCounters() {
        InfinityManager.CacheTimer();
        if(Main.GlobalTimeWrappedHourly >= s_groupTimer){
            s_groupTimer = (s_groupTimer + Configs.InfinityDisplay.Instance.dot_PageTime) % 3600;
            s_groupIndex = (s_groupIndex + 1) % InfinityManager.GroupsLCM;
        }
        if(Main.GlobalTimeWrappedHourly >= s_infinityTimer){
            s_infinityTimer = (s_infinityTimer + Configs.InfinityDisplay.Instance.glow_InfinityTime) % 3600;
            s_InfinityIndex = (s_InfinityIndex + 1) % InfinityManager.InfinitiesLCM;
        }
    }
    private static int s_groupIndex = 0, s_InfinityIndex = 0;
    private static float s_groupTimer = 0, s_infinityTimer = 0;

    public const int MaxDots = 8;
    public const int DotScale = 2;
    public static readonly Vector2 DotSize = new Vector2(4, 4) * DotScale;
    public static readonly Vector2 Borders = DotSize * 2f / 3f;

    private static readonly Vector2[] s_innerPixels = new Vector2[]{ new(1,1),new(2,1),new(1,2),new(2,2) };
    private static readonly Vector2[] s_outerPixels = new Vector2[]{ new(1,0),new(0,1),new(0,2),new(1,3),new(2,3),new(3,2),new(3,1),new(2,0) };
}

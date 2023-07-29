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
        foreach (IInfinity infinity in itemDisplay.DisplayedInfinities) {
            (TooltipLine lineToFind, TooltipLineID? position) = infinity.GetTooltipLine(item);
            bool added = false;
            TooltipLine? line = display.toopltip_AddMissingLines ? tooltips.FindorAddLine(lineToFind, out added, position) : tooltips.FindLine(lineToFind.Name);
            
            if (line is null) continue;
            DisplayOnLine(line, item, infinity, itemDisplay[infinity]);
            if (added) line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
        }
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Configs.InfinityDisplay config = Configs.InfinityDisplay.Instance;

        if (Main.gameMenu || !config.glow_ShowGlow || !config.general_ShowInfinities) return true;

        ItemDisplay itemDisplay = item.GetLocalItemDisplay();

        List<IInfinity> withDisplay = new();
        foreach(IInfinity g in itemDisplay.DisplayedInfinities) {
            if (itemDisplay[g].Infinity > 0) withDisplay.Add(g);
        }

        if (withDisplay.Count == 0) return true;
        IInfinity infinity = withDisplay[s_InfinityIndex % withDisplay.Count];
        DisplayGlow(spriteBatch, item, position, frame, origin, scale, infinity);

        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Configs.InfinityDisplay config = Configs.InfinityDisplay.Instance;

        if (Main.gameMenu || !config.dots_ShowDots || !(config.general_ShowInfinities || config.general_ShowRequirement)) return;

        Vector2 cornerDirection = config.dots_Start switch {
            Configs.InfinityDisplay.Corner.TopLeft => new(-1, -1),
            Configs.InfinityDisplay.Corner.TopRight => new(1, -1),
            Configs.InfinityDisplay.Corner.BottomLeft => new(-1, 1),
            Configs.InfinityDisplay.Corner.BottomRight => new(1, 1),
            _ => new(0, 0)
        };

        Vector2 slotCenter = position;
        Vector2 dotPosition = slotCenter + (TextureAssets.InventoryBack.Value.Size() / 2f * Main.inventoryScale - Borders) * cornerDirection - DotSize / 2f * Main.inventoryScale;
        Vector2 dotDelta = DotSize * (config.dots_Direction == Configs.InfinityDisplay.Direction.Vertical ? new Vector2(0, -cornerDirection.Y) : new Vector2(-cornerDirection.X, 0)) * Main.inventoryScale;


        ItemDisplay itemDisplay = item.GetLocalItemDisplay();
        if(itemDisplay.DisplayedInfinities.Count == 0) return;
        
        foreach (IInfinity infinity in itemDisplay.InfinitiesByGroup[s_groupIndex % itemDisplay.InfinitiesByGroup.Count]) {
            FullInfinity display = itemDisplay[infinity];
            if(display.Count == 0) continue;
            DisplayDot(spriteBatch, dotPosition, infinity, display);
            dotPosition += dotDelta;
        }

    }

    public static void DisplayOnLine(TooltipLine line, Item item, IInfinity infinity, FullInfinity display) {
        Configs.InfinityDisplay config = Configs.InfinityDisplay.Instance;
        IGroup group = infinity.Group;

        bool canDisplayInfinity = config.general_ShowInfinities;
        long count = canDisplayInfinity ? display.Count : 0;
        string extra = config.general_ShowCategories ? string.Join(", ", display.Extras) : string.Empty;
        
        void AddExtra() {
            if (extra.Length != 0) line.Text += $" ({extra})";
        }


        if (canDisplayInfinity && display.Infinity > 0) {
            line.OverrideColor = infinity.Color;
            line.Text = display.Requirement.Multiplier >= 1 ?
                Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Infinite", line.Text) :
                Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.PartialyInfinite", line.Text, group.CountToString(item, 0, display.Infinity, config.tooltip_RequirementStyle));
            AddExtra();
        }
        else if (config.general_ShowRequirement) {
            line.Text += extra.Length == 0 ? $" ({group.CountToString(item, count, display.Requirement.Count, config.tooltip_RequirementStyle)})" : $" ({group.CountToString(item, count, display.Requirement.Count, config.tooltip_RequirementStyle)}, {extra})";
        }
        else AddExtra();

    }

    public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, IInfinity infinity, FullInfinity display) {
        Configs.InfinityDisplay config = Configs.InfinityDisplay.Instance;
        float scale = DotScale * Main.inventoryScale;
        
        float maxAlpha = Main.mouseTextColor / 255f;
        float ratio;
        if(config.general_ShowInfinities && display.Infinity > 0){
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
            if (config.general_ShowInfinities && config.general_ShowRequirement) {
                ratio = (float)display.Count / display.Requirement.Count;
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

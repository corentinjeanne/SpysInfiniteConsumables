using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using SPIC.ConsumableGroup;

namespace SPIC.Globals;

public enum DisplayFlags {
    Category = 0b0001,
    Requirement = 0b0010,
    Infinity = 0b0100,
    All = Category | Requirement | Infinity
}

public class InfinityDisplayItem : GlobalItem {

    private static IEnumerable<IConsumableGroup> DisplayableTypes(Item item) {
        foreach (IConsumableGroup type in InfinityManager.ConsumableGroups(FilterFlags.NonGlobal | FilterFlags.Enabled)) {
            if (InfinityManager.IsUsed(item, type.UID)) yield return type;
        }
        foreach (IConsumableGroup type in InfinityManager.ConsumableGroups(FilterFlags.Global | FilterFlags.Enabled)) {
            if(item.GetRequirement(type.UID) is not NoRequirement) yield return type;
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!Main.PlayerLoaded) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.toopltip_ShowTooltip) return;

        foreach (IConsumableGroup type in DisplayableTypes(item)) {
            type.ModifyTooltip(item, tooltips);
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

    internal record struct DisplayInfo(Color Color, DisplayFlags DisplayFlags, Infinity Infinity, ICount Next);
    internal static List<DisplayInfo> s_wouldDisplayDot = new();
    internal static List<DisplayInfo> s_wouldDisplayGlow = new();

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if(!Main.PlayerLoaded) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.dots_ShowDots && !display.glow_ShowGlow) return;

        s_wouldDisplayDot.Clear();
        s_wouldDisplayGlow.Clear();


        foreach(IConsumableGroup type in DisplayableTypes(item)){
            if (display.dots_ShowDots) type.DrawInInventorySlot(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            if (display.glow_ShowGlow) type.DrawOnItemSprite(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        if (display.dots_ShowDots && s_wouldDisplayDot.Count > 0) {
            Vector2 delta = Configs.InfinityDisplay.Instance.dots_Start switch {
                Configs.InfinityDisplay.Corner.TopLeft =>     new( 1, 1),
                Configs.InfinityDisplay.Corner.TopRight =>    new(-1, 1),
                Configs.InfinityDisplay.Corner.BottomLeft =>  new( 1,-1),
                Configs.InfinityDisplay.Corner.BottomRight => new(-1,-1),
                _ =>                                          new( 0, 0)
            };
            Vector2 dotPosition = position + frame.Size() / 2f * scale - SmallDot.Size() / 2 * Main.inventoryScale - (TextureAssets.InventoryBack.Value.Size() * Main.inventoryScale / 2 - SmallDot.Size() * 2 / 3) * delta;
            Vector2 dotDelta = SmallDot.Size() * (Configs.InfinityDisplay.Instance.dots_vertical ? new Vector2(0, delta.Y) : new Vector2(delta.X, 0)) * Main.inventoryScale;
            
            int startingDot = s_focusType % ((s_wouldDisplayDot.Count+display.dots_PerPage-1)/display.dots_PerPage * display.dots_PerPage) / display.dots_PerPage * display.dots_PerPage;
            for (int i = startingDot; i < startingDot + display.dots_PerPage && i < s_wouldDisplayDot.Count; i++) {
                DisplayDot(spriteBatch, dotPosition, s_wouldDisplayDot[i].Color, s_wouldDisplayDot[i].DisplayFlags, s_wouldDisplayDot[i].Infinity, s_wouldDisplayDot[i].Next);
                dotPosition += dotDelta;
            }
        }

        if (display.glow_ShowGlow && s_wouldDisplayGlow.Count > 0) {
            int i = s_focusType % s_wouldDisplayGlow.Count;

            if (i < s_wouldDisplayGlow.Count) {
                DisplayGlow(spriteBatch, item, position, origin, frame, scale, s_wouldDisplayGlow[i].Color, (float)s_frame / display.glow_PulseTime, s_wouldDisplayGlow[i].DisplayFlags, s_wouldDisplayGlow[i].Infinity, s_wouldDisplayGlow[i].Next);
            }
        }
    }

    public static DisplayFlags OnLineDisplayFlags => DisplayFlags.All;
    public static void DisplayOnLine(ref string line, ref Color? lineColor, Color color, DisplayFlags displayFlags, Category category, Infinity infinity, ICount nextRequirement, ICount consumableCount) {

        if (displayFlags.HasFlag(DisplayFlags.Infinity)) {
            lineColor = color;
            if (nextRequirement.IsNone) line = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line);
            else line = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite", line, infinity.Value.Display());
        }

        int total = 0;
        string Separator() => total++ == 0 ? " (" : ", ";

        if (displayFlags.HasFlag(DisplayFlags.Category)) line += Separator() + category.Label();

        if (displayFlags.HasFlag(DisplayFlags.Requirement)) {
            line += Separator() + (consumableCount.IsNone ?
                nextRequirement.Display() : consumableCount.DisplayRatio(nextRequirement));
        }
        if (total > 0) line += ")";
    }

    public static DisplayFlags GetDisplayFlags(Category category, Infinity infinity, ICount next) {
        DisplayFlags flags = 0;
        if (!category.IsNone) flags |= DisplayFlags.Category;
        if (!next.IsNone) flags |= DisplayFlags.Requirement;
        if (!infinity.Value.IsNone) flags |= DisplayFlags.Infinity;
        return flags;
    }

    public static readonly Asset<Texture2D> SmallDot = ModContent.Request<Texture2D>("SPIC/Textures/Small_Dot");
    public static readonly Asset<Texture2D> TinyDot = ModContent.Request<Texture2D>("SPIC/Textures/Tiny_Dot");
    public static DisplayFlags DotsDisplayFlags => DisplayFlags.Infinity;
    public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, Color color, DisplayFlags displayFlags, Infinity infinity, ICount next) {
        if (displayFlags.HasFlag(DisplayFlags.Infinity)) {
            if (infinity.Value.IsNone) return;
            spriteBatch.Draw(
                next.IsNone ? SmallDot.Value : TinyDot.Value,
                position,
                null,
                color,
                0f,
                Vector2.Zero,
                Main.inventoryScale,
                SpriteEffects.None,
                0
            );
        }
    }

    public static DisplayFlags GlowDisplayFlags => DisplayFlags.Infinity;
    public static void DisplayGlow(SpriteBatch spriteBatch, Item item, Vector2 position, Vector2 origin, Rectangle frame, float scale, Color color, float ratio, DisplayFlags displayFlags, Infinity infinity, ICount next) {
        if (!displayFlags.HasFlag(DisplayFlags.Infinity)) return;
        if (infinity.Value.IsNone) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        float increase = (ratio < 0.5f ? ratio : 1 - ratio) * 2;
        float maxScale = next.IsNone ? ((frame.Size().X + 4) / frame.Size().X - 1) : 0;
        spriteBatch.Draw(
            TextureAssets.Item[item.type].Value,
            position - frame.Size() / 2 * increase * maxScale * scale * Main.inventoryScale,
            frame,
            color * display.glow_Intensity * increase,
            0,
            origin,
            scale * (increase * maxScale + 1f),
            SpriteEffects.None,
            0f
        );
    }
}

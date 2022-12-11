using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

public record struct DisplayInfo<TCount>(DisplayFlags DisplayFlags, System.Enum? Category, Infinity<TCount> Infinity, TCount Next, TCount ConsumableCount)
where TCount : ICount<TCount>;


public class InfinityDisplayItem : GlobalItem {


    public static IEnumerable<IConsumableGroup> DisplayableTypes(Item item) {
        foreach (IConsumableGroup group in InfinityManager.ConsumableGroups(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled)) {
            if(group.CanDisplay(item)) yield return group;
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!Main.PlayerLoaded || !Config.InfinityDisplay.Instance.toopltip_ShowTooltip) return;

        foreach (IConsumableGroup group in DisplayableTypes(item)) group.ModifyTooltip(item, tooltips);
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if(!Main.PlayerLoaded) return;
        Config.InfinityDisplay display = Config.InfinityDisplay.Instance;
        if (!display.dots_ShowDots && !display.glow_ShowGlow) return;

        s_wouldDisplayDot.Clear();
        s_wouldDisplayGlow.Clear();


        foreach(IConsumableGroup group in DisplayableTypes(item)){
            if (display.dots_ShowDots) group.DrawInInventorySlot(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            if (display.glow_ShowGlow) group.DrawOnItemSprite(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        if (display.dots_ShowDots && s_wouldDisplayDot.Count > 0) {
            s_wouldDisplayDot.Reverse();
            Vector2 cornerDirection = display.dots_Start switch {
                Config.InfinityDisplay.Corner.TopLeft =>     new(-1,-1),
                Config.InfinityDisplay.Corner.TopRight =>    new( 1,-1),
                Config.InfinityDisplay.Corner.BottomLeft =>  new(-1, 1),
                Config.InfinityDisplay.Corner.BottomRight => new( 1, 1),
                _ =>                                         new( 0, 0)
            };
            Vector2 dotSize = _dotSize * DotScale;

            Vector2 slotCenter = position + frame.Size()/2f * scale;
            Vector2 borders = dotSize * 2f/3f;
            Vector2 dotPosition = slotCenter + (TextureAssets.InventoryBack.Value.Size()/2f*Main.inventoryScale - borders) * cornerDirection - dotSize / 2f * Main.inventoryScale;
            Vector2 dotDelta = dotSize * (display.dots_Direction == Config.InfinityDisplay.Direction.Vertical ? new Vector2(0, -cornerDirection.Y) : new Vector2(-cornerDirection.X, 0)) * Main.inventoryScale;

            int pages = (s_wouldDisplayDot.Count + display.dots_Count - 1) / display.dots_Count;
            int startingDot = s_dotFocusIndex % (pages * display.dots_Count) / display.dots_Count * display.dots_Count;

            for (int i = startingDot; i < startingDot + display.dots_Count && i < s_wouldDisplayDot.Count; i++) {
                s_wouldDisplayDot[i].ActualDrawInInventorySlot(item, spriteBatch, dotPosition);
                dotPosition += dotDelta;
            }
        }

        if (display.glow_ShowGlow && s_wouldDisplayGlow.Count > 0) {
            int i = s_glowFocusIndex % s_wouldDisplayGlow.Count;
            if (i < s_wouldDisplayGlow.Count) s_wouldDisplayGlow[i].ActualDrawOnItemSprite(item, spriteBatch, position, frame, origin, scale);
        }
    }

    public static DisplayFlags GetDisplayFlags<TCount>(System.Enum? category, Infinity<TCount> infinity, TCount next, TCount maxInfinity) where TCount : ICount<TCount> {
        DisplayFlags flags = 0;
        if (category != null && System.Convert.ToByte(category) != CategoryHelper.None) flags |= DisplayFlags.Category;
        if (!infinity.Value.IsNone) flags |= DisplayFlags.Infinity;
        if (infinity.Value.CompareTo(maxInfinity) < 0 && !next.IsNone) flags |= DisplayFlags.Requirement;
        
        return flags;
    }


    public static DisplayFlags LineDisplayFlags => DisplayFlags.Infinity | DisplayFlags.Requirement | DisplayFlags.Category;
    public static DisplayFlags DotsDisplayFlags => DisplayFlags.Infinity | DisplayFlags.Requirement;
    public static DisplayFlags GlowDisplayFlags => DisplayFlags.Infinity;

    public static void DisplayOnLine<TCount>(ref string line, ref Color? lineColor, Color color, DisplayInfo<TCount> info) where TCount : ICount<TCount> {
        Config.InfinityDisplay visuals = Config.InfinityDisplay.Instance;

        if (info.DisplayFlags.HasFlag(DisplayFlags.Infinity)) {
            lineColor = color * (Main.mouseTextColor / 255f);
            if (info.Next.IsNone) line = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line);
            else line = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite", line, info.Infinity.Value.Display(visuals.tooltip_RequirementStyle));
        }

        int total = 0;
        string Separator() => total++ == 0 ? " (" : ", ";

        System.Text.StringBuilder addons = new();

        if (info.DisplayFlags.HasFlag(DisplayFlags.Category)) {
            addons.Append(Separator());
            addons.Append(info.Category!.Label());
        }

        if (info.DisplayFlags.HasFlag(DisplayFlags.Requirement)) {
            addons.Append(Separator());
            addons.Append(info.ConsumableCount.IsNone ?
                info.Next.Display(visuals.tooltip_RequirementStyle) :
                $"{info.ConsumableCount.DisplayRawValue(visuals.tooltip_RequirementStyle)} / {info.Next.Display(visuals.tooltip_RequirementStyle)}"
            );
        }
        if (total > 0) addons.Append(')');
        line += addons.ToString();
    }
    public static void DisplayDot<TCount>(SpriteBatch spriteBatch, Vector2 position, Color color, DisplayInfo<TCount> info) where TCount : ICount<TCount> {
        float scale = DotScale * Main.inventoryScale;
        float colorMult = 1;

        // BUG Does clear cache when buying items (visual bug for partial infs)
        if(info.DisplayFlags.HasFlag(DisplayFlags.Infinity) && !info.Infinity.Value.IsNone){
            colorMult = Main.mouseTextColor / 255f;
            for (int i = 0; i < _outerPixels.Length; i++) {
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    position+_outerPixels[i]*scale,
                    new Rectangle(0,0,1,1),
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        float ratio = info.Next.IsNone ? 1 : info.ConsumableCount.Ratio(info.Next);
        for (int i = 0; i < _innerPixels.Length; i++) {
            float alpha;
            if(ratio != 0 && info.DisplayFlags.HasFlag(DisplayFlags.Requirement)) alpha = ratio >= (i + 1f) / _innerPixels.Length ? 1f : 0.5f;
            else if(info.DisplayFlags.HasFlag(DisplayFlags.Infinity) && !info.Infinity.EffectiveRequirement.IsNone) alpha = info.Next.IsNone ? 1f : 0.33f;
            else alpha = 0f;
            
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                position+_innerPixels[i]*scale,
                new Rectangle(0,0,1,1),
                color * alpha * colorMult,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0
            );
        }
    }
    public static void DisplayGlow<TCount>(SpriteBatch spriteBatch, Item item, Vector2 position, Vector2 origin, Rectangle frame, float scale, Color color, DisplayInfo<TCount> info) where TCount : ICount<TCount> {
        if (!info.DisplayFlags.HasFlag(DisplayFlags.Infinity)) return;
        if (info.Infinity.Value.IsNone) return;
        Config.InfinityDisplay display = Config.InfinityDisplay.Instance;

        float ratio = (float)s_glowFrame / display.glow_PulseTime;
        float increase = (ratio < 0.5f ? ratio : 1 - ratio) * 2;
        float maxScale = info.Next.IsNone ? ((frame.Size().X + 4) / frame.Size().X - 1) : 0;
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


    public static void IncrementCounters() {
        s_glowFrame++;
        if (s_glowFrame >= Config.InfinityDisplay.Instance.glow_PulseTime) {
            s_glowFrame = 0;
            s_glowFocusIndex++;
            if (s_glowFocusIndex >= InfinityManager.GroupsLCM) s_glowFocusIndex = 0;
        }
        s_dotFrame++;
        if (s_dotFrame >= Config.InfinityDisplay.Instance.dot_PulseTime) {
            s_dotFrame = 0;
            s_dotFocusIndex++;
            if (s_dotFocusIndex >= InfinityManager.GroupsLCM) s_dotFocusIndex = 0;
        }
    }

    private static int s_glowFrame, s_dotFrame;
    private static int s_glowFocusIndex, s_dotFocusIndex;


    public const int MaxDots = 8;
    public const int DotScale = 2;
    private static Vector2 _dotSize = new(4, 4);
    private static readonly Vector2[] _innerPixels = new Vector2[]{ new(1,1),new(2,1),new(1,2),new(2,2) };
    private static readonly Vector2[] _outerPixels = new Vector2[]{ new(1,0),new(0,1),new(0,2),new(1,3),new(2,3),new(3,2),new(3,1),new(2,0) };

    internal static List<IStandardGroup> s_wouldDisplayDot = new();
    internal static List<IStandardGroup> s_wouldDisplayGlow = new();
}

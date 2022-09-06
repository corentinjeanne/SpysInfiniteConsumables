using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

using SPIC.ConsumableTypes;
namespace SPIC.Globals;

public class InfinityDisplayItem : GlobalItem {

    private static int s_DotFrame = 0;
    private static int s_DotCount = 0;
    private static readonly Asset<Texture2D> s_smallDot = ModContent.Request<Texture2D>("SPIC/Textures/Small_Dot");

    private static int s_HighestDot = -1;

    public override void SetStaticDefaults() {
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        int lcm = 1;
        for (int i = 2; i < InfinityManager.ConsumableTypesCount; i++) {
            lcm = i*lcm/GCD(i, lcm);
        }

        s_HighestDot = lcm;
    }

    public static void IncrementDotFrame() {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        s_DotFrame++;
        if (s_DotFrame < display.glow_PulseTime) return;
        s_DotFrame = 0;
        s_DotCount++;
        if (s_DotCount >= s_HighestDot) s_DotCount = 0;
    }

    // TODO display full infinity on name
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        Player player = Main.LocalPlayer;

        Configs.InfinityDisplay visuals = Configs.InfinityDisplay.Instance;

        void ModifyLine(System.Func<TooltipLine> lineGetter, string missingLinePosition, ConsumableType type, Item item){
            if (!visuals.toopltip_ShowMissingLines && tooltips.FindLine(missingLinePosition) is null) return;

            TooltipLine line = null;
            TooltipLine Line() => line ??= tooltips.FindorAddLine(lineGetter(), missingLinePosition);
            bool showInfinity = false;
            InfinityDisplayLevel displayLevel = type.GetInfinityDisplayLevel(item, true);
            if (displayLevel >= InfinityDisplayLevel.Infinity){
                long infinity = InfinityManager.GetInfinity(player, item, type.UID);
                if (visuals.toopltip_ShowInfinities && infinity > ConsumableType.NotInfinite) {
                    Line().OverrideColor = visuals.Colors[type.Name];
                    showInfinity = true;
                    KeyValuePair<int, long>[] partialInfinities = null;
                    if (type.IsFullyInfinite(item, infinity) || (partialInfinities = type.GetPartialInfinity(item, infinity)).Length == 0) {
                        line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line.Text);
                    } else {
                        string items = "";
                        for (int i = 0; i < partialInfinities.Length; i++) {
                            if (i != partialInfinities.Length - 1) line.Text += ' ';
                            items += visuals.tooltip_UseItemName ? Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteName", Lang.GetItemNameValue(partialInfinities[i].Key), partialInfinities[i].Value) :
                                Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteSprite", partialInfinities[i].Key, partialInfinities[i].Value);
                        }
                        line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite", line.Text, items);
                    }
                }
            }
            if (displayLevel >= InfinityDisplayLevel.Requirement) {
                int total = 0;
                string Separator() => total++ == 0 ? " (" : ", ";
                byte category = InfinityManager.GetCategory(item, type.UID);
                int requirement = InfinityManager.GetRequirement(item, type.UID);
                if (visuals.toopltip_ShowCategories && System.Array.IndexOf(type.HiddenCategories, category) == -1) Line().Text += Separator() + Language.GetTextValue(type.CategoryKey(category));
                if (visuals.toopltip_ShowRequirement && requirement != ConsumableType.NoRequirement && !showInfinity) Line().Text += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.Requirement", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.RequirementStacks", -requirement));
                if (total > 0) line.Text += ")";
            }


        }
        foreach (int usedID in item.UsedConsumableTypes()){
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            ModifyLine(() => type.TooltipLine, type.MissingLinePosition, type, item);
            // BUG ammo inf display for other weapons using this ammo even when not in inventory
            if(type.ConsumesAmmo(item)){
                if(type.HasAmmo(player, item, out Item ammo)) ModifyLine(() =>  type.AmmoLine(item, ammo), "WandConsumes", type, ammo);
            }

        }
    }

    // TODO display full infinity on sprite
    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.dots_ShowDots && !display.glow_ShowGlow) return;
        Player player = Main.LocalPlayer;

        // ? add randomness in the timing

        List<(ConsumableType type, bool fullyInfinity)> infinities = new();
        foreach (int usedID in item.UsedConsumableTypes()) {
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            Item target = null;
            long targetInfinity;
            if((targetInfinity = player.GetInfinity(item, type.UID)) > ConsumableType.NotInfinite)
                target = item;
            else if (type.ConsumesAmmo(item) && type.HasAmmo(player, item, out Item ammo) && (targetInfinity = player.GetInfinity(ammo, type.UID)) > ConsumableType.NotInfinite)
                target = ammo;

            if(target != null) infinities.Add((type, type.IsFullyInfinite(target, targetInfinity)));
        }

        for (int i = infinities.Count - 1; i >= 0; i--){
            if(infinities[i].type.GetInfinityDisplayLevel(item, false) < InfinityDisplayLevel.Infinity)
                infinities.RemoveAt(i);
        }

        if(infinities.Count == 0) return;
        if (display.glow_ShowGlow) {
            int activeInfinitynumber = s_DotCount % infinities.Count;
            (ConsumableType type, bool fullyInfinity) = infinities[activeInfinitynumber];
            float progress = 1 - System.MathF.Abs(display.glow_PulseTime / 2 - s_DotFrame % display.glow_PulseTime) / display.glow_PulseTime * 2;
            float maxScale = fullyInfinity ? ((frame.Size().X + 4) / frame.Size().X - 1) : 0;
            spriteBatch.Draw(
                TextureAssets.Item[item.type].Value,
                position + frame.Size() / 2f * scale,
                frame,
                display.Colors[type.Name] * display.glow_Intensity * progress,
                0,
                origin + frame.Size() / 2f,
                scale * (progress * maxScale + 1f),
                SpriteEffects.None,
                0f
            );
        }
        if (display.dots_ShowDots) {
            Vector2 dotFrameSize = TextureAssets.InventoryBack.Value.Size() * Main.inventoryScale;
            Vector2 pos = position + frame.Size() / 2f * scale - dotFrameSize / 2 + Vector2.One;
            dotFrameSize -= s_smallDot.Size() * Main.inventoryScale;
            Vector2 start = display.dots_Start;
            Vector2 delta = display.dots_PerPage == 1 ? Vector2.Zero : (display.dots_Start - display.dots_End) / (display.dots_PerPage - 1);
           
            int pageCount = (infinities.Count+display.dots_PerPage-1) / display.dots_PerPage;
            int displayedPage = s_DotCount / display.dots_PerPage % pageCount;
            for (int i = 0; i + displayedPage * display.dots_PerPage < infinities.Count && i < display.dots_PerPage; i++) {
                (ConsumableType type, bool fullyInfinity) = infinities[i + displayedPage*display.dots_PerPage];
                spriteBatch.Draw(
                    s_smallDot.Value,
                    pos + dotFrameSize * (start - delta * i),
                    null,
                    display.Colors[type.Name] * (fullyInfinity ? 1f : 0.5f),
                    0f,
                    Vector2.Zero,
                    Main.inventoryScale,
                    SpriteEffects.None,
                    0
                );
            }
        }
    }
}

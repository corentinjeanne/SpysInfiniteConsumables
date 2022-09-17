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

    private static int s_glowFrame = 0;
    private static int s_dotFrame = 0;
    private static int s_dotCount = 0;
    private static readonly Asset<Texture2D> s_smallDot = ModContent.Request<Texture2D>("SPIC/Textures/Small_Dot");
    private static readonly Asset<Texture2D> s_tinyDot = ModContent.Request<Texture2D>("SPIC/Textures/Tiny_Dot");

    private static int s_highestDot = -1;

    public override void SetStaticDefaults() {
        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        int lcm = 1;
        int i = 1;
        foreach (ConsumableType type in InfinityManager.ConsumableTypes) {
            lcm = i*lcm/GCD(i, lcm);
            i++;
        }

        s_highestDot = lcm;
    }

    public static void IncrementDotFrame() {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        s_glowFrame++;
        if (s_glowFrame >= display.glow_PulseTime) s_glowFrame = 0;
        s_dotFrame++;
        if (s_dotFrame < display.dot_PulseTime) return;
        s_dotFrame = 0;
        s_dotCount++;
        if (s_dotCount >= s_highestDot) s_dotCount = 0;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        Player player = Main.LocalPlayer;

        Configs.InfinityDisplay visuals = Configs.InfinityDisplay.Instance;

        void ModifyLine(System.Func<TooltipLine> lineGetter, string missingLinePosition, Color color, ConsumableType infinityValues, Item item, InfinityDisplayFlag displayFlags){
            if (!visuals.toopltip_ShowMissingLines && tooltips.FindLine(missingLinePosition) is null) return;

            TooltipLine line = null;
            TooltipLine Line() => line ??= tooltips.FindorAddLine(lineGetter(), missingLinePosition);
            bool showInfinity = false;
            if (displayFlags.HasFlag(InfinityDisplayFlag.Infinity)){
                long infinity = InfinityManager.GetInfinity(player, item, infinityValues.UID);
                if (visuals.toopltip_ShowInfinities && InfinityManager.IsInfinite(ConsumableType.MinInfinity, infinity)) {
                    showInfinity = true;
                    Line();
                    if(visuals.tooltip_Color) line.OverrideColor = color;

                    KeyValuePair<int, long>[] partialInfinities = null;
                    if (infinityValues is not IPartialInfinity pType || player.HasFullyInfinite(item, infinityValues.UID) || (partialInfinities = pType.GetPartialInfinity(item, infinity)).Length == 0) {
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
                    if (infinityValues is IPartialInfinity pType2){
                        var a = pType2.GetPartialInfinity(item, infinity);
                    }
                }
            }
            int total = 0;
            string Separator() => total++ == 0 ? " (" : ", ";
            byte category = InfinityManager.GetCategory(item, infinityValues.UID);
            int requirement = InfinityManager.GetRequirement(item, infinityValues.UID);
            if (displayFlags.HasFlag(InfinityDisplayFlag.Category) && visuals.toopltip_ShowCategories && System.Array.IndexOf(infinityValues.HiddenCategories, category) == -1)
                Line().Text += Separator() + infinityValues.LocalizedCategoryName(category);
            if (displayFlags.HasFlag(InfinityDisplayFlag.Requirement) && visuals.toopltip_ShowRequirement && requirement != ConsumableType.NoRequirement && !showInfinity)
                Line().Text += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.items", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.stacks", -requirement));
            if (total > 0) Line().Text += ")";
            
        }
        foreach (int usedID in item.UsedConsumableTypes()){
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            ModifyLine(() => type.TooltipLine, type.MissingLinePosition, visuals.Colors[type.ToDefinition()], type, item, type.GetInfinityDisplayLevel(item, true));
        }
        foreach (int usedID in item.UsedAmmoTypes()){
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            IAmmunition aType = (IAmmunition)type;
            Color color = visuals.Colors[type.ToDefinition()];
            if(!aType.HasAmmo(player, item, out Item ammo) || !ammo.IsTypeEnabled(usedID)) continue;
            if(!ammo.IsTypeUsed(type.UID)) type = Mixed.Instance;
            ModifyLine(() =>  aType.AmmoLine(item, ammo), "WandConsumes",color , type, ammo, type.GetInfinityDisplayLevel(item, true) & type.GetInfinityDisplayLevel(ammo, true));
        }


        if(visuals.toopltip_ShowInfinities && Mixed.Instance.GetInfinityDisplayLevel(item, true).HasFlag(InfinityDisplayFlag.Infinity)){
            if(player.HasFullyInfinite(item, Mixed.ID)){
                string infText = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", "").TrimEnd();
                
                Color specialColor = new (Main.DiscoR, Main.DiscoG, Main.DiscoB);
                TooltipLine line = tooltips.FindLine("ItemName");
                if(item.stack > 1) line.Text = line.Text.Replace($"({item.stack})", $"[c/{specialColor.Hex3()}:({infText})]");
                else line.Text += $" [c/{specialColor.Hex3()}:({infText})]";
            }
            else if(player.HasInfinite(item, ConsumableType.MinInfinity, Mixed.ID)){
                long mixed = player.GetInfinity(item, Mixed.ID);
                string items = Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteName", "items", mixed);
                string infText = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite", "", items).Replace("  ", " ");
                
                Color specialColor = new (255, (byte)(Main.masterColor * 200f), 0);
                TooltipLine line = tooltips.FindLine("ItemName");
                if(item.stack > 1) line.Text = line.Text.Replace($"({item.stack})", $"[c/{specialColor.Hex3()}:({item.stack}, {infText})]");
                else line.Text += $" [c/{specialColor.Hex3()}:({infText})]";
            }
        }
    }

    // ? display full infinity on sprite
    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if(!Main.PlayerLoaded) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.dots_ShowDots && !display.glow_ShowGlow) return;
        Player player = Main.LocalPlayer;

        List<(ConsumableType type, bool fullyInfinite)> infinities = new();
        foreach (int usedID in item.UsedConsumableTypes()) {
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            if(player.HasInfinite(item, ConsumableType.MinInfinity, type.UID) && type.GetInfinityDisplayLevel(item, false).HasFlag(InfinityDisplayFlag.Infinity))
                infinities.Add((type, player.HasFullyInfinite(item, usedID)));
        }
        foreach (int usedID in item.UsedAmmoTypes()) {
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            IAmmunition aType = (IAmmunition)type;
            if (aType.HasAmmo(player, item, out Item ammo) && player.HasInfinite(ammo, ConsumableType.MinInfinity, type.UID)
                    && type.GetInfinityDisplayLevel(item, false).HasFlag(InfinityDisplayFlag.Infinity) && type.GetInfinityDisplayLevel(ammo, false).HasFlag(InfinityDisplayFlag.Infinity))
                infinities.Add((type, player.HasFullyInfinite(ammo, usedID)));
        }

        if(infinities.Count == 0) return;
        if (display.glow_ShowGlow) {
            int activeInfinitynumber = s_dotCount % infinities.Count;
            (ConsumableType type, bool fullyInfinite) = infinities[activeInfinitynumber];
            float progress = 1 - System.MathF.Abs(display.glow_PulseTime / 2 - s_glowFrame % display.glow_PulseTime) / display.glow_PulseTime * 2;
            float maxScale = fullyInfinite ? ((frame.Size().X + 4) / frame.Size().X - 1) : 0;
            spriteBatch.Draw(
                TextureAssets.Item[item.type].Value,
                position + frame.Size() / 2f * scale,
                frame,
                display.Colors[type.ToDefinition()] * display.glow_Intensity * progress,
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
            int displayedPage = s_dotCount / display.dots_PerPage % pageCount;
            for (int i = 0; i + displayedPage * display.dots_PerPage < infinities.Count && i < display.dots_PerPage; i++) {
                (ConsumableType type, bool fullyInfinite) = infinities[i + displayedPage*display.dots_PerPage];
                spriteBatch.Draw(
                    fullyInfinite ? s_smallDot.Value : s_tinyDot.Value,
                    pos + dotFrameSize * (start - delta * i),
                    null,
                    display.Colors[type.ToDefinition()] * (fullyInfinite ? 1f : 1f),
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

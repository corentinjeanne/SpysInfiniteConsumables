using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpikysLib;
using SPIC.Default.Displays;
using SPIC.Default.Infinities;
using MonoMod.Cil;
using System;

namespace SPIC.Default.Globals;

public sealed class InfinityDisplayItem : GlobalItem {

    public override void Load() {
        MonoModHooks.Add(typeof(PlayerLoader).GetMethod(nameof(PlayerLoader.ModifyNursePrice)), HookNursePrice);
        MonoModHooks.Add(typeof(ItemLoader).GetMethod(nameof(ItemLoader.ReforgePrice)), HookReforgePrice);
        IL_Main.DrawHairWindow += ILFakeHairPrice;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (Tooltip.Instance.Enabled) Tooltip.ModifyTooltips(item, tooltips);
        if (Configs.InfinityDisplay.Instance.exclusiveDisplay) ModifyItemPrice(item, tooltips);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Main.gameMenu && !Main.ingameOptionsWindow && Glow.Instance.Enabled) Glow.PreDrawInInventory(item, spriteBatch, position, frame, origin, scale);
        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Main.gameMenu && !Main.ingameOptionsWindow && Dots.Instance.Enabled) Dots.PostDrawInInventory(item, spriteBatch, position);
    }

    private delegate void ModifyNursePriceFn(Player player, NPC npc, int health, bool removeDebuffs, ref int price);
    private static void HookNursePrice(ModifyNursePriceFn orig, Player player, NPC npc, int health, bool removeDebuffs, ref int price) {
        orig(player, npc, health, removeDebuffs, ref price);
        if (!(!Configs.InfinityDisplay.Instance.exclusiveDisplay || price <= 0 || !player.HasInfinite(CurrencyHelper.Coins, price, Nurse.Instance))) price = 1;
    }

    private delegate bool ReforgePriceFn(Item item, ref int reforgePrice, ref bool canApplyDiscount);
    private static bool HookReforgePrice(ReforgePriceFn orig, Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        bool res = orig(item, ref reforgePrice, ref canApplyDiscount);
        int price = reforgePrice;
        if (res) {
            if (canApplyDiscount && Main.LocalPlayer.discountAvailable) {
                price = (int)(price * 0.8);
            }
            price = (int)(price * Main.player[Main.myPlayer].currentShoppingSettings.PriceAdjustment);
            price /= 3;
        }
        if (Configs.InfinityDisplay.Instance.exclusiveDisplay && price > 0 && Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, price, Reforging.Instance)) reforgePrice = 1;
        return res;
    }

    public static void ModifyItemPrice(Item item, List<TooltipLine> tooltips) {
        if (!item.isAShopItem && !item.buyOnce) return;
        Main.LocalPlayer.GetItemExpectedPrice(item, out _, out long price);
        if (!Main.LocalPlayer.HasInfinite(item.shopSpecialCurrency, price, Shop.Instance)) return;
        var line = tooltips.FindLine(item.shopSpecialCurrency == -1 ? TooltipLineID.Price : TooltipLineID.SpecialPrice);
        if (line is null) return;
        line.Text = Lang.tip[51].Value;
        float color = Main.mouseTextColor / 255f;
        line.OverrideColor = new Color((byte)(120f * color), (byte)(120f * color), (byte)(120f * color), Main.mouseTextColor);
    }

    private static void ILFakeHairPrice(ILContext il) {
        ILCursor cursor = new(il);
        int priceLoc = 9;
        if (!cursor.TryGotoNext(i => i.MatchLdfld(Reflection.ShoppingSettings.PriceAdjustment))
        || !cursor.TryGotoNext(i => i.MatchStloc(out priceLoc))
        || !cursor.TryGotoNext(i => i.MatchCall(typeof(Math), nameof(Math.Round)))
        || !cursor.TryGotoNext(MoveType.Before, i => i.MatchStloc(priceLoc))) {
            SpysInfiniteConsumables.Instance.Logger.Error($"{nameof(ILFakeHairPrice)} failed to apply. Stylist price will not be modified when they are infinite");
            return;
        }
        if (priceLoc != 9) SpysInfiniteConsumables.Instance.Logger.Warn($"Found loc {priceLoc} but default is {9}");
        cursor.EmitDelegate((int price) => Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, price, Purchase.Instance) ? 1 : price);
    }
}
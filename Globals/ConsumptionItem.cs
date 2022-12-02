using System.Reflection;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SPIC.VanillaGroups;

namespace SPIC.Globals;

public class ConsumptionItem : GlobalItem {

    public override void SetDefaults(Item item) {
        if (item.tileWand != -1) Placeable.RegisterWand(item);
    }

    public override bool ConsumeItem(Item item, Player player) {
        Config.CategoryDetection detection = Config.CategoryDetection.Instance;

        DetectionPlayer detectionPlayer = player.GetModPlayer<DetectionPlayer>();

        // LeftClick
        if (detectionPlayer.InItemCheck) {
            // Consumed by other item, i.e. wand
            if (item != player.HeldItem) {
                if (detection.DetectMissing && item.GetCategory(Placeable.Instance) == PlaceableCategory.None)
                    Config.CategoryDetection.Instance.SaveDetectedCategory(item, PlaceableCategory.Block, Placeable.Instance);

                return !player.HasInfinite(item, 1, Placeable.Instance);
            }

            detectionPlayer.TryDetectCategory();
        } else {
            // RightClick
            if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease) {

                if (item.GetCategory(GrabBag.Instance) == GrabBagCategory.Unknown) {
                    if (item.GetCategory(Usable.Instance) == UsableCategory.Tool)
                        return !player.HasInfinite(item, 1, Usable.Instance);

                    if (detection.DetectMissing) Config.CategoryDetection.Instance.SaveDetectedCategory(item, GrabBagCategory.Crate, GrabBag.Instance);
                }
                return !player.HasInfinite(item, 1, GrabBag.Instance);

            }

            // Hotkey
        }

        // LeftClick
        if (item.GetCategory(Usable.Instance) != UsableCategory.None)
            return !player.HasInfinite(item, 1, Usable.Instance);
        if (item.Placeable())
            return !player.HasInfinite(item, 1, Placeable.Instance);
        return !player.HasInfinite(item, 1, GrabBag.Instance);
    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
        => !player.HasInfinite(ammo, 1, Ammo.Instance);

    public override bool? CanConsumeBait(Player player, Item bait)
        => !player.HasInfinite(bait, 1, Usable.Instance) ?
            null : false;

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        if (!Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, reforgePrice, Currency.Instance)) return false;
        reforgePrice = 0;
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched) {
        int sacrifices = Main.LocalPlayerCreativeTracker.ItemSacrifices.SacrificesCountByItemIdCache[item.type];
        int researchCost = Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
        int consumed = System.Math.Min(Utils.Clamp(researchCost - sacrifices, 0, researchCost), item.stack);
        if (Main.LocalPlayer.HasInfinite(item, consumed, JourneySacrifice.Instance))
            item.stack += consumed;
    }
}

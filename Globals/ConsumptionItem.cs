using Terraria;
using Terraria.ModLoader;

using SPIC.Infinities;
using Terraria.ID;

namespace SPIC.Globals;

public class ConsumptionItem : GlobalItem {

    public override bool ConsumeItem(Item item, Player player) {

        DetectionPlayer detectionPlayer = player.GetModPlayer<DetectionPlayer>();

        
        if(Configs.InfinitySettings.Instance.DetectMissingCategories) detectionPlayer.TryDetectCategory(true);

        // LeftClick
        if (detectionPlayer.InItemCheck) {
            if (item != player.HeldItem) { // Wands
                if(item.type == ItemID.DD2EnergyCrystal) return !player.HasInfinite(item, 1, Ammo.Instance);

                return !player.HasInfinite(item, 1,
                    () => player.HeldItem.damage != 0 ? InfinityManager.SaveDetectedCategory(item, AmmoCategory.Special, Ammo.Instance): InfinityManager.SaveDetectedCategory(item, PlaceableCategory.Block, Placeable.Instance),
                    Placeable.Instance, Ammo.Instance
                );
            }

            int tileTarget = Main.tile[Player.tileTargetX, Player.tileTargetY].TileType;
            if(tileTarget == TileID.Extractinator || tileTarget == TileID.ChlorophyteExtractinator) return !player.HasInfinite(item, 1, Usable.Instance, GrabBag.Instance);
            return !player.HasInfinite(item, 1, Usable.Instance, Placeable.Instance, GrabBag.Instance);

        } else if(DetectionPlayer.InRightClick)
            return !player.HasInfinite(item, 1,
                () => InfinityManager.SaveDetectedCategory(item, GrabBagCategory.Container, GrabBag.Instance),
                GrabBag.Instance, Usable.Instance
            );
        else { // Hotkey or special right click action
            return !player.HasInfinite(item, 1,
                () => InfinityManager.SaveDetectedCategory(item, GrabBagCategory.Container, GrabBag.Instance)
                , Usable.Instance, GrabBag.Instance
            );
        }

    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => !player.HasInfinite(ammo, 1, Ammo.Instance);

    public override bool? CanConsumeBait(Player player, Item bait) => !player.HasInfinite(bait, 1, Usable.Instance) ? null : false;

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        if (!Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, reforgePrice, Shop.Instance)) return false;
        reforgePrice = 0;
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched) {
        int sacrifices = Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type);
        int researchCost = Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
        int consumed = System.Math.Min(Utils.Clamp(researchCost - sacrifices, 0, researchCost), item.stack);
        if (Main.LocalPlayer.HasInfinite(item, consumed, JourneySacrifice.Instance)) item.stack += consumed;
    }
}

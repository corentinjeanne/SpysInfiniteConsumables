using Terraria;
using Terraria.ModLoader;

using SPIC.VanillaGroups;
using Terraria.ID;

namespace SPIC.Globals;

public class ConsumptionItem : GlobalItem {

    
    public override void SetStaticDefaults() {
        for(int t = 0; t < ItemLoader.ItemCount; t++){
            Item i = new(t);
            if (i.tileWand != -1) Placeable.RegisterWand(i);
        }
    }

    public override void RightClick(Item item, Player player) {
        if(item.type != ItemID.CanOfWorms && item.type != ItemID.Oyster) return;
        if(player.HasInfinite(item, 1,
            () => Configs.CategoryDetection.Instance.SaveDetectedCategory(item, GrabBagCategory.Crate, GrabBag.Instance),
            GrabBag.Instance
        )) item.stack++;
    }

    public override bool ConsumeItem(Item item, Player player) {
        Configs.CategoryDetection detection = Configs.CategoryDetection.Instance;

        DetectionPlayer detectionPlayer = player.GetModPlayer<DetectionPlayer>();

        
        if(detection.DetectMissing) detectionPlayer.TryDetectCategory(true);

        // LeftClick
        if (detectionPlayer.InItemCheck) {
            if (item != player.HeldItem) { // Wands
                if(item.type == ItemID.DD2EnergyCrystal) return !player.HasInfinite(item, 1, Ammo.Instance);
                return !player.HasInfinite(item, 1,
                    () => player.HeldItem.damage != 0 ? Configs.CategoryDetection.Instance.SaveDetectedCategory(item, AmmoCategory.Special, Ammo.Instance): Configs.CategoryDetection.Instance.SaveDetectedCategory(item, PlaceableCategory.Block, Placeable.Instance),
                    Placeable.Instance, Ammo.Instance
                );
            }

            return !player.HasInfinite(item, 1, Usable.Instance, Placeable.Instance, GrabBag.Instance);

        } else if(DetectionPlayer.InRightClick)
            return !player.HasInfinite(item, 1,
                () => Configs.CategoryDetection.Instance.SaveDetectedCategory(item, GrabBagCategory.Crate, GrabBag.Instance),
                GrabBag.Instance, Usable.Instance
            );
        else { // Hotkey or special right click action
            return !player.HasInfinite(item, 1,
                () => Configs.CategoryDetection.Instance.SaveDetectedCategory(item, GrabBagCategory.Crate, GrabBag.Instance)
                , Usable.Instance, GrabBag.Instance
            );
        }

    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => !player.HasInfinite(ammo, 1, Ammo.Instance);

    public override bool? CanConsumeBait(Player player, Item bait) => !player.HasInfinite(bait, 1, Usable.Instance) ? null : false;

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        if (!Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, reforgePrice, Currency.Instance)) return false;
        reforgePrice = 0;
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched) {
        int sacrifices = Main.LocalPlayerCreativeTracker.ItemSacrifices.SacrificesCountByItemIdCache[item.type];
        int researchCost = Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
        int consumed = System.Math.Min(Utils.Clamp(researchCost - sacrifices, 0, researchCost), item.stack);
        if (Main.LocalPlayer.HasInfinite(item, consumed, JourneySacrifice.Instance)) item.stack += consumed;
    }
}

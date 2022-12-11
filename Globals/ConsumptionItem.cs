using Terraria;
using Terraria.ModLoader;

using SPIC.VanillaGroups;

namespace SPIC.Globals;

public class ConsumptionItem : GlobalItem {

    
    public override void SetStaticDefaults() {
        for(int t = 0; t < ItemLoader.ItemCount; t++){
            Item i = new(t);
            if (i.tileWand != -1) Placeable.RegisterWand(i);
        }
    }

    public override bool ConsumeItem(Item item, Player player) {
        Config.CategoryDetection detection = Config.CategoryDetection.Instance;

        DetectionPlayer detectionPlayer = player.GetModPlayer<DetectionPlayer>();

        
        if(detection.DetectMissing) detectionPlayer.TryDetectCategory(true);

        // LeftClick
        if (detectionPlayer.InItemCheck) {
            if (item != player.HeldItem) {
                if (detection.DetectMissing && item.GetCategory(Placeable.Instance) == PlaceableCategory.None)
                    Config.CategoryDetection.Instance.SaveDetectedCategory(item, PlaceableCategory.Block, Placeable.Instance);

                return !player.HasInfinite(item, 1, Placeable.Instance);
            }

            if (!item.GetRequirement(Usable.Instance).IsNone) return !player.HasInfinite(item, 1, Usable.Instance);
            if (!item.GetRequirement(Placeable.Instance).IsNone) return !player.HasInfinite(item, 1, Placeable.Instance);
            return !player.HasInfinite(item, 1, GrabBag.Instance);


        } else if(DetectionPlayer.InRightClick){
            if (!item.GetRequirement(GrabBag.Instance).IsNone) return !player.HasInfinite(item, 1, GrabBag.Instance);
            return !player.HasInfinite(item, 1, Usable.Instance);
        
        } else { // Hotkey
            return !player.HasInfinite(item, 1, Usable.Instance);
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

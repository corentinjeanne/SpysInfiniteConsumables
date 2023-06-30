using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using SPIC.VanillaGroups;
using Terraria.DataStructures;

namespace SPIC.Globals;

public class ConsumptionNPC : GlobalNPC {
    
    public override void Load() {
        On_Chest.SetupShop_string_NPC += HookSetupShop;
        On_NPC.ReleaseNPC += HookReleaseNPC;
    }
    private static void HookSetupShop(On_Chest.orig_SetupShop_string_NPC orig, Chest self, string shopName, NPC npc) {
        orig(self, shopName, npc);

        _hightestCost.Clear();

        static void UpdatePrice(int currency, int value){
            if (HighestShopValue(currency) < value) _hightestCost[currency] = value;
            if (HighestEverValue(currency, 0) < value) _highestItemValue[currency] = value;
        }

        foreach (Item item in self.item) {
            if (item.IsAir) continue;

            if (item.shopCustomPrice.HasValue) {
                UpdatePrice(item.shopSpecialCurrency, item.shopCustomPrice.Value);
                if (Main.LocalPlayer.HasInfinite(item.shopSpecialCurrency, item.shopCustomPrice.Value, Currency.Instance)) item.shopCustomPrice = item.value = 0;
            }
            else {
                UpdatePrice(CurrencyHelper.Coins, item.value);
                if (Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, item.value, Currency.Instance)) item.value = 0;
            }
        }
    }


    private static int HookReleaseNPC(On_NPC.orig_ReleaseNPC orig, int x, int y, int Type, int Style, int who) { // TODO >>> refactor into OnSpawn
        int spawnIndex = orig(x, y, Type, Style, who);

        // Prevent duping
        if (spawnIndex >= 0) {
            NPC critter = Main.npc[spawnIndex];
            if (Configs.GroupSettings.Instance.PreventItemDupication && Main.player[who].HasInfinite(new(critter.catchItem), 1, Usable.Instance))
                critter.SpawnedFromStatue = true;
        }
        return spawnIndex;
    }


    public static long HighestShopValue(int currency, long missing = 0) => _hightestCost.ContainsKey(currency) ? _hightestCost[currency] : missing;
    public static long HighestEverValue(int currency, long missing = long.MaxValue) => _highestItemValue.ContainsKey(currency) ? _highestItemValue[currency] : missing;

    private static readonly System.Collections.Generic.Dictionary<int, long> _hightestCost = new();
    private static readonly System.Collections.Generic.Dictionary<int, long> _highestItemValue = new();
}

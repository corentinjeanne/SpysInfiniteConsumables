using Terraria;
using Terraria.ModLoader;
using SPIC.Infinities;
using Terraria.DataStructures;

namespace SPIC.Globals;

public sealed class ConsumptionNPC : GlobalNPC {
    
    public override void Load() {
        On_Chest.SetupShop_string_NPC += HookSetupShop;
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
                if (Main.LocalPlayer.HasInfinite(item.shopSpecialCurrency, item.shopCustomPrice.Value, Shop.Instance)) item.shopCustomPrice = item.value = 0;
            }
            else {
                UpdatePrice(CurrencyHelper.Coins, item.value);
                if (Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, item.value, Shop.Instance)) item.value = 0;
            }
        }
    }

    public override void OnSpawn(NPC npc, IEntitySource source) {
        if(source is EntitySource_Parent parent && Configs.InfinitySettings.Instance.PreventItemDupication
                && parent.Entity is Player player && player.HasInfinite(new(npc.catchItem), 1, Usable.Instance)) {
            npc.SpawnedFromStatue = true;
        }
    }

    public static long HighestShopValue(int currency, long missing = 0) => _hightestCost.ContainsKey(currency) ? _hightestCost[currency] : missing;
    public static long HighestEverValue(int currency, long missing = long.MaxValue) => _highestItemValue.ContainsKey(currency) ? _highestItemValue[currency] : missing;

    private static readonly System.Collections.Generic.Dictionary<int, long> _hightestCost = new();
    private static readonly System.Collections.Generic.Dictionary<int, long> _highestItemValue = new();
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using SPIC.VanillaGroups;

namespace SPIC.Globals;

public class SpicNPC : GlobalNPC {

    public static long HighestPrice(int currency) => _hightestCost.ContainsKey(currency) ? _hightestCost[currency] : 0;
    public static long HighestItemValue(int currency) => _highestItemValue.ContainsKey(currency) ? _highestItemValue[currency] : 0;

    
    public override void Load() {
        On.Terraria.Chest.SetupShop += HookSetupShop;
        On.Terraria.NPC.ReleaseNPC += HookReleaseNPC;
    }


    private static void HookSetupShop(On.Terraria.Chest.orig_SetupShop orig, Chest self, int type) {
        orig(self, type);

        _hightestCost.Clear();
        foreach (Item item in self.item) {
            if (item.IsAir) continue;

            if (item.shopCustomPrice.HasValue) {
                if (HighestPrice(item.shopSpecialCurrency) < item.shopCustomPrice.Value) _hightestCost[item.shopSpecialCurrency] = item.shopCustomPrice.Value;
                if (HighestItemValue(item.shopSpecialCurrency) < item.shopCustomPrice.Value) _highestItemValue[item.shopSpecialCurrency] = item.shopCustomPrice.Value;
                
                if (Main.LocalPlayer.HasInfinite(item.shopSpecialCurrency, item.shopCustomPrice.Value, Currency.Instance)) item.shopCustomPrice = item.value = 0;
            } else {
                if (HighestPrice(CurrencyHelper.Coins) < item.value) _hightestCost[CurrencyHelper.Coins] = item.value;
                if (HighestItemValue(CurrencyHelper.Coins) < item.value) _highestItemValue[CurrencyHelper.Coins] = item.value;
                
                if (Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, item.value, Currency.Instance)) item.value = 0;
            }
        }
    }


    private static void HookReleaseNPC(On.Terraria.NPC.orig_ReleaseNPC orig, int x, int y, int Type, int Style, int who) {
        if (Main.netMode == NetmodeID.MultiplayerClient || Type < 0 || who < 0 || !Main.npcCatchable[Type] || !NPC.CanReleaseNPCs(who)) {
            orig(x, y, Type, Style, who);
            return;
        }

        // Find npc spawn slot
        int spawnIndex = -1;
        if (NPCID.Sets.SpawnFromLastEmptySlot[Type]) {
            for (int i = 199; i >= 0; i--) {
                if (!Main.npc[i].active) {
                    spawnIndex = i;
                    break;
                }
            }
        } else {
            for (int i = 0; i < 200; i++) {
                if (!Main.npc[i].active) {
                    spawnIndex = i;
                    break;
                }
            }
        }

        orig(x, y, Type, Style, who);

        // Prevent duping
        if (spawnIndex > 0) {
            NPC critter = Main.npc[spawnIndex];
            if (critter.active && critter.type == Type && Config.RequirementSettings.Instance.PreventItemDupication && Main.player[who].HasInfinite(new(critter.catchItem), 1, Usable.Instance))
                critter.SpawnedFromStatue = true;
        }
    }


    private static readonly System.Collections.Generic.Dictionary<int, long> _hightestCost = new();
    private static readonly System.Collections.Generic.Dictionary<int, long> _highestItemValue = new();
}

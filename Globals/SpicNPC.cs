using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using SPIC.VanillaConsumableTypes;

namespace SPIC.Globals {

    public class SpicNPC : GlobalNPC {


        public override void Load() {
            On.Terraria.Chest.SetupShop += HookSetupShop;
            On.Terraria.NPC.ReleaseNPC += HookReleaseNPC;
        }
        private static readonly System.Collections.Generic.Dictionary<int, long> _hightestCost = new();
        public static long HighestPrice(int currency) => _hightestCost.ContainsKey(currency) ? _hightestCost[currency] : 0;

        public static readonly System.Collections.Generic.Dictionary<int, long> _highestItemValue = new();
        public static long HighestItemValue(int currency) => _highestItemValue.ContainsKey(currency) ? _highestItemValue[currency] : long.MaxValue;

        private static void HookSetupShop(On.Terraria.Chest.orig_SetupShop orig, Chest self, int type) {
            orig(self, type);
            // InfinityPlayer infinityPlayer = Main.LocalPlayer.GetModPlayer<InfinityPlayer>();
            _hightestCost.Clear();
            foreach (Item item in self.item)  {
                if (item.IsAir) continue;
                if (item.shopCustomPrice.HasValue) {
                    if (!_hightestCost.ContainsKey(item.shopSpecialCurrency) || _hightestCost[item.shopSpecialCurrency] < item.shopCustomPrice.Value)
                        _hightestCost[item.shopSpecialCurrency] = item.shopCustomPrice.Value;
                    if (!_highestItemValue.ContainsKey(item.shopSpecialCurrency) || _highestItemValue[item.shopSpecialCurrency] < item.shopCustomPrice.Value)
                        _highestItemValue[item.shopSpecialCurrency] = item.shopCustomPrice.Value;
                    if (Main.LocalPlayer.HasInfinite(item.shopSpecialCurrency, item.shopCustomPrice.Value, Currency.ID))
                        item.shopCustomPrice = item.value = 0;
                } else {
                    if (!_hightestCost.ContainsKey(CurrencyHelper.Coins) || _hightestCost[CurrencyHelper.Coins] < item.value) _hightestCost[CurrencyHelper.Coins] = item.value;
                    if (!_highestItemValue.ContainsKey(CurrencyHelper.Coins) || _highestItemValue[CurrencyHelper.Coins] < item.value) _highestItemValue[CurrencyHelper.Coins] = item.value;
                    if (Main.LocalPlayer.HasInfinite(CurrencyHelper.Coins, item.value, Currency.ID))
                        item.value = 0;
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
            }
            else {
                for (int i = 0; i < 200; i++) {
                    if (!Main.npc[i].active) {
                        spawnIndex = i;
                        break;
                    }
                }
            }
            
            orig(x, y, Type, Style, who);

            // Prevent duping
            if(spawnIndex > 0) {
                NPC critter = Main.npc[spawnIndex];
                if (critter.active && critter.type == Type && Configs.RequirementSettings.Instance.PreventItemDupication && Main.player[who].HasInfinite(critter.catchItem, 1, Usable.ID))
                    critter.SpawnedFromStatue = true;
            }
        }
    }
}
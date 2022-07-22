using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SPIC.Globals {

    public class SpicNPC : GlobalNPC {


        public override void Load() {
            On.Terraria.Chest.SetupShop += HookSetupShop;
            On.Terraria.NPC.ReleaseNPC += HookReleaseNPC;
        }

        private static void HookSetupShop(On.Terraria.Chest.orig_SetupShop orig, Chest self, int type) {
            orig(self, type);
            SpicPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<SpicPlayer>();
            foreach (Item item in self.item)  {
                if (item.IsAir) continue;
                if (item.shopCustomPrice.HasValue) {
                    if (item.shopCustomPrice.Value <= spicPlayer.GetCurrencyInfinity(item.shopSpecialCurrency))
                        item.shopCustomPrice = item.value = 0;
                }
                else if (item.value <= spicPlayer.GetCurrencyInfinity(-1))
                    item.value = 0;

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
                if (critter.active && critter.type == Type
                        && Configs.Requirements.Instance.PreventItemDupication
                        && 1 <= Main.player[who].GetModPlayer<SpicPlayer>().GetInfinities(critter.catchItem).Consumable)
                    critter.SpawnedFromStatue = true;
            }
        }
    }
}
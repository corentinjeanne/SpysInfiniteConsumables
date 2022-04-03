using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SPIC.Globals {

	public class SpicNPC : GlobalNPC {


		public override void Load() {
			On.Terraria.NPC.ReleaseNPC += HookReleaseNPC;
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
				if (critter.active && critter.type == Type) {
					if(ModContent.GetInstance<Configs.ConsumableConfig>().PreventItemDupication && Main.player[who].HasInfinite(critter.catchItem, Categories.Consumable.Critter)){
						critter.SpawnedFromStatue = true;
					}
				}
			}

		}
	}
}
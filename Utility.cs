using Terraria;
using Terraria.ModLoader;

namespace SPIC {

	public struct NPCStats {
		public int boss, total;
	}
	public static class Utility {

		public static int WorldDifficulty() => Main.masterMode ? 2 : Main.expertMode ? 1 : 0;

		public static int NameToType(string name, bool noCaps = true){
			string fullName = name.Replace("_", " ");
			if(noCaps) fullName = fullName.ToLower();
			for (var k = 0; k < ItemLoader.ItemCount; k++) {
				string testedName = noCaps ? Lang.GetItemNameValue(k).ToLower() : Lang.GetItemNameValue(k);
				if (fullName == testedName) {
					return k;
				}
			}
			throw new UsageException("Invalid Name" + name);
		}
		public static string TypeToName(int type) {
			Item item = new Item(type);
			if(item.IsAir) throw new UsageException("Invalid type" + type);
			return item.Name;
		}

		public static bool Placeable(this Item item) => item.createTile != -1 || item.createWall != -1;

		public static int CountInContainer(Item[] container, int type) {
			int total = 0;
			foreach (Item i in container) {
				if (i.type == type) total += i.stack;
			}
			return total;
		}
		public static void RemoveFromInventory(this Player player, int type, int count = 1) {
			foreach (Item i in player.inventory) {
				if (i.type != type) continue;
				if (i.stack > count) {
					i.stack -= count;
					return;
				}
				count -= i.stack;
				i.TurnToAir();
			}
		}
		public static int CountAllItems(this Player player, int type, bool includechest = false){
			int total = CountInContainer(player.inventory, type);

			if (!includechest) return total;
			return total + player.chest switch {
				-1 => 0,
				-2 => CountInContainer(player.bank.item, type),
				-3 => CountInContainer(player.bank.item, type),
				-4 => CountInContainer(player.bank.item, type),
				-5 => CountInContainer(player.bank.item, type),
				_ =>  CountInContainer(Main.chest[player.chest].item, type)
			};
		}

		public static NPCStats GetNPCStats() {
			NPCStats stats = new NPCStats();
			foreach (NPC npc in Main.npc) {
				if (npc.active) {
					stats.total++;
					if(npc.boss) stats.boss++;
				}
			}
			return stats;
		}
	}
}
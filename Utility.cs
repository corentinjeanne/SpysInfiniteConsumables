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
			if(Main.mouseItem is not null && Main.mouseItem.type == type) total += Main.mouseItem.stack;
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
			NPCStats stats = new ();
			foreach (NPC npc in Main.npc) {
				if (npc.active) {
					stats.total++;
					if(npc.boss) stats.boss++;
				}
			}
			return stats;
		}

		public static int InfinityToItems(int infinity, int type, int LargestStack = 999) {
			int maxStack = Globals.SpicItem.MaxStack(type);
			int items = infinity >= 0 ? infinity > maxStack ? maxStack : infinity :
				-infinity * (maxStack < LargestStack ? maxStack : LargestStack);

			return infinity == 0 ? int.MaxValue : items;
		}
	}
}
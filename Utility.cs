using Terraria;
using Terraria.ModLoader;

namespace SPIC {

	public static class Utility {

        public static bool ModifiedMaxStack {get; private set;}


		public static int WorldDifficulty => Main.masterMode ? 2 : Main.expertMode ? 1 : 0;

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
			if(item.IsAir)
				throw new UsageException("Invalid type" + type);
			return item.Name;
		}

		public static bool Placeable(this Item item) => item.createTile != -1 || item.createWall != -1;
		public static void RemoveFromInventory(this Player player, Item item, int count = 1) => player.RemoveFromInventory(item.type, count);
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
		public static int CountAllItems(this Player player, Item item) => player.CountAllItems(item.type);
		public static int CountAllItems(this Player player, int type){
			int total = 0;
			foreach(Item i in player.inventory) {
				if (i.type == type) total += i.stack;
			}
			return total;
		}
		public static bool BusyWithInvasion() {
			return Main.invasionType != 0;
		}

		public static int BossCount() {
			int total = 0;
			foreach (NPC npc in Main.npc) {
				if (npc.active && npc.boss) total++;
			}
			return total;
		}
	}
}
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

using SPIC.Categories;

namespace SPIC.Globals {

	public class SpicRecipe : GlobalRecipe {

		public static List<int> CraftingStations;

		public override void SetStaticDefaults() {
			CraftingStations = new();
			foreach(Recipe r in Main.recipe) {
				foreach(int t in r.requiredTile) {
					if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
				}
			}
		}

		public override void ConsumeItem(Recipe recipe, int type, ref int amount) {
			Player player = Main.player[Main.myPlayer];
			if (player == null) return;

			if (player.HasInfiniteMaterial(new Item(type))) {
				amount = 0;
			}
		}
		
	}

}
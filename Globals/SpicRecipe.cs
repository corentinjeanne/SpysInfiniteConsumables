using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace SPIC.Globals {

	public class SpicRecipe : GlobalRecipe {

		public static readonly List<int> CraftingStations = new();

		public override void SetStaticDefaults() {
			CraftingStations.Clear();
			foreach(Recipe r in Main.recipe) {
				foreach(int t in r.requiredTile) {
					if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
				}
			}
		}

		public override void ConsumeItem(Recipe recipe, int type, ref int amount) {
			Player player = Main.player[Main.myPlayer];
			if (player == null || !Configs.ConsumableConfig.Instance.InfiniteCrafting) return;

			if (player.HasInfinite(type, new Item(type).GetMaterialCategory())) {
				amount = 0;
			}
		}
		
	}

}
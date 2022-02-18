using SPIC.Config;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Categories {
	public static class Material {
		public enum Category {
			NotaMaterial,
			Single,
			Stack
		}
		public static Category? CategoryFromConfigCategory(ConsumableConfig.Category category) {
			return category switch {
				_ => null,
			};
		}
		public static int LargestStack(Category category) {
			return category switch {
				Category.NotaMaterial => 999,
				Category.Single => 1,
				Category.Stack => 999,
				_ => 999,
			};
		}
		public static Category GetCategory(int type) {
			Item item = new(type);
			return item.GetMaterialCategory();
		}
		public static Category GetMaterialCategory(this Item item) {

			if (item == null || !item.material) return Category.NotaMaterial;

			return Globals.SpicItem.MaxStack(item.type) switch {
				1 => Category.Single,
				_ => Category.Stack
			};
		}

		public static bool IsInfiniteMaterial(this Item item) {
			if (item.playerIndexTheItemIsReservedFor == -1) return false;
			return Main.player[item.playerIndexTheItemIsReservedFor].HasInfiniteMaterial(item);
		}

		public static bool HasInfiniteMaterial(this Player player, Item item) {
			Category category = item.GetMaterialCategory();
			return player.HasInfinite(item.type, category);
		}

		public static bool HasInfinite(this Player player, int type, Category category) {
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			if (!config.InfiniteCrafting || category == Category.NotaMaterial) return false;


			if (config.JourneyRequirement)
				return player.CountAllItems(type) >= CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];

			int infinityCount = category switch {
				Category.Single => config.InfinityRequirement(config.wiring, type, LargestStack(category)),
				Category.Stack => config.InfinityRequirement(config.wiring, type, LargestStack(category)),
				_ => throw new System.NotImplementedException()
			};
			int playerTotal = player.CountAllItems(type);
			return playerTotal >= infinityCount;
		}
	}
}
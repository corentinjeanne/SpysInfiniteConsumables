using SPIC.Config;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

using Terraria.ID;

namespace SPIC.Categories {
	public static class Material {
		public enum Category {
			NotaMaterial,
			Basic,//
			Ore,//
			Furniture,//
			MaterialOnly,
			NonStackable//
		}
		public static Category? CategoryFromConfigCategory(ConsumableConfig.Category category) {
			return category switch {
				_ => null,
			};
		}
		public static int LargestStack(Category category) {
			return category switch {
				Category.NotaMaterial => 999,
				Category.Basic => 999,
				Category.Ore => 999,
				Category.Furniture => 99,
				Category.MaterialOnly => 999,
				Category.NonStackable => 1,
				_ => throw new System.NotImplementedException(),
			};
		}
		public static Category GetCategory(int type) {
			Item item = new(type);
			return item.GetMaterialCategory();
		}
		public static Category GetMaterialCategory(this Item item) {

			int type = item.type;
			if (item == null || !item.material) return Category.NotaMaterial;

			if (Globals.SpicItem.MaxStack(type) == 1) return Category.NonStackable;

			Consumable.Category category = item.GetConsumableCategory() ?? Consumable.Category.NotConsumable;

			if (Consumable.IsFurnitureCategory(category)) return Category.Furniture;

			if(category == Consumable.Category.Ore || type == ItemID.Hellstone) return Category.Ore;

			if (Consumable.IsCommonBlockCategory(category)
				|| type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow 
				|| type == ItemID.Wire || type == ItemID.BottledWater
				|| type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
				return Category.Basic;

			return Category.MaterialOnly;
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

			int playerTotal = player.CountAllItems(type, includechest: true);

			if (config.JourneyRequirement)
				return playerTotal >= CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];

			int infinityCount = category switch {
				Category.Basic => config.InfinityRequirement(config.craftingBasis, type, LargestStack(category)),
				Category.Ore => config.InfinityRequirement(config.craftingOres, type, LargestStack(category)),
				Category.Furniture => config.InfinityRequirement(config.craftingFurnitures, type, LargestStack(category)),
				Category.MaterialOnly => config.InfinityRequirement(config.craftingMaterials, type, LargestStack(category)),
				Category.NonStackable => config.InfinityRequirement(config.craftingSingleStack, type, LargestStack(category)),
				_ => throw new System.NotImplementedException(),
			};
			return playerTotal >= infinityCount;
		}
	}
}
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {

	namespace Categories {
		public enum Material {
			None,
			Basic,
			Ore,
			Furniture,
			Miscellaneous,
			NonStackable
		}
	}
	public static class MaterialExtension {
		public static int MaxStack(this Material category) => category switch {
			Material.Basic => 999,
			Material.Ore => 999,
			Material.Furniture => 99,
			Material.Miscellaneous => 999,
			Material.NonStackable => 1,
			Material.None => 999,
			_ => throw new System.NotImplementedException(),
		};
		public static int Infinity(this Material material) {
			Configs.Materials m = Configs.ConsumableConfig.Instance.Materials;

			return material switch {
				Material.Basic => m.Basics,
				Material.Ore => m.Ores,
				Material.Furniture => m.Furnitures,
				Material.Miscellaneous => m.Miscellaneous,
				Material.NonStackable => m.NonStackable,
				Material.None => 0,
				_ => throw new System.NotImplementedException(),
			};
		}
		public static Material GetMaterialCategory(this Item item) {

			int type = item.type;
			if (item == null || !item.material) return Material.None;

			if (Globals.SpicItem.MaxStack(type) == 1) return Material.NonStackable;

			Consumable consumable = item.GetConsumableCategory() ?? Consumable.None;

			if (consumable.IsFurniture()) return Material.Furniture;

			if(consumable == Consumable.Ore) return Material.Ore;

			if (consumable.IsCommonTile()
					|| type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow 
					|| type == ItemID.Wire || type == ItemID.BottledWater
					|| type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
				return Material.Basic;

			return Material.Miscellaneous;
		}

		public static bool IsInfiniteMaterial(this Item item) {
			if (item.playerIndexTheItemIsReservedFor == -1) return false;
			return Main.player[item.playerIndexTheItemIsReservedFor].HasInfiniteMaterial(item);
		}

		public static bool HasInfiniteMaterial(this Player player, Item item) => player.HasInfinite(item.type, item.GetMaterialCategory());
		public static bool HasInfinite(this Player player, int type, Material material) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

			if (material == Material.None) return false;

			int infinityCount = config.JourneyRequirement ?
				CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type] :
				Utility.InfinityToItems(material.Infinity(), type, material.MaxStack());
			

			return infinityCount >= player.CountAllItems(type, true);
		}
	}
}
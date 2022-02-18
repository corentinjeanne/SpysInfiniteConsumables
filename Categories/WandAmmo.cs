using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

using SPIC.Config;
using Terraria.GameContent.Creative;

namespace SPIC.Categories {
	public static class WandAmmo {

		public static List<int> wandAmmoTypes = new();
		public enum Category {
			NotWandAmmo,
			Wiring,
			WandAmmo,
		}
		public static Category? CategoryFromConfigCategory(ConsumableConfig.Category category) {
			return category switch {
				ConsumableConfig.Category.Blacklist => Category.NotWandAmmo,
				ConsumableConfig.Category.Block => throw new System.NotImplementedException(),
				ConsumableConfig.Category.Wiring => throw new System.NotImplementedException(),
				ConsumableConfig.Category.Ammo => throw new System.NotImplementedException(),
				ConsumableConfig.Category.Special => throw new System.NotImplementedException(),
				_ => null
			};
		}
		public static int LargestStack(Category category) {
			return category switch {
				Category.WandAmmo => 999,
				Category.Wiring => 999,
				_ => 999,
			};
		}
		public static Category? GetCategory(int type) {
			Item item = new(type);
			return item.GetWandAmmoCategory();
		}
		public static Category? GetWandAmmoCategory(this Item item) {

			if (item == null) return Category.NotWandAmmo;

			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
			if (config.HasCustomCategory(item, out ConsumableConfig.Category custom)) {
				Category? category = CategoryFromConfigCategory(custom);
				if (category.HasValue) return category.Value;
			}
			if (wandAmmoTypes.Contains(item.type)) return Category.WandAmmo;
			if (item.FitsAmmoSlot() && item.mech) return Category.Wiring;
			return null;
		}

		public static bool IsInfiniteWandAmmo(this Item item) {
			if (item.playerIndexTheItemIsReservedFor == -1) return false;
			return Main.player[item.playerIndexTheItemIsReservedFor].HasInfiniteAmmo(item);
		}

		public static bool HasInfiniteWandAmmo(this Player player, Item item) {
			Category? category = item.GetWandAmmoCategory();
			return category.HasValue ? player.HasInfinite(item.type, category.Value) : false;
		}

		public static bool HasInfinite(this Player player, int type, Category category) {
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			if (!config.InfiniteTiles || category == Category.NotWandAmmo) return false;

			if (config.JourneyRequirement)
				return player.CountAllItems(type) >= CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];


			if (!config.HasCustomInfinity(type, out int infinityCount)) {
				infinityCount = category switch {
					Category.Wiring => config.InfinityRequirement(config.tileWiring, type, LargestStack(category)),
					Category.WandAmmo => config.InfinityRequirement(config.tileBlocks, type, LargestStack(category)),
					_ => throw new System.NotImplementedException()
				};
			}
			int playerTotal = player.CountAllItems(type);
			return playerTotal >= infinityCount;
		}
	}
}
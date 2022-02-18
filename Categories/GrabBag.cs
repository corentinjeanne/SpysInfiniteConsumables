using SPIC.Config;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Categories {
	public static class GrabBag {
		public enum Category {
			NotaBag,
			BossBag,
			GrabBag
		}
		public static Category? CategoryFromConfigCategory(ConsumableConfig.Category category) {
			return category switch {
				ConsumableConfig.Category.Blacklist => Category.NotaBag,
				ConsumableConfig.Category.Bags => Category.GrabBag,
				_ => null
			};
		}
		public static int LargestStack(Category category) {
			return category switch {
				Category.BossBag => 999,
				Category.GrabBag => 99,
				_ => 999,
			};
		}
		public static Category? GetCategory(int type) {
			Item item = new(type);
			return item.GetBagCategory();
		}
		public static Category? GetBagCategory(this Item item) {
			if (item == null) return null;
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
			if (config.HasCustomCategory(item, out ConsumableConfig.Category custom)) {
				Category? category = CategoryFromConfigCategory(custom);
				if (category.HasValue) return category.Value;
			}
			if (ItemID.Sets.BossBag[item.type]) return Category.BossBag;
			if (item.ModItem?.CanRightClick() ?? false) return Category.GrabBag;
			return null;
		}

		public static bool? IsInfiniteBag(this Item item) {
			if (item.playerIndexTheItemIsReservedFor == -1) return false;
			return Main.player[item.playerIndexTheItemIsReservedFor].HasInfiniteBag(item);
		}
		public static bool? HasInfiniteBag(this Player player, Item item) {
			Category? category = item.GetBagCategory();
			return category.HasValue ? player.HasInfinite(item.type, category.Value) : null;
		}

		public static bool HasInfinite(this Player player, int type, Category category) {
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			if (!config.InfiniteConsumables || category == Category.NotaBag) return false;


			if (config.JourneyRequirement)
				return player.CountAllItems(type) >= CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];

			if (!config.HasCustomInfinity(type, out int infinityCount)) {
					infinityCount = category switch {
					Category.BossBag => config.InfinityRequirement(config.bags, type, LargestStack(category)),
					Category.GrabBag => config.InfinityRequirement(config.bags, type, LargestStack(category), 2.5f),
					_ => throw new System.NotImplementedException()
				};
			}
			return player.CountAllItems(type) >= infinityCount;
		}

	}
}
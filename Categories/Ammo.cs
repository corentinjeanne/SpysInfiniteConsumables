using SPIC.Config;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Categories {
	public static class Ammo {
		public enum Category {
			NotAmmo,
			BasicAmmo,
			SpecialAmmo
		}
		public static Category? CategoryFromConfigCategory(ConsumableConfig.Category category) {
			return category switch {
				ConsumableConfig.Category.Blacklist => Category.NotAmmo,
				ConsumableConfig.Category.Ammo => Category.BasicAmmo,
				ConsumableConfig.Category.Special => Category.SpecialAmmo,
				_ => null,
			};
		}
		public static int LargestStack(Category category) {
			return category switch {
				Category.BasicAmmo => 999,
				Category.SpecialAmmo => 999,
				_ => 999,
			};
		}
		public static Category GetCategory(int type) {
			Item item = new(type);
			return item.GetAmmoCategory();
		}
		public static Category GetAmmoCategory(this Item item) {

			if (item == null || !item.consumable) return Category.NotAmmo;

			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
			if(config.HasCustomCategory(item, out ConsumableConfig.Category custom)) {
				Category? category = CategoryFromConfigCategory(custom);
				if (category.HasValue) return category.Value;
			}

			if (item.ammo == AmmoID.None) return Category.NotAmmo;
			if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet)
				return Category.BasicAmmo;
			return Category.SpecialAmmo;
		}

		public static bool IsInfiniteAmmo(this Item item) {
			if (item.playerIndexTheItemIsReservedFor == -1) return false;
			return Main.player[item.playerIndexTheItemIsReservedFor].HasInfiniteAmmo(item);
		}

		public static bool HasInfiniteAmmo(this Player player, Item item) {
			Category category = item.GetAmmoCategory();
			return player.HasInfinite(item.type, category);
		}

		public static bool HasInfinite(this Player player, int type, Category category) {
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			if (!config.InfiniteConsumables || category == Category.NotAmmo) return false;

			if (config.JourneyRequirement)
				return player.CountAllItems(type) >= CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];

			if (!config.HasCustomInfinity(type, out int infinityCount)) {
				infinityCount = category switch {
					Category.BasicAmmo => config.InfinityRequirement(config.ammos, type, LargestStack(category)),
					Category.SpecialAmmo => config.InfinityRequirement(config.specialAmmos, type, LargestStack(category)),
					_ => throw new System.NotImplementedException()
				};
			}
			return player.CountAllItems(type) >= infinityCount;
		}
	}
}
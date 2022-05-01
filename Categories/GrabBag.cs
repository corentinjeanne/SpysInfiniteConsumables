using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
using Terraria.UI;

namespace SPIC {
	namespace Categories {
		public enum GrabBag {
			None,
			Crate,
			TreasureBag,
		}
	}
	public static class GrabBagExtension {
		public static int MaxStack(this GrabBag bag) => bag switch {
			GrabBag.TreasureBag => 999,
			GrabBag.Crate => 99,
			GrabBag.None => 999,
			_ => throw new System.NotImplementedException(),
		};public static int Infinity(this GrabBag bag) {
			Configs.GrabBag b = Configs.ConsumableConfig.Instance.Bags;
			return bag switch {
				GrabBag.Crate => b.Crates,
				GrabBag.TreasureBag => b.TreasureBags,
				GrabBag.None => 0,
				_ => throw new System.NotImplementedException(),
			};
		}
		public static GrabBag? GetGrabBagCategory(this Item item) {
			if (item == null) return null;

            var categories = Configs.ConsumableConfig.Instance.GetCategoriesOverride(item.type);
            if (categories.GrabBag.HasValue) return categories.GrabBag.Value;

			if (ItemID.Sets.BossBag[item.type]) return GrabBag.TreasureBag;
			if (item.ModItem?.CanRightClick() ?? false) return GrabBag.Crate;

			return null;
		}
		
		public static bool HasInfinite(this Player player, int type, GrabBag grabBag) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

			int items;
            var infinities = config.GetInfinitiesOverride(type);
            if (infinities.GrabBag.HasValue)
                items = Utility.InfinityToItems(infinities.GrabBag.Value, type, GrabBag.None.MaxStack());
			else {
				if (config.JourneyRequirement) items = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];
				else {
					if (grabBag == GrabBag.None) return false;
					items = Utility.InfinityToItems(grabBag.Infinity(), type, grabBag.MaxStack());
				}
			}

			return player.CountAllItems(type) >= items;
		}

	}
}
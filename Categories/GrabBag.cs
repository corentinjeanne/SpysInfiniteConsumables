using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
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

			if (Configs.ConsumableConfig.Instance.HasCustom(item.type, out Configs.Custom custom) && custom.GrabBag != null && custom.GrabBag.Category != GrabBag.None)
				return custom.GrabBag.Category;

			if (ItemID.Sets.BossBag[item.type]) return GrabBag.TreasureBag;
			if (item.ModItem?.CanRightClick() ?? false) return GrabBag.Crate;
			return null;
		}
		
		public static bool? HasInfiniteBag(this Player player, Item item) {
			GrabBag? GrabBag = item.GetGrabBagCategory();
			return GrabBag.HasValue ? player.HasInfinite(item.type, GrabBag.Value) : null;
		}

		public static bool HasInfinite(this Player player, int type, GrabBag grabBag) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

			int items;
			if (config.HasCustom(type, out Configs.Custom custom) && custom.GrabBag?.Category == GrabBag.None) {
				items = Utility.InfinityToItems(custom.GrabBag.Infinity, type, GrabBag.None.MaxStack());
			}
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
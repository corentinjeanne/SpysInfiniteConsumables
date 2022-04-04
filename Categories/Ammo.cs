using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {
	namespace Categories {
		public enum Ammo {
			None,
			Basic,
			Special
		}
	}
	public static class AmmoExtension {
		public static int MaxStack(this Ammo ammo) => ammo switch {
			Ammo.Basic => 999,
			Ammo.Special => 999,
			Ammo.None => 999,
			_ => throw new System.NotImplementedException(),
		};
		public static int Infinity(this Ammo ammo) {
			Configs.Ammo a = Configs.ConsumableConfig.Instance.Ammos;
			return ammo switch {
				Ammo.Basic => a.Basic,
				Ammo.Special => a.Special,
				Ammo.None => 0,
				_ => throw new System.NotImplementedException(),
			};
		}
		public static Ammo GetAmmoCategory(this Item item) {

			if (item == null) return Ammo.None;
			
			if(Configs.ConsumableConfig.Instance.HasCustom(item.type, out Configs.Custom custom) && custom.Ammo != null && custom.Ammo.Category != Ammo.None)
				return custom.Ammo.Category;

			if(!item.consumable || item.ammo == AmmoID.None) return Ammo.None;
			if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
				return Ammo.Basic;

			return Ammo.Special;
		}

		public static bool HasInfiniteAmmo(this Player player, Item item) => player.HasInfinite(item.type, item.GetAmmoCategory());
		public static bool HasInfinite(this Player player, int type, Ammo ammo) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

			int items;
			if(config.HasCustom(type, out Configs.Custom custom) && custom.Ammo?.Category == Ammo.None) {
				items = Utility.InfinityToItems(custom.Ammo.Infinity, type, Ammo.None.MaxStack());
			}
			else {
				if (config.JourneyRequirement) items = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];
				else {
					if (ammo == Ammo.None) return false;
					items = Utility.InfinityToItems(ammo.Infinity(), type, ammo.MaxStack());
				}
			}
			return player.CountAllItems(type) >= items;
		}
	}
}
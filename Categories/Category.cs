using Terraria;

namespace SPIC.Categories {

	public struct ItemCategories {
		public readonly Item Item;
		public readonly Consumable? Consumable;
		public readonly Ammo Ammo;
		public readonly WandAmmo? WandAmmo;
		public readonly GrabBag? GrabBag;
		public readonly Material Material;

		public ItemCategories(Item item) {
			Item = item;
			Consumable = item.GetConsumableCategory();
			Ammo = item.GetAmmoCategory();
			WandAmmo = item.GetWandAmmoCategory();
			GrabBag = item.GetGrabBagCategory();
			Material = item.GetMaterialCategory();
		}
	}

	public enum CategoryType {
		Consumable,
		Ammo,
		WandAmmo,
		GrabBag,
		Material
	}

	public static class CategoryHelper {
		public static ItemCategories GetCategory(this Item item) => new ItemCategories(item);
	}
}
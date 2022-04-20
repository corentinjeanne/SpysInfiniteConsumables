using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Creative;

using SPIC.Categories;
using Terraria.ID;

namespace SPIC {
	namespace Categories {
		public enum WandAmmo {
			None,
			Block,
			Mechanical,
			Wiring, // Fore wires
		}
	}
	public static class WandAmmoExtension {

		private static readonly List<int> m_WandAmmoCache = new();
		public static bool IsInCache(int type) => m_WandAmmoCache.Contains(type);
		public static void AddToCache(int type) => m_WandAmmoCache.Add(type);
		public static void ClearCache() => m_WandAmmoCache.Clear();
		public static int MaxStack(this WandAmmo wandAmmo) => wandAmmo switch {
			WandAmmo.None => 999,
			WandAmmo.Block => 999,
			WandAmmo.Wiring => 999,
			WandAmmo.Mechanical => 999,
			_ => throw new System.NotImplementedException(),
		};
		public static int Infinity(this WandAmmo wandAmmo) {
			Configs.ConsumableConfig c = Configs.ConsumableConfig.Instance;
			return wandAmmo switch {
				WandAmmo.None => 0,
				WandAmmo.Block => c.CommonTiles.Blocks,
				WandAmmo.Wiring => c.CommonTiles.Wiring,
				WandAmmo.Mechanical => c.OtherTiles.Mechanical,
				_ => throw new System.NotImplementedException(),
			};
		}
		public static WandAmmo? GetWandAmmoCategory(this Item item) {

			if (item == null) return WandAmmo.None;

			if (Configs.ConsumableConfig.Instance.HasCustom(item.type, out Configs.Custom custom) && custom.WandAmmo != null && custom.WandAmmo.Category != WandAmmo.None)
				return custom.WandAmmo.Category;

			if (m_WandAmmoCache.Contains(item.type)) return WandAmmo.Block;
			if (item.FitsAmmoSlot() && item.mech) return item.useStyle == ItemUseStyleID.None ? WandAmmo.Wiring : WandAmmo.Mechanical;

			return null;
		}
		public static bool HasInfinite(this Player player, int type, WandAmmo wandAmmo) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

			int items;
			if (config.HasCustom(type, out Configs.Custom custom) && custom.WandAmmo?.Category == WandAmmo.None) {
				items = Utility.InfinityToItems(custom.WandAmmo.Infinity, type, WandAmmo.None.MaxStack());
			}
			else {
				if (config.JourneyRequirement) items = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];
				else {
					if (wandAmmo == WandAmmo.None) return false;
					items = Utility.InfinityToItems(wandAmmo.Infinity(), type, wandAmmo.MaxStack());
				}
			}
			return player.CountAllItems(type) >= items;
		}
	}
}
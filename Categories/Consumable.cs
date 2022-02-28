using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using SPIC.Config;
using SPIC.Globals;

using Terraria.ObjectData;
using Terraria.GameContent.Creative;

namespace SPIC.Categories {

	public static class Consumable {

		public static List<int> buckets = new() { ItemID.EmptyBucket, ItemID.WaterBucket, ItemID.LavaBucket, ItemID.HoneyBucket };

		public enum Category : uint {
			NotConsumable = 0,

			// Weapons
			ThrownWeapon,

			// Potions
			Recovery, // Mushroom, leasser potions bottle water, ManaPotion
			Buff, // ales

			// Boosters
			PlayerBooster,
			WorldBooster,

			// NPCs
			Summoner,
			Critter,

			// Other consumables
			Explosives,
			Tool,

			// Basic tiles
			Block,
			Ore,
			Platform,
			Wall,

			// Furnitures
			Torch,
			LightSource,
			Functional,
			CraftingStation,
			Housing,
			Decoration,
			Container,
			MusicBox,

			// Other placeables
			Wiring,
			Bucket,
			Seed
		}
		public static bool IsTileCategory(Category category) => category >= Category.Block;
		public static bool IsCommonBlockCategory(Category category) => Category.Block <= category && category <= Category.Wall;
		public static bool IsFurnitureCategory(Category category) => Category.Torch <= category && category <= Category.MusicBox;

		public static Category? CategoryFromConfigCategory(ConsumableConfig.Category category){
			return category switch {
				ConsumableConfig.Category.Blacklist => Category.NotConsumable,
				ConsumableConfig.Category.Weapon => Category.ThrownWeapon,
				ConsumableConfig.Category.Recovery => Category.Recovery,
				ConsumableConfig.Category.Buff => Category.Buff,
				ConsumableConfig.Category.Booster => Category.WorldBooster,
				ConsumableConfig.Category.Summoner => Category.Summoner,
				ConsumableConfig.Category.Critter => Category.Critter,
				ConsumableConfig.Category.Tools => Category.Tool, // explosive
				ConsumableConfig.Category.Block => Category.Block,
				ConsumableConfig.Category.Furniture => Category.Functional,
				ConsumableConfig.Category.Wall => Category.Wall,
				ConsumableConfig.Category.Liquid => Category.Bucket,
				ConsumableConfig.Category.Wiring => Category.Wiring,
				_ => null,
			};
		}
		public static int LargestStack(Category category) {
			return category switch {
				Category.ThrownWeapon => 999,
				Category.Recovery => 99,
				Category.Buff => 30,
				Category.PlayerBooster => 99,
				Category.WorldBooster => 20,
				Category.Summoner => 20,
				Category.Critter => 99,
				Category.Explosives => 99,
				Category.Tool => 999,
				Category.Block => 999,
				Category.Torch => 999,
				Category.Ore => 999,
				Category.Platform => 999,
				Category.Wall => 999,
				Category.Bucket => 99,
				Category.Seed => 99,
				Category.MusicBox => 1,
				Category.Container => 99,
				Category.LightSource => 99,
				Category.Housing => 99,
				Category.CraftingStation => 99,
				Category.Functional => 99,
				Category.Wiring => 999,
				Category.Decoration => 99,
				_ => 999,
			};
		}
		public static Category? GetCategory(int type) {
			Item item = new(type);
			return item.GetConsumableCategory();
		}
		public static Category? GetConsumableCategory(this Item item) {

			if (item == null) return null;

			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			if (config.HasCustomCategory(item, out ConsumableConfig.Category custom)) {
				Category? category = CategoryFromConfigCategory(custom);
				if (category.HasValue) return category.Value;
			}

			// Vanilla inconsitancies
			switch (item.type) {
			case ItemID.Actuator: return Category.Wiring;
			case ItemID.PirateMap: return Category.Summoner;
			case ItemID.EmpressButterfly: return Category.Summoner;
			}			
				
				
			if (buckets.Contains(item.type)) return Category.Bucket;

			if(!item.consumable || item.useStyle == ItemUseStyleID.None) return Category.NotConsumable;

			if (item.createWall != -1) return Category.Wall;
			if (item.createTile != -1) {

				int tileType = item.createTile;
				if (item.accessory) return Category.MusicBox;
				if (TileID.Sets.Platforms[tileType]) return Category.Platform;

				if (Main.tileAlch[tileType] || TileID.Sets.TreeSapling[tileType] || TileID.Sets.Grass[tileType]) return Category.Seed;
				if (Main.tileContainer[tileType]) return Category.Container;

				if (item.mech) return Category.Wiring;
				if (Main.tileSpelunker[tileType]) return Category.Ore;

				if (Main.tileFrameImportant[tileType]) {

					bool GoodTile(int t) => t == tileType;

					if (TileID.Sets.Torch[tileType]) return Category.Torch;
					if(System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTorch, GoodTile)) return Category.LightSource;

					if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsChair, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTable, GoodTile))
						return Category.Housing;

					if (SpicRecipe.CraftingStations.Contains(tileType)) return Category.CraftingStation;

					if (TileID.Sets.HasOutlines[tileType]) return Category.Functional;

					return Category.Decoration;

				}
				return Category.Block;
			}

			if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
				return Category.Summoner;
			if (item.makeNPC != NPCID.None || item.bait != 0) return Category.Critter;

			if (item.damage > 0) return Category.ThrownWeapon;

			if (item.buffType != 0 && item.buffTime != 0) return Category.Buff;
			if (item.healLife > 0 || item.healMana > 0 || item.potion) return Category.Recovery;

			if (item.shoot != ProjectileID.None) return Category.Tool;


			if (item.hairDye != -1) return Category.PlayerBooster;

			// Most modded summoners, booster and non buff potions, modded liquids...
			return null;
		}

		public static bool? IsInfiniteConsumable(this Item item) {
			if (item.playerIndexTheItemIsReservedFor == -1) return false;
			return Main.player[item.playerIndexTheItemIsReservedFor].HasInfiniteConsumable(item);
		}

		public static bool? HasInfiniteConsumable(this Player player, Item item) {
			Category? category = item.GetConsumableCategory();
			return category.HasValue ? player.HasInfinite(item.type, category.Value) : null;
		}

		public static bool CannotStopDrop(int type) {
			// WallXxX
			// 2x5
			// 3x5
			// 3x6
			// Sunflower
			// Gnome
			// Chest
			// drop in 2x1 bug : num instead of num3
			Item item = new(type);
			// Does no place a tile
			if (item.createTile < TileID.Dirt || item.createWall >= WallID.None || item.createTile == TileID.TallGateClosed) return false;
			if (item.createTile == TileID.GardenGnome || item.createTile == TileID.Sunflower || TileID.Sets.BasicChest[item.createTile]) return true;
			
			TileObjectData data = TileObjectData.GetTileData(item.createTile, item.placeStyle);
			// No data or 1x1 moditem
			if (data == null || (item.ModItem != null && data.Width > 1 && data.Height > 1)) return false;
			if ((data.Width == 2 && data.Height == 1) || (data.Width == 2 && data.Height == 5) || (data.Width == 3 && data.Height == 4) || (data.Width == 3 && data.Height == 5) || (data.Width == 3 && data.Height == 6)) return true;
			return data.AnchorWall || (TileID.Sets.HasOutlines[item.createTile] && System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, t => t == item.createTile));

		}
		public static bool HasInfinite(this Player player, int type, Category category) {
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			if (category == Category.NotConsumable || (IsTileCategory(category) ? !config.InfiniteTiles : !config.InfiniteConsumables)) return false;

			if (config.JourneyRequirement)
				return player.CountAllItems(type) >= CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];

			if (!config.HasCustomInfinity(type, out int infinityCount)) {
				infinityCount = category switch {
					Category.ThrownWeapon => config.InfinityRequirement(config.consumablesThrown, type, LargestStack(category)),

					Category.Recovery => config.InfinityRequirement(config.consumablesRecovery, type, LargestStack(category)),
					Category.Buff => config.InfinityRequirement(config.consumablesBuffPotions, type, LargestStack(category)),
					Category.PlayerBooster => config.InfinityRequirement(config.consumablesBoosters, type, LargestStack(category)),
					Category.WorldBooster => config.InfinityRequirement(config.consumablesBoosters, type, LargestStack(category)),

					Category.Summoner => config.InfinityRequirement(config.consumablesSummoning, type, LargestStack(category)),
					Category.Critter => config.InfinityRequirement(config.consumablesCritters, type, LargestStack(category)),

					Category.Explosives => config.InfinityRequirement(config.consumablesTools, type, LargestStack(category)),
					Category.Tool => config.InfinityRequirement(config.consumablesTools, type, LargestStack(category)),

					Category.Block => config.InfinityRequirement(config.tileBlocks, type, LargestStack(category)),
					Category.Wall => config.InfinityRequirement(config.tileWalls, type, LargestStack(category)),
					Category.Bucket => config.InfinityRequirement(config.tileLiquids, type, LargestStack(category)),
					Category.Torch => config.InfinityRequirement(config.tileLightSources, type, LargestStack(category)), // custom category ?
					Category.Ore => config.InfinityRequirement(config.tileOres, type, LargestStack(category)),
					Category.Platform => config.InfinityRequirement(config.tileBlocks, type, LargestStack(category)),
					Category.Seed => config.InfinityRequirement(config.tileSeeds, type, LargestStack(category)),
					Category.MusicBox => config.InfinityRequirement(config.tilesFurnitures, type, LargestStack(category)), // custom category ?
					Category.Container => config.InfinityRequirement(config.tileContainers, type, LargestStack(category)),
					Category.LightSource => config.InfinityRequirement(config.tileLightSources, type, LargestStack(category)),
					Category.Housing => config.InfinityRequirement(config.tilesFurnitures, type, LargestStack(category)),
					Category.CraftingStation => config.InfinityRequirement(config.tilesFurnitures, type, LargestStack(category)),
					Category.Functional => config.InfinityRequirement(config.tilesFurnitures, type, LargestStack(category)),
					Category.Wiring => config.InfinityRequirement(config.tileWiring, type, LargestStack(category)),
					Category.Decoration => config.InfinityRequirement(config.tilesFurnitures, type, LargestStack(category)),
					_ => throw new System.NotImplementedException()
				};
			}

			if (config.PreventItemDupication && IsTileCategory(category) && (Main.netMode != NetmodeID.SinglePlayer || CannotStopDrop(type)))
				return player.CountAllItems(type) == infinityCount;

			return player.CountAllItems(type) >= infinityCount;
		}
	}
}
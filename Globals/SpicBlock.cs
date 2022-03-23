using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.ModLoader;

using SPIC.Systems;
using SPIC.Config;
using SPIC.Categories;

using System.Collections.Generic;

namespace SPIC.Globals {

	public struct LargeObject {
		//public int type;
		public int X, Y;
		public int W, H;
		public bool IsInside(int x, int y) => X <= x && x < X + W && Y <= y && y < Y + H;

		public override string ToString() => $"({X},{Y}),{W}x{H}";
	}
	public class SPICTile : GlobalTile {

		private readonly List<LargeObject> m_NoDropCache = new();
		private static bool s_InDropItem;

		public override void Load() {
			On.Terraria.WorldGen.KillTile_DropItems += HookKillTile_DropItem;
			On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement += HookReplaceTIle_DoActualReplacement;
		}

		private static void HookKillTile_DropItem(On.Terraria.WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops) {
			s_InDropItem = true;
			orig(x, y, tileCache, includeLargeObjectDrops);
			s_InDropItem = false;
		}
		private static void HookReplaceTIle_DoActualReplacement(On.Terraria.WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t) {
			Player player = Main.player[Main.myPlayer];
			ModContent.GetInstance<SPICTile>().PlaceInWorld(topLeftX, topLeftY, player.HeldItem.createTile, player.HeldItem);

			orig(targetType, targetStyle, topLeftX, topLeftY, t);
		}

		public override void PlaceInWorld(int i, int j, int type, Item item) {

			if (Main.netMode != NetmodeID.SinglePlayer) return;

			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			int playerIndex = item.playerIndexTheItemIsReservedFor;
			if (WorldGen.generatingWorld || playerIndex < 0 || !config.InfiniteTiles || !config.PreventItemDupication)
				return;

			if (Consumable.CannotStopDrop(item.type)) return;

			SpicWorld world = ModContent.GetInstance<SpicWorld>();
			if (Main.player[playerIndex].HeldItem == item) {
				if (item.IsInfiniteConsumable() ?? false) {
					TileObjectData data = TileObjectData.GetTileData(type, item.placeStyle);
					if (data == null) world.PlaceTile(i, j);
					else world.PlaceTile(i-data.Origin.X, j- data.Origin.Y);
				}
				return;
			}
			if (item.IsInfiniteWandAmmo()) world.PlaceTile(i, j);

		}

		public override bool Drop(int i, int j, int type) {
			if (Main.netMode != NetmodeID.SinglePlayer) return true;

			TileObjectData data;
			SpicWorld world = ModContent.GetInstance<SpicWorld>();
			if (s_InDropItem) {
				bool noDrop = world.MineTile(i, j);
				if (noDrop) {
					data = TileObjectData.GetTileData(Main.tile[i, j]);
					if (data != null && (data.Width > 1 || data.Height > 1)) {
						m_NoDropCache.Add(new LargeObject() {
							X = i, Y = j,
							W = data.Width, H = data.Height
						});
					}
				}
				return !noDrop;
			}
			
			for (int k = 0; k < m_NoDropCache.Count; k++) {
				if (m_NoDropCache[k].IsInside(i, j)) {
					m_NoDropCache.RemoveAt(k);
					return false;
				}
			}

			data = TileObjectData.GetTileData(Main.tile[i, j]);
			if (data != null) {
				int top = j - (Main.tile[i, j].TileFrameX % (18 * data.Height)) / 18;
				int left = i - (Main.tile[i, j].TileFrameX % (18 * data.Width)) / 18;
				bool noDrop = world.MineTile(left, top);
				if (noDrop) return false;
			}

			return true;
		}
	}

	
	public class SPICWall : GlobalWall {
	
		public override void PlaceInWorld(int i, int j, int type, Item item) {

			if (Main.netMode != NetmodeID.SinglePlayer) return;

			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
			if (WorldGen.generatingWorld || item.playerIndexTheItemIsReservedFor < 0 || !config.InfiniteTiles || !config.PreventItemDupication)
				return;

			ModContent.GetInstance<SpicWorld>().PlaceWall(i, j);
		}

		public override bool Drop(int i, int j, int type, ref int dropType) {

			if (Main.netMode != NetmodeID.SinglePlayer) return true;

			return !ModContent.GetInstance<SpicWorld>().MineWall(i, j);

		}

	}
}
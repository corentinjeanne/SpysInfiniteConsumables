using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.ModLoader;

using SPIC.Systems;
using SPIC.Config;
using SPIC.Categories;

using System.Collections.Generic;

using System.Diagnostics;

namespace SPIC.Globals {

    public struct LargeObject {
        //public int type;
        public int X, Y;
        public int W, H;
        public bool IsInside(int x, int y) => X <= x && x < X + W && Y <= y && y < Y + H;

        public override string ToString() => $"({X},{Y}),{W}x{H}";
	}
	public class NoTileDup : GlobalTile {
        private readonly List<LargeObject> noDropObjects = new();
        private static bool s_InDropItem;
		public override void Load() {
			On.Terraria.WorldGen.KillTile_DropItems += HookKillTile_DropItem;
        }
		public override void Unload() {
            On.Terraria.WorldGen.KillTile_DropItems -= HookKillTile_DropItem;
        }

		private static void HookKillTile_DropItem(On.Terraria.WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops) {
            s_InDropItem = true;
            orig(x, y, tileCache, includeLargeObjectDrops);
            s_InDropItem = false;
        }


        public override void PlaceInWorld(int i, int j, int type, Item item) {
            SpicWorld world = ModContent.GetInstance<SpicWorld>();

            int playerIndex = item.playerIndexTheItemIsReservedFor;
            if (WorldGen.generatingWorld || playerIndex < 0 || !ModContent.GetInstance<ConsumableConfig>().PreventItemDupication)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
                //return;
            }

            if (Consumable.CannotStopDrop(item.type)) return;

			if (Main.player[playerIndex].HeldItem == item) {
                if (item.IsInfiniteConsumable() ?? false) {
                    TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
                    if (data == null) world.PlaceTile(i, j);
					else world.PlaceTile(i-data.Origin.X, j- data.Origin.Y);
                }
                return;
			}
			if (item.IsInfiniteWandAmmo()) world.PlaceTile(i, j);

        }
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
            if (fail || WorldGen.generatingWorld) return;
            if (!WorldGen.destroyObject) noDropObjects.Clear(); // Clears list when mine a tile, just in case

            SpicWorld world = ModContent.GetInstance<SpicWorld>();
            bool noDrop =  world.MineTile(i, j);
			if (noDrop) {
                TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
                if(data != null && (data.Width > 1 || data.Height > 1)) {
                    noDropObjects.Add(new LargeObject() {
                        X = i, Y = j,
                        W = data.Width, H = data.Height
                    });
					//Mod.Logger.Debug($"added {noDropObjects[^1]} to {nameof(noDropObjects)}");
				}
                noItem = true;
			}
		}
        public override bool Drop(int i, int j, int type) {
            //Mod.Logger.Debug($"Drop called: type={type},i={i},j={j}, destroy={WorldGen.destroyObject}, DropItem={s_InDropItem}");

            if (!WorldGen.destroyObject || s_InDropItem) return true;

            for (int o = 0; o < noDropObjects.Count; o++) {
                if (noDropObjects[o].IsInside(i, j)) {
                    //Mod.Logger.Debug($"removed {noDropObjects[o]} to {nameof(noDropObjects)}");
                    noDropObjects.RemoveAt(o);
                    return false;
                }
			}

			SpicWorld world = ModContent.GetInstance<SpicWorld>();
            TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
            if (data != null) {
                int top = j - (Main.tile[i, j].TileFrameX % (18 * data.Height)) / 18;
                int left = i - (Main.tile[i, j].TileFrameX % (18 * data.Width)) / 18;
                bool noDrop = world.MineTile(left, top);
                if (noDrop) {
                    //Mod.Logger.Debug($"no drop: type={type} at ({i},{j}), corner=({left},{top})");
                    return false;
                }
            }

                
            
            return true; // true
            // WallXxX
            // 2x5
            // 3x5
            // 3x6
            // Sunflower
            // Gnome
            // Chest
            // drop in 2x1 bug : num instead of num3
        }
    }

	public class NoWallDup : GlobalWall {
    

        public override void PlaceInWorld(int i, int j, int type, Item item) {
            SpicWorld world = ModContent.GetInstance<SpicWorld>();
            if (ModContent.GetInstance<ConsumableConfig>().PreventItemDupication){
                world.PlaceWall(i, j);
            }

        }
		public override bool Drop(int i, int j, int type, ref int dropType) {

            SpicWorld world = ModContent.GetInstance<SpicWorld>();
            return !world.MineWall(i, j);

        }

    }
}
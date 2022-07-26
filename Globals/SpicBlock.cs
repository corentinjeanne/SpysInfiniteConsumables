using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.ModLoader;

namespace SPIC.Globals {

    public struct LargeObject {
        //public int type;
        public int X, Y;
        public int W, H;
        public bool IsInside(int x, int y) => X <= x && x < X + W && Y <= y && y < Y + H;
    }
    public class SPICTile : GlobalTile {

        private readonly List<LargeObject> _noDropCache = new();
        private static bool s_inDropItem;

        public override void Load() {
            On.Terraria.WorldGen.KillTile_DropItems += HookKillTile_DropItem;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement += HookReplaceTIle_DoActualReplacement;
        }

        private static void HookKillTile_DropItem(On.Terraria.WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops) {
            s_inDropItem = true;
            orig(x, y, tileCache, includeLargeObjectDrops);
            s_inDropItem = false;
        }

        private static void HookReplaceTIle_DoActualReplacement(On.Terraria.WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t) {
            Player player = Main.LocalPlayer;
            ModContent.GetInstance<SPICTile>().PlaceInWorld(topLeftX, topLeftY, player.HeldItem.createTile, player.HeldItem);
            orig(targetType, targetStyle, topLeftX, topLeftY, t);
        }


        //  TODO Falling tiles
        public override void PlaceInWorld(int i, int j, int type, Item item) {
            Configs.Requirements infs = Configs.Requirements.Instance;

            int playerIndex = item.playerIndexTheItemIsReservedFor;
            if (WorldGen.generatingWorld || playerIndex < 0 || !infs.InfinitePlaceables || !infs.PreventItemDupication)
                return;

            if (!PlaceableExtension.CanNoDuplicationWork(item)) return;

            Systems.SpicWorld world = ModContent.GetInstance<Systems.SpicWorld>();
            InfinityPlayer spicPlayer = Main.player[playerIndex].GetModPlayer<InfinityPlayer>();

            if (1 <= spicPlayer.GetTypeInfinities(item.tileWand == -1 ? item.type : item.tileWand).Placeable) {
                    TileObjectData data = TileObjectData.GetTileData(type, item.placeStyle);
                    if (data == null) world.PlaceBlock(i, j);
                    else world.PlaceBlock(i-data.Origin.X, j- data.Origin.Y);
                }
            }

        public override bool Drop(int i, int j, int type) {
            if (!PlaceableExtension.CanNoDuplicationWork()) return true;

            TileObjectData data;
            Systems.SpicWorld world = ModContent.GetInstance<Systems.SpicWorld>();
            if (s_inDropItem) {
                if (world.MineBlock(i, j)) {
                    data = TileObjectData.GetTileData(Main.tile[i, j]);
                    if (data != null && (data.Width > 1 || data.Height > 1)) {
                        _noDropCache.Add(new LargeObject() {
                            X = i, Y = j,
                            W = data.Width, H = data.Height
                        });
                    }
                    return false;
                }
                return true;
            }
            
            for (int k = 0; k < _noDropCache.Count; k++) {
                if (_noDropCache[k].IsInside(i, j)) {
                    _noDropCache.RemoveAt(k);
                    return false;
                }
            }

            data = TileObjectData.GetTileData(Main.tile[i, j]);
            if (data != null) {
                int top = j - Main.tile[i, j].TileFrameX % (18 * data.Height) / 18;
                int left = i - Main.tile[i, j].TileFrameX % (18 * data.Width) / 18;
                bool noDrop = world.MineBlock(left, top);
                if (noDrop) return false;
            }

            return true;
        }
    }

    
    public class SPICWall : GlobalWall {
    
        public override void PlaceInWorld(int i, int j, int type, Item item) {

            Configs.Requirements config = Configs.Requirements.Instance;
            if (WorldGen.generatingWorld || item.playerIndexTheItemIsReservedFor < 0 || !config.InfinitePlaceables || !config.PreventItemDupication)
                return;

            if(!PlaceableExtension.CanNoDuplicationWork(item)) return;

            ModContent.GetInstance<Systems.SpicWorld>().PlaceWall(i, j);
        }

        public override bool Drop(int i, int j, int type, ref int dropType) {

            if (!PlaceableExtension.CanNoDuplicationWork()) return true;

            return !ModContent.GetInstance<Systems.SpicWorld>().MineWall(i, j);

        }

    }
}
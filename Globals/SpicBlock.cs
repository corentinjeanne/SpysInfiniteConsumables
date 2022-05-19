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
            Player player = Main.player[Main.myPlayer];
            ModContent.GetInstance<SPICTile>().PlaceInWorld(topLeftX, topLeftY, player.HeldItem.createTile, player.HeldItem);

            orig(targetType, targetStyle, topLeftX, topLeftY, t);
        }

        public override void PlaceInWorld(int i, int j, int type, Item item) {

            if (Main.netMode != NetmodeID.SinglePlayer) return;

            Configs.Infinities config = Configs.Infinities.Instance;

            int playerIndex = item.playerIndexTheItemIsReservedFor;
            if (WorldGen.generatingWorld || playerIndex < 0 || !config.InfiniteTiles || !config.PreventItemDupication)
                return;

            if (item.CannotStopDrop()) return;

            Systems.SpicWorld world = ModContent.GetInstance<Systems.SpicWorld>();
            SpicPlayer spicPlayer = Main.player[playerIndex].GetModPlayer<SpicPlayer>();
            
            if (spicPlayer.Player.HeldItem == item) {
                if (spicPlayer.HasInfiniteConsumable(item.type)) {
                    TileObjectData data = TileObjectData.GetTileData(type, item.placeStyle);
                    if (data == null) world.PlaceBlock(i, j);
                    else world.PlaceBlock(i-data.Origin.X, j- data.Origin.Y);
                }
            }
            else if (spicPlayer.HasInfiniteWandAmmo(item)) world.PlaceBlock(i, j);
        }

        public override bool Drop(int i, int j, int type) {
            if (Main.netMode != NetmodeID.SinglePlayer) return true;

            TileObjectData data;
            Systems.SpicWorld world = ModContent.GetInstance<Systems.SpicWorld>();
            if (s_inDropItem) {
                bool noDrop = world.MineBlock(i, j);
                if (noDrop) {
                    data = TileObjectData.GetTileData(Main.tile[i, j]);
                    if (data != null && (data.Width > 1 || data.Height > 1)) {
                        _noDropCache.Add(new LargeObject() {
                            X = i, Y = j,
                            W = data.Width, H = data.Height
                        });
                    }
                }
                return !noDrop;
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

            if (Main.netMode != NetmodeID.SinglePlayer) return;

            Configs.Infinities config = Configs.Infinities.Instance;
            if (WorldGen.generatingWorld || item.playerIndexTheItemIsReservedFor < 0 || !config.InfiniteTiles || !config.PreventItemDupication)
                return;

            ModContent.GetInstance<Systems.SpicWorld>().PlaceWall(i, j);
        }

        public override bool Drop(int i, int j, int type, ref int dropType) {

            if (Main.netMode != NetmodeID.SinglePlayer) return true;

            return !ModContent.GetInstance<Systems.SpicWorld>().MineWall(i, j);

        }

    }
}
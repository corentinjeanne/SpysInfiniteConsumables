using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using SPIC.Default.Globals;
using Terraria.ObjectData;

namespace SPIC.Globals;

// TODO multipler
public class SPICTile : GlobalTile {

    public static (int topLeftX, int topLeftY) GetTopLeft(int centerX, int centerY, int type, int style) {
        TileObjectData? data = TileObjectData.GetTileData(type, style);
        return data is null ? (centerX, centerY) : (centerX - data.Origin.X, centerY - data.Origin.Y);
    }

    public static (int topLeftX, int topLeftY) GetTopLeft(int tileX, int tileY) {
        TileObjectData? data = TileObjectData.GetTileData(Main.tile[tileX, tileY]);
        return data is null ? (tileX, tileY) : (tileX - Main.tile[tileX, tileY].TileFrameX % (18 * data.Width) / 18, tileY - Main.tile[tileX, tileY].TileFrameY % (18 * data.Height) / 18);
    }

    public override void Load() {
        On_WorldGen.ReplaceTIle_DoActualReplacement += HookReplaceTIle_DoActualReplacement;
    }

    public override void PlaceInWorld(int i, int j, int type, Item item) {
        if (!Placeable.Instance.Config.preventItemDuplication) return;

        if (Main.LocalPlayer.HasInfinite(Placeable.GetAmmo(Main.LocalPlayer, item) ?? item, 1, Placeable.Instance)) {
            (i, j) = GetTopLeft(i, j, type, item.placeStyle);
            InfiniteWorld.Instance.SetInfinite(i, j, TileType.Block);
        }
    }

    private static void HookReplaceTIle_DoActualReplacement(On_WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t) {
        orig(targetType, targetStyle, topLeftX, topLeftY, t);
        if (Placeable.Instance.Config.preventItemDuplication && Main.LocalPlayer.HasInfinite(Placeable.GetAmmo(Main.LocalPlayer, Main.LocalPlayer.HeldItem) ?? Main.LocalPlayer.HeldItem, 1, Placeable.Instance)) {
            InfiniteWorld.Instance.SetInfinite(topLeftX, topLeftY, TileType.Block);
        }
    }

    public override bool CanDrop(int i, int j, int type) {
        if (!Placeable.Instance.Config.preventItemDuplication) return true;
        var world = InfiniteWorld.Instance;
        (i, j) = GetTopLeft(i, j);
        var res = !world.IsInfinite(i, j, TileType.Block);
        world.ClearInfinite(i, j, TileType.Block);
        return res;
    }
}

public class SPICWall : GlobalWall {
    public override void Load() {
        On_WorldGen.ReplaceWall += HookReplaceWall;
    }

    public override void PlaceInWorld(int i, int j, int type, Item item) {
        if (!Placeable.Instance.Config.preventItemDuplication) return;
        if (Main.LocalPlayer.HasInfinite(item, 1, Placeable.Instance)) InfiniteWorld.Instance.SetInfinite(i, j, TileType.Wall);
    }

    private static bool HookReplaceWall(On_WorldGen.orig_ReplaceWall orig, int x, int y, ushort targetWall) {
        bool res = orig(x, y, targetWall);
        if (!Placeable.Instance.Config.preventItemDuplication) return res;
        if (Main.LocalPlayer.HasInfinite(Main.LocalPlayer.HeldItem, 1, Placeable.Instance)) InfiniteWorld.Instance.SetInfinite(x, y, TileType.Wall);
        return res;
    }

    public override bool Drop(int i, int j, int type, ref int dropType) {
        if (!Placeable.Instance.Config.preventItemDuplication) return true;
        var world = InfiniteWorld.Instance;
        var res = !world.IsInfinite(i, j, TileType.Wall);
        world.ClearInfinite(i, j, TileType.Wall);
        return res;
    }
}

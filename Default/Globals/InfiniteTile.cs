using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using SPIC.Default.Globals;
using Terraria.ObjectData;
using Terraria.ID;
using System.Linq;
using System;

namespace SPIC.Globals;

// TODO multiplayer
public class InfiniteTile : GlobalTile {

    public override void Load() {
        On_WorldGen.PlaceTile += HookPlaceTile;
        On_TileObject.Place += HookPlaceObject;
        On_WorldGen.ReplaceTIle_DoActualReplacement += HookReplaceTIle_DoActualReplacement;
        On_WorldGen.KillTile_DropItems += HookDropItems;
        On_WorldGen.KillTile_GetItemDrops += HookNoSecondaryDrop;
        On_WorldGen.SpawnFallingBlockProjectile += HookFallingSand;
        On_WorldGen.KillTile += HookKillTile;
        MonoModHooks.Add(typeof(TileLoader).GetMethod(nameof(TileLoader.Drop)), HookDrop);
        MonoModHooks.Add(typeof(TileLoader).GetMethod(nameof(TileLoader.GetItemDrops)), HookNoSecondaryDropModded);
        MonoModHooks.Add(typeof(PlantLoader).GetMethod(nameof(PlantLoader.ShakeTree)), HookShakeTree);
    }

    public static readonly int[] HerbsID = [TileID.MatureHerbs, TileID.BloomingHerbs];
    public static bool IsATree(int type) => type switch {
        TileID.Trees or TileID.PalmTree or TileID.VanityTreeSakura or TileID.VanityTreeYellowWillow or TileID.TreeAsh => true,
        >= TileID.TreeAmethyst and <= TileID.TreeAmber => true,
        _ => false
    };

    public static (int topLeftX, int topLeftY) GetUsedTile(TileObject tileObject) => (tileObject.xCoord, tileObject.yCoord + TileObjectData.GetTileData(tileObject.type, tileObject.style, tileObject.alternate).Height-1);
    public static (int topLeftX, int topLeftY) GetUsedTile(int centerX, int centerY, int type, int style) => GetUsedTile(centerX, centerY, TileObjectData.GetTileData(type, style));
    public static (int topLeftX, int topLeftY) GetUsedTile(int tileX, int tileY) => GetUsedTile(tileX, tileY, TileObjectData.GetTileData(Main.tile[tileX, tileY]));
    public static (int topLeftX, int topLeftY) GetUsedTile(int x, int y, TileObjectData? data) { // Bottom left
        if (IsATree(Main.tile[x, y].TileType)) {
            WorldGen.GetTreeBottom(x, y, out int botX, out int botY);
            return (botX, botY-1);
        }
        if (data is null) return (x, y);
        return (x - Main.tile[x, y].TileFrameX % (18 * data.Width) / 18, y - Main.tile[x, y].TileFrameY % (18 * data.Height) / 18 + data.Height-1);
    }

    internal static void PlaceInfinite(int usedI, int usedJ, TileType type) {
        var world = InfiniteWorld.Instance;
        if (world.IsInfinitePlacementContext()) world.SetInfinite(usedI, usedJ, type);
    }
    private static bool HookPlaceTile(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style) {
        if (!orig(i, j, Type, mute, forced, plr, style)) return false;
        if (!Placeable.PreventItemDuplication) return true;
        (i, j) = GetUsedTile(i, j, Type, style);
        PlaceInfinite(i, j, TileType.Block);
        return true;
    }
    private static bool HookPlaceObject(On_TileObject.orig_Place orig, TileObject toBePlaced) {
        if (!orig(toBePlaced)) return false;
        if (!Placeable.PreventItemDuplication) return true;
        (int i, int j) = GetUsedTile(toBePlaced);
        PlaceInfinite(i, j, TileType.Block);
        return true;
    }
    private static void HookReplaceTIle_DoActualReplacement(On_WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t) {
        (int i, int j) = GetUsedTile(topLeftX, topLeftY, targetType, targetStyle);
        InfiniteWorld.Instance.ClearInfinite(i, j, TileType.Block);
        orig(targetType, targetStyle, topLeftX, topLeftY, t);
        if (Placeable.PreventItemDuplication) PlaceInfinite(i, j, TileType.Block);
    }

    private static void HookDropItems(On_WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops, bool includeAllModdedLargeObjectDrops) {
        (int i, int j) = GetUsedTile(x, y);
        if (Placeable.PreventItemDuplication && InfiniteWorld.Instance.IsInfinite(i, j, TileType.Block)) _dropOnlyItemMiscDrop = true;
        orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
        _dropOnlyItemMiscDrop = false;
    }
    public delegate bool DropFn(int i, int j, int type, bool includeLargeObjectDrops = true);
    private static bool HookDrop(DropFn orig, int i, int j, int type, bool includeLargeObjectDrops = true) {
        if (!Placeable.PreventItemDuplication) return orig(i, j, type, includeLargeObjectDrops);

        (int x, int y) = GetUsedTile(i, j);
        bool canDrop = !InfiniteWorld.Instance.IsInfinite(x, y, TileType.Block);
        return Placeable.Instance.Config.preventItemDuplication.Value.allowMiscDrops ?
            (orig(i, j, type, includeLargeObjectDrops) && (_dropOnlyItemMiscDrop || canDrop)) :
            (canDrop && orig(i, j, type, includeLargeObjectDrops));
    }

    private static void HookNoSecondaryDrop(On_WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops) {
        if (!_dropOnlyItemMiscDrop) {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            return;
        }
        secondaryItem = ItemID.None; secondaryItemStack = 0;
        if (IsATree(tileCache.TileType) || HerbsID.Contains(tileCache.TileType)) orig(x, y, tileCache, out dropItem, out dropItemStack, out _, out _, includeLargeObjectDrops);
        else (dropItem, dropItemStack) = (ItemID.None, 0);
    }
    public delegate void GetItemDropsFn(int x, int y, Tile tileCache, bool includeLargeObjectDrops = false, bool includeAllModdedLargeObjectDrops = false);
    public static void HookNoSecondaryDropModded(GetItemDropsFn orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops = false, bool includeAllModdedLargeObjectDrops = false) {
        if (!_dropOnlyItemMiscDrop) orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
    }
    public delegate bool ShakeTreeFn(int x, int y, int type, ref bool createLeaves);
    public static bool HookShakeTree(ShakeTreeFn orig, int x, int y, int type, ref bool createLeaves)
        => !(Placeable.PreventItemDuplication && Placeable.Instance.Config.preventItemDuplication.Value.allowMiscDrops && InfiniteWorld.Instance.IsInfinite(x, y - 1, TileType.Block)) && orig(x, y, type, ref createLeaves);

    private void HookKillTile(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem) {
        orig(i, j, fail, effectOnly, noItem);
        if (!Main.tile[i, j].HasTile) InfiniteWorld.Instance.ClearInfinite(i, j, TileType.Block);
    }
    private bool HookFallingSand(On_WorldGen.orig_SpawnFallingBlockProjectile orig, int i, int j, Tile tileCache, Tile tileTopCache, Tile tileBottomCache, int type) {
        var world = InfiniteWorld.Instance;
        if (world.contextPlayer is not null && Main.myPlayer == world.contextPlayer.whoAmI && Player.tileTargetX == i && Player.tileTargetY == j) PlaceInfinite(i, j, TileType.Block);
        
        bool res = orig(i, j, tileCache, tileTopCache, tileBottomCache, type);
        if (res) world.ClearInfinite(i, j, TileType.Block);
        return res;
    }
    private static bool _dropOnlyItemMiscDrop;
}

public class InfiniteWall : GlobalWall {
    public override void Load() {
        On_WorldGen.PlaceWall += HookPlaceWall;
        On_WorldGen.ReplaceWall += HookReplaceWall;
        On_WorldGen.KillWall += HookKillWall;
    }

    private static void HookPlaceWall(On_WorldGen.orig_PlaceWall orig, int i, int j, int type, bool mute) {
        orig(i, j, type, mute);
        if (Main.tile[i, j].WallType == type && Placeable.PreventItemDuplication) InfiniteTile.PlaceInfinite(i, j, TileType.Wall);
    }
    private static bool HookReplaceWall(On_WorldGen.orig_ReplaceWall orig, int x, int y, ushort targetWall) {
        bool res = orig(x, y, targetWall);
        InfiniteWorld.Instance.ClearInfinite(x, y, TileType.Wall);
        if (!res) return false;
        if (Placeable.PreventItemDuplication) InfiniteTile.PlaceInfinite(x, y, TileType.Wall);
        return true;
    }

    public override bool Drop(int i, int j, int type, ref int dropType) => !Placeable.PreventItemDuplication || !InfiniteWorld.Instance.IsInfinite(i, j, TileType.Wall);

    private void HookKillWall(On_WorldGen.orig_KillWall orig, int i, int j, bool fail) {
        orig(i, j, fail);
        if (Main.tile[i, j].WallType == WallID.None) InfiniteWorld.Instance.ClearInfinite(i, j, TileType.Wall);
    }
}

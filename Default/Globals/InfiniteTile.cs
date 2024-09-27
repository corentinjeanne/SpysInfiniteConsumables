using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using SPIC.Default.Globals;
using Terraria.ObjectData;
using Terraria.ID;
using System.Linq;

namespace SPIC.Globals;

// TODO multiplayer
public class SPICTile : GlobalTile {

    public override void Load() {
        On_WorldGen.ReplaceTIle_DoActualReplacement += HookReplaceTIle_DoActualReplacement;
        On_WorldGen.KillTile_DropItems += HookDropItems;
        On_WorldGen.KillTile_GetItemDrops += HookNoSecondaryDrop;
        On_WorldGen.KillTile += HookKillTile;
        MonoModHooks.Add(typeof(TileLoader).GetMethod(nameof(TileLoader.GetItemDrops)), HookNoSecondaryDropModded);
        MonoModHooks.Add(typeof(PlantLoader).GetMethod(nameof(PlantLoader.ShakeTree)), HookShakeTree);
    }

    public static bool IsATree(int type) => type switch {
        TileID.Trees or TileID.PalmTree or TileID.VanityTreeSakura or TileID.VanityTreeYellowWillow or TileID.TreeAsh => true,
        >= TileID.TreeAmethyst and <= TileID.TreeAmber => true,
        _ => false
    };

    private static readonly int[] HerbsID = [TileID.MatureHerbs, TileID.BloomingHerbs];
    private static bool _noSecondaryDrops;

    public static (int topLeftX, int topLeftY) GetUsedTile(int centerX, int centerY, int type, int style) => GetUsedTile(centerX, centerY, TileObjectData.GetTileData(type, style));
    public static (int topLeftX, int topLeftY) GetUsedTile(int tileX, int tileY) => GetUsedTile(tileX, tileY, TileObjectData.GetTileData(Main.tile[tileX, tileY]));
    public static (int topLeftX, int topLeftY) GetUsedTile(int x, int y, TileObjectData? data) {
        if (IsATree(Main.tile[x, y].TileType)) {
            WorldGen.GetTreeBottom(x, y, out int botX, out int botY);
            return (botX, botY-2);
        }
        if (data is null) return (x, y);
        return (x - Main.tile[x, y].TileFrameX % (18 * data.Width) / 18, y - Main.tile[x, y].TileFrameY % (18 * data.Height) / 18);
    }

    public override void PlaceInWorld(int i, int j, int type, Item item) {
        if (!Placeable.PreventItemDuplication) return;
        if (Main.LocalPlayer.HasInfinite(Placeable.GetAmmo(Main.LocalPlayer, item) ?? item, 1, Placeable.Instance)) {
            (i, j) = GetUsedTile(i, j, type, item.placeStyle);
            InfiniteWorld.Instance.SetInfinite(i, j, TileType.Block);
        }
    }

    private static void HookReplaceTIle_DoActualReplacement(On_WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t) {
        orig(targetType, targetStyle, topLeftX, topLeftY, t);
        if (Placeable.Instance.Config.preventItemDuplication && Main.LocalPlayer.HasInfinite(Placeable.GetAmmo(Main.LocalPlayer, Main.LocalPlayer.HeldItem) ?? Main.LocalPlayer.HeldItem, 1, Placeable.Instance)) {
            InfiniteWorld.Instance.SetInfinite(topLeftX, topLeftY, TileType.Block);
        }
    }

    private void HookDropItems(On_WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops, bool includeAllModdedLargeObjectDrops) {
        (int i, int j) = GetUsedTile(x, y);
        if (Placeable.PreventItemDuplication && Placeable.Instance.Config.preventItemDuplication.Value.allowMisc
            && (IsATree(tileCache.TileType) || HerbsID.Contains(tileCache.TileType)) && InfiniteWorld.Instance.IsInfinite(i, j, TileType.Block)) _noSecondaryDrops = true;
        orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
        _noSecondaryDrops = false;
    }
    
    public override bool CanDrop(int i, int j, int type) {
        (i, j) = GetUsedTile(i, j);
        return !Placeable.PreventItemDuplication || _noSecondaryDrops || !InfiniteWorld.Instance.IsInfinite(i, j, TileType.Block);
    }

    private static void HookNoSecondaryDrop(On_WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops) {
        orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
        if (_noSecondaryDrops) secondaryItem = ItemID.None;
    }
    public delegate void GetItemDropsFn(int x, int y, Tile tileCache, bool includeLargeObjectDrops = false, bool includeAllModdedLargeObjectDrops = false);
    public static void HookNoSecondaryDropModded(GetItemDropsFn orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops = false, bool includeAllModdedLargeObjectDrops = false) {
        if (!_noSecondaryDrops) orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
    }
    public delegate bool ShakeTreeFn(int x, int y, int type, ref bool createLeaves);
    public static bool HookShakeTree(ShakeTreeFn orig, int x, int y, int type, ref bool createLeaves)
        => (_noSecondaryDrops || !InfiniteWorld.Instance.IsInfinite(x, y - 2, TileType.Block)) && orig(x, y, type, ref createLeaves);

    private void HookKillTile(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem) {
        orig(i, j, fail, effectOnly, noItem);
        if (!Main.tile[i, j].HasTile) InfiniteWorld.Instance.ClearInfinite(i, j, TileType.Block);
    }
}

public class SPICWall : GlobalWall {
    public override void Load() {
        On_WorldGen.ReplaceWall += HookReplaceWall;
        On_WorldGen.KillWall += HookKillWall;
    }

    public override void PlaceInWorld(int i, int j, int type, Item item) {
        if (!Placeable.PreventItemDuplication) return;
        if (Main.LocalPlayer.HasInfinite(item, 1, Placeable.Instance)) InfiniteWorld.Instance.SetInfinite(i, j, TileType.Wall);
    }

    private static bool HookReplaceWall(On_WorldGen.orig_ReplaceWall orig, int x, int y, ushort targetWall) {
        bool res = orig(x, y, targetWall);
        if (!Placeable.PreventItemDuplication) return res;
        if (Main.LocalPlayer.HasInfinite(Main.LocalPlayer.HeldItem, 1, Placeable.Instance)) InfiniteWorld.Instance.SetInfinite(x, y, TileType.Wall);
        return res;
    }

    public override bool Drop(int i, int j, int type, ref int dropType) {
        if (!Placeable.PreventItemDuplication) return true;
        var world = InfiniteWorld.Instance;
        return !world.IsInfinite(i, j, TileType.Wall);
    }

    private void HookKillWall(On_WorldGen.orig_KillWall orig, int i, int j, bool fail) {
        orig(i, j, fail);
        if (Main.tile[i, j].WallType == WallID.None) InfiniteWorld.Instance.ClearInfinite(i, j, TileType.Wall);
    }
}

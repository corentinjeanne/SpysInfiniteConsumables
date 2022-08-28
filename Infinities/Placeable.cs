using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SPIC.Infinities;

public enum PlaceableCategory : byte {
    None = Infinity.NoCategory,

    Block,
    Wall,
    Wiring,
    Torch,
    Ore,
    Gem,

    LightSource,
    Container,
    Functional,
    CraftingStation,
    Decoration,
    MusicBox,

    Mechanical,
    Liquid,
    Seed,
    Paint
}

public static class PlaceableExtension {
    public static bool IsCommonTile(this PlaceableCategory category) => category != PlaceableCategory.None && category < PlaceableCategory.LightSource;
    public static bool IsFurniture(this PlaceableCategory category) => category != PlaceableCategory.None && !category.IsCommonTile() && category < PlaceableCategory.Mechanical;
    public static bool IsMisc(this PlaceableCategory category) => category != PlaceableCategory.None && !category.IsCommonTile() && !category.IsFurniture();
}

public class Placeable : Infinity<Placeable> {


    public override int MaxStack(byte category) => (PlaceableCategory)category switch {
        PlaceableCategory.Block => 999,
        PlaceableCategory.Wall => 999,
        PlaceableCategory.Torch => 999,
        PlaceableCategory.Ore => 999,
        PlaceableCategory.Gem => 999,
        PlaceableCategory.Wiring => 999,

        PlaceableCategory.LightSource => 99,
        PlaceableCategory.Container => 99,
        PlaceableCategory.CraftingStation => 99,
        PlaceableCategory.Functional => 99,
        PlaceableCategory.Decoration => 99,
        PlaceableCategory.MusicBox => 1,

        PlaceableCategory.Mechanical => 999,
        PlaceableCategory.Liquid => 99,
        PlaceableCategory.Seed => 99,

        PlaceableCategory.Paint => 999,

        PlaceableCategory.None or _ => 999,
    };

    public override int Requirement(byte category) {
        Configs.Requirements infs = Configs.Requirements.Instance;
        return (PlaceableCategory)category switch {
            PlaceableCategory.Block or PlaceableCategory.Wall or PlaceableCategory.Wiring => infs.placeables_Tiles,
            PlaceableCategory.Torch => infs.placeables_Torches,
            PlaceableCategory.Ore => infs.placeables_Ores,

            PlaceableCategory.LightSource or PlaceableCategory.MusicBox
                    or PlaceableCategory.Functional or PlaceableCategory.Decoration
                    or PlaceableCategory.Container or PlaceableCategory.CraftingStation
                => infs.placeables_Furnitures,

            PlaceableCategory.Liquid => infs.placeables_Liquids,
            PlaceableCategory.Mechanical => infs.placeables_Mechanical,
            PlaceableCategory.Seed => infs.placeables_Seeds,
            PlaceableCategory.Paint => infs.placeables_Paints,
            PlaceableCategory.None or _ => Infinity.NoRequirement,
        };
    }

    public override bool Enabled => Configs.Requirements.Instance.InfinitePlaceables;

    public override byte GetCategory(Item item) {

        Configs.DetectedCategories autos = Configs.CategoryDetection.Instance.GetDetectedCategories(item.type);
        if (autos.Placeable.HasValue) return (byte)autos.Placeable.Value;

        if (!(item.consumable && item.useStyle != ItemUseStyleID.None) && item.paint == 0 && !s_Ammos.ContainsKey(item.type))
            return (byte)PlaceableCategory.None;

        return GetCategory_NoCheck(item);

    }

    private static byte GetCategory_NoCheck(Item item) {

        if (item.paint != 0) return (byte)PlaceableCategory.Paint;

        switch (item.type) {
        case ItemID.Hellstone: return (byte)PlaceableCategory.Ore;
        }

        if (item.createTile != -1) {

            int tileType = item.createTile;
            if (item.accessory) return (byte)PlaceableCategory.MusicBox;
            if (TileID.Sets.Platforms[tileType]) return (byte)PlaceableCategory.Block;

            if (Main.tileAlch[tileType] || TileID.Sets.TreeSapling[tileType] || TileID.Sets.Grass[tileType]) return (byte)PlaceableCategory.Seed;
            if (Main.tileContainer[tileType]) return (byte)PlaceableCategory.Container;

            if (item.mech) return (byte)PlaceableCategory.Mechanical;

            if (Main.tileFrameImportant[tileType]) {
                bool GoodTile(int t) => t == tileType;

                if (TileID.Sets.Torch[tileType]) return (byte)PlaceableCategory.Torch;
                if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTorch, GoodTile)) return (byte)PlaceableCategory.LightSource;

                if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsChair, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTable, GoodTile))
                    return (byte)PlaceableCategory.Functional;

                if (Systems.InfiniteRecipe.CraftingStations.Contains(tileType)) return (byte)PlaceableCategory.CraftingStation;

                if (TileID.Sets.HasOutlines[tileType]) return (byte)PlaceableCategory.Functional;

                return (byte)PlaceableCategory.Decoration;
            }

            if (Main.tileSpelunker[tileType]) return (byte)PlaceableCategory.Ore;

            return (byte)PlaceableCategory.Block;
        }
        if (item.createWall != -1) return (byte)PlaceableCategory.Wall;

        if(item.FitsAmmoSlot() && item.mech) return (byte)PlaceableCategory.Wiring;
        if (s_Ammos.TryGetValue(item.type, out byte category)) return category;

        return (byte)PlaceableCategory.None;

    }

    public override Microsoft.Xna.Framework.Color Color => Configs.InfinityDisplay.Instance.color_Placeables;
    public override TooltipLine TooltipLine => AddedLine("Placeable", Lang.tip[33].Value);
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.Placeable.{(PlaceableCategory)category}";

    // TODO >>> Paint tools and grand desing
    public override bool ConsumesAmmo(Item item) => item.tileWand != -1;
    public override Item GetAmmo(Player player, Item weapon) => System.Array.Find(player.inventory, item => item.type == weapon.tileWand);
    public override TooltipLine AmmoLine(Item ammo) => AddedLine("WandConsumes", null);

    private static readonly Dictionary<int, byte> s_Ammos = new(); // type, category (ammo)
    internal static void ClearWandAmmos() => s_Ammos.Clear();
    public static void RegisterWand(Item wand) => s_Ammos.TryAdd(wand.tileWand, GetCategory_NoCheck(wand));




    // public static bool CanNoDuplicationWork(Item item = null) => Main.netMode == NetmodeID.SinglePlayer && (item == null || !AlwaysDrop(item));

    // //- TODO Update as tml updates
    // // Wires and actuators
    // // Wall XxX
    // // 2x5, 3x5, 3x6
    // // Sunflower, Gnome
    // // Chest
    // // drop in 2x1 bug : num instead of num3
    // public static bool AlwaysDrop(Item item) {
    //     if (item.type == ItemID.Wire || item.type == ItemID.Actuator) return true;
    //     if ((item.createTile < TileID.Dirt && item.createWall != WallID.None) || item.createTile == TileID.TallGateClosed) return false;
    //     if (item.createTile == TileID.GardenGnome || item.createTile == TileID.Sunflower || TileID.Sets.BasicChest[item.createTile]) return true;

    //     TileObjectData data = TileObjectData.GetTileData(item.createTile, item.placeStyle);

    //     // No data or 1x1 moditem
    //     if (data == null || (item.ModItem != null && data.Width > 1 && data.Height > 1)) return false;
    //     if ((data.Width == 2 && data.Height == 1) || (data.Width == 2 && data.Height == 5) || (data.Width == 3 && data.Height == 4) || (data.Width == 3 && data.Height == 5) || (data.Width == 3 && data.Height == 6)) return true;

    //     return data.AnchorWall || (TileID.Sets.HasOutlines[item.createTile] && System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, t => t == item.createTile));
    // }
}

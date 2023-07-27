using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

using SPIC.Configs;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace SPIC.Infinities;

public enum PlaceableCategory {
    None,

    Block,
    Wall,
    Wiring,
    Torch,
    Ore,

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

public class PlaceableRequirements {
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Tiles")]
    public Count Tiles = 999;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Ores")]
    public Count Ores = 499;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Torches")]
    public Count Torches = 99;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Furnitures")]
    public Count Furnitures = 3;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Mechanical")]
    public Count Mechanical = 3;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Liquids")]
    public Count Liquids = 10;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Seeds")]
    public Count Seeds = 20;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Paints")]
    public Count Paints = 999;
}

public class Placeable : InfinityStatic<Placeable, Items, Item, PlaceableCategory> { // TODO dupplication

    public override int IconType => ItemID.ArchitectGizmoPack;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.RarityAmber;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<PlaceableRequirements>(this);
        for (int t = 0; t < ItemLoader.ItemCount; t++) {
            Item i = new(t);
            if (i.tileWand != -1) RegisterWand(i);
        }
    }

    public override Requirement GetRequirement(PlaceableCategory category) {
        return category switch {
            PlaceableCategory.Block or PlaceableCategory.Wall or PlaceableCategory.Wiring => new(Config.Obj.Tiles),
            PlaceableCategory.Torch => new(Config.Obj.Torches),
            PlaceableCategory.Ore => new(Config.Obj.Ores),

            PlaceableCategory.LightSource
                    or PlaceableCategory.Functional or PlaceableCategory.Decoration
                    or PlaceableCategory.Container or PlaceableCategory.CraftingStation
                => new(Config.Obj.Furnitures),
            PlaceableCategory.MusicBox => new(Config.Obj.Furnitures),
            PlaceableCategory.Liquid => new(Config.Obj.Liquids),
            PlaceableCategory.Mechanical => new(Config.Obj.Mechanical),
            PlaceableCategory.Seed => new(Config.Obj.Seeds),
            PlaceableCategory.Paint => new(Config.Obj.Paints),
            PlaceableCategory.None or _ => new(),
        };
    }
    
    public override PlaceableCategory GetCategory(Item item) => GetCategory(item, false);
    public static PlaceableCategory GetCategory(Item item, bool wand) {
        switch (item.type) {
        case ItemID.Hellstone or ItemID.DemoniteOre or ItemID.CrimtaneOre: return PlaceableCategory.Ore; // Main.tileSpelunker[tileType] == false
        }

        if (_wandAmmos.TryGetValue(item.type, out int wandType)) return GetCategory(new(wandType), true);
        if (item.paint != 0) return PlaceableCategory.Paint;
        if(ItemID.Sets.AlsoABuildingItem[item.type]) {
            if (item.FitsAmmoSlot() && item.mech) return PlaceableCategory.Wiring;
            // TODO buckets
        }

        if (!wand && (!item.consumable || item.useStyle == ItemUseStyleID.None)) return PlaceableCategory.None;
       
        if(item.XMasDeco()) return PlaceableCategory.Decoration;

        if (item.createTile != -1) {

            int tileType = item.createTile;
            if (item.accessory) return PlaceableCategory.MusicBox;
            if (TileID.Sets.Platforms[tileType]) return PlaceableCategory.Block;

            if (Main.tileAlch[tileType] || TileID.Sets.CommonSapling[tileType] || ItemID.Sets.GrassSeeds[item.type]) return PlaceableCategory.Seed;
            if (Main.tileContainer[tileType]) return PlaceableCategory.Container;

            if (item.mech) return PlaceableCategory.Mechanical;

            if (Main.tileFrameImportant[tileType]) {
                bool GoodTile(int t) => t == tileType;

                if (TileID.Sets.Torch[tileType]) return PlaceableCategory.Torch;
                if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTorch, GoodTile)) return PlaceableCategory.LightSource;

                if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsChair, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTable, GoodTile))
                    return PlaceableCategory.Functional;

                if (Systems.InfiniteRecipe.CraftingStations.Contains(tileType)) return PlaceableCategory.CraftingStation;

                if (TileID.Sets.HasOutlines[tileType]) return PlaceableCategory.Functional;

                return PlaceableCategory.Decoration;
            }

            if (Main.tileSpelunker[tileType] || ItemID.Sets.ExtractinatorMode[item.type] != -1) return PlaceableCategory.Ore;

            return PlaceableCategory.Block;
        }
        if (item.createWall != -1) return PlaceableCategory.Wall;

        return PlaceableCategory.None;
    }

    private static readonly Dictionary<int, int> _wandAmmos = new(); // ammoType, wandType
    internal static void ClearWandAmmos() => _wandAmmos.Clear();
    public static void RegisterWand(Item wand) {
        if (Instance.GetCategory(new(wand.tileWand)) == PlaceableCategory.None) _wandAmmos.TryAdd(wand.tileWand, wand.type);
    }

    public Wrapper<PlaceableRequirements> Config = null!;

    public override Item DisplayedValue(Item consumable) {
        return GetWandType(consumable) switch {
            WandType.Tile => Main.LocalPlayer.FindItemRaw(consumable.tileWand),
            WandType.Wire => Main.LocalPlayer.FindItemRaw(ItemID.Wire),
            WandType.PaintBrush or WandType.PaintRoller => Main.LocalPlayer.PickPaint(),
            WandType.None or _ => null
        } ?? consumable;
    }

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) {
        Item ammo = DisplayedValue(item);
        if (ammo == item) return (new(Mod, "Placeable", Lang.tip[33].Value), TooltipLineID.Placeable);
        (string name, TooltipLineID position) = GetWandType(item) switch {
            WandType.Tile => ("WandConsumes", TooltipLineID.WandConsumes),
            WandType.Wire => ("Tooltip0", TooltipLineID.Tooltip),
            WandType.None or WandType.PaintBrush or WandType.PaintRoller or _ => ("PaintConsumes", TooltipLineID.Modded)
        };
        return (new(Mod, name, Lang.tip[52].Value + ammo.Name), position);
    }

    public enum WandType {
        None,
        Tile,
        Wire,
        PaintBrush,
        PaintRoller
    }

    public static WandType GetWandType(Item item) => item switch {
        { tileWand: not -1 } => WandType.Tile,
        { type: ItemID.Wrench or ItemID.BlueWrench or ItemID.GreenWrench or ItemID.YellowWrench or ItemID.MulticolorWrench or ItemID.WireKite } => WandType.Wire,
        { type: ItemID.Paintbrush or ItemID.SpectrePaintbrush } => WandType.PaintBrush,
        { type: ItemID.PaintRoller or ItemID.SpectrePaintRoller } => WandType.PaintRoller,
        _ => WandType.None
    };


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

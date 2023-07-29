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

public sealed class PlaceableRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Tiles")]
    public Count Tiles = 999;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Ores")]
    public Count Ores = 499;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Torches")]
    public Count Torches = 99;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Furnitures")]
    public Count Furnitures = 3;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Mechanical")]
    public Count Mechanical = 3;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Liquids")]
    public Count Liquids = 10;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Seeds")]
    public Count Seeds = 20;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Paints")]
    public Count Paints = 999;
}

public sealed class Placeable : InfinityStatic<Placeable, Items, Item, PlaceableCategory> {

    public override int IconType => ItemID.ArchitectGizmoPack;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.RarityAmber;


    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<PlaceableRequirements>(this);
        for (int t = 0; t < ItemLoader.ItemCount; t++) {
            Item i = new(t);
            if (i.tileWand != -1) RegisterWand(i);
        }
        DisplayOverrides += AmmoSlots;
        InfinityOverrides += DuplicationInfinity;
    }

    public override void Unload() {
        base.Unload();
        ClearWandAmmos();
    }

    public override Requirement GetRequirement(PlaceableCategory category) {
        return category switch {
            PlaceableCategory.Block or PlaceableCategory.Wall or PlaceableCategory.Wiring => new(Config.Value.Tiles),
            PlaceableCategory.Torch => new(Config.Value.Torches),
            PlaceableCategory.Ore => new(Config.Value.Ores),

            PlaceableCategory.LightSource
                    or PlaceableCategory.Functional or PlaceableCategory.Decoration
                    or PlaceableCategory.Container or PlaceableCategory.CraftingStation
                => new(Config.Value.Furnitures),
            PlaceableCategory.MusicBox => new(Config.Value.Furnitures),
            PlaceableCategory.Liquid => new(Config.Value.Liquids),
            PlaceableCategory.Mechanical => new(Config.Value.Mechanical),
            PlaceableCategory.Seed => new(Config.Value.Seeds),
            PlaceableCategory.Paint => new(Config.Value.Paints),
            PlaceableCategory.None or _ => new(),
        };
    }

    public override PlaceableCategory GetCategory(Item item) {
        PlaceableCategory category = GetCategory(item, false);
        return category == PlaceableCategory.None && IsWandAmmo(item.type, out int wandType) ? GetCategory(new(wandType), true) : category;
    }

    public static PlaceableCategory GetCategory(Item item, bool wand) {
        switch (item.type) {
        case ItemID.Hellstone or ItemID.DemoniteOre or ItemID.CrimtaneOre: return PlaceableCategory.Ore; // Main.tileSpelunker[tileType] == false
        }

        if (item.paint != 0) return PlaceableCategory.Paint;
        if(ItemID.Sets.AlsoABuildingItem[item.type]) {
            // TODO buckets
        }
        if (item.FitsAmmoSlot() && item.mech) return PlaceableCategory.Wiring;

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
    public static void RegisterWand(Item wand) => _wandAmmos.TryAdd(wand.tileWand, wand.type);
    public static bool IsWandAmmo(int type, out int wandType) => _wandAmmos.TryGetValue(type, out wandType);

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

    public static void AmmoSlots(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index < 50 || 58 <= index) return;

        PlaceableCategory category = Instance.GetCategory(item);
        if (category == PlaceableCategory.Wiring || category == PlaceableCategory.Paint || IsWandAmmo(item.type, out _)) visibility = InfinityVisibility.Exclusive;
    }

    public static void DuplicationInfinity(Player _, Item consumable, Requirement requirement, long count, ref long infinity, List<object> extras) {
        if(!InfinitySettings.Instance.PreventItemDupication || count <= requirement.Count) return;
        if(consumable.createTile != -1 || consumable.createWall != -1 || IsWandAmmo(consumable.type, out int _) || (consumable.FitsAmmoSlot() && consumable.mech)) {
            extras.Add(new InfinityOverride("Tile duplication"));
            infinity = 0;
        }
    }

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

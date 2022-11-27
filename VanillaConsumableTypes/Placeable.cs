using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.VanillaConsumableTypes;
public enum PlaceableCategory : byte {
    None = Category.None,

    Block,
    Wall,
    Wiring,
    Torch,
    Ore, //demonite and crymtane, hellstone

    LightSource,
    Container,
    Functional,
    CraftingStation,
    Decoration,
    MusicBox,

    Mechanical, // statues
    Liquid,
    Seed, //gemtrees, mud seeds
    Paint
}

public static class PlaceableExtension {
    public static bool IsCommonTile(this PlaceableCategory category) => category != PlaceableCategory.None && category < PlaceableCategory.LightSource;
    public static bool IsFurniture(this PlaceableCategory category) => category != PlaceableCategory.None && !category.IsCommonTile() && category < PlaceableCategory.Mechanical;
    public static bool IsMisc(this PlaceableCategory category) => category != PlaceableCategory.None && !category.IsCommonTile() && !category.IsFurniture();
}

public class PlaceableRequirements {
    [Label("$Mods.SPIC.Types.Placeable.tiles")]
    public ItemCountWrapper Tiles = new(1.0f);
    [Label("$Mods.SPIC.Types.Placeable.ores")]
    public ItemCountWrapper Ores = new(499);
    [Label("$Mods.SPIC.Types.Placeable.torches")]
    public ItemCountWrapper Torches = new(99);
    [Label("$Mods.SPIC.Types.Placeable.furnitures")]
    public ItemCountWrapper Furnitures = new(3, 99);
    [Label("$Mods.SPIC.Types.Placeable.mechanical")]
    public ItemCountWrapper Mechanical = new(3);
    [Label("$Mods.SPIC.Types.Placeable.liquids")]
    public ItemCountWrapper Liquids = new(10);
    [Label("$Mods.SPIC.Types.Placeable.seeds")]
    public ItemCountWrapper Seeds = new(20, 99);
    [Label("$Mods.SPIC.Types.Placeable.paints")]
    public ItemCountWrapper Paints = new(1.0f, 999);
}

public class Placeable : ItemGroup<Placeable, PlaceableCategory>, IConfigurable<PlaceableRequirements>, ICustomizable, IDetectable {

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.ArchitectGizmoPack;

    public override bool DefaultsToOn => false;
#nullable disable
    public PlaceableRequirements Settings { get; set; }
#nullable restore

    public override IRequirement Requirement(PlaceableCategory category) {
        return category switch {
            PlaceableCategory.Block or PlaceableCategory.Wall or PlaceableCategory.Wiring => new CountRequirement((ItemCount)Settings.Tiles),
            PlaceableCategory.Torch => new CountRequirement((ItemCount)Settings.Torches),
            PlaceableCategory.Ore => new CountRequirement((ItemCount)Settings.Ores),

            PlaceableCategory.LightSource
                    or PlaceableCategory.Functional or PlaceableCategory.Decoration
                    or PlaceableCategory.Container or PlaceableCategory.CraftingStation
                => new CountRequirement((ItemCount)Settings.Furnitures),
            PlaceableCategory.MusicBox => new CountRequirement(new ItemCount(Settings.Furnitures){MaxStack = 1}),
            PlaceableCategory.Liquid => new CountRequirement((ItemCount)Settings.Liquids),
            PlaceableCategory.Mechanical => new CountRequirement((ItemCount)Settings.Mechanical),
            PlaceableCategory.Seed => new CountRequirement((ItemCount)Settings.Seeds),
            PlaceableCategory.Paint => new CountRequirement((ItemCount)Settings.Paints),
            PlaceableCategory.None or _ => new NoRequirement(),
        };
    }
    
    public override PlaceableCategory GetCategory(Item item) {
        if (!(item.consumable && item.useStyle != ItemUseStyleID.None) && item.paint == 0 && !s_ammos.ContainsKey(item.type) && !(item.FitsAmmoSlot() && item.mech))
            return PlaceableCategory.None;
        return GetCategory_NoCheck(item);
    }

    private static PlaceableCategory GetCategory_NoCheck(Item item) {

        if (item.paint != 0) return PlaceableCategory.Paint;

        switch (item.type) {
        case ItemID.Hellstone: return PlaceableCategory.Ore;
        }

        if (item.createTile != -1) {

            int tileType = item.createTile;
            if (item.accessory) return PlaceableCategory.MusicBox;
            if (TileID.Sets.Platforms[tileType]) return PlaceableCategory.Block;

            if (Main.tileAlch[tileType] || TileID.Sets.TreeSapling[tileType] || TileID.Sets.Grass[tileType]) return PlaceableCategory.Seed;
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

            if (Main.tileSpelunker[tileType]) return PlaceableCategory.Ore;

            return PlaceableCategory.Block;
        }
        if (item.createWall != -1) return PlaceableCategory.Wall;

        if(item.FitsAmmoSlot() && item.mech) return PlaceableCategory.Wiring;
        if (s_ammos.TryGetValue(item.type, out PlaceableCategory category)) return category;

        return PlaceableCategory.None;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityAmber;
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Placeable", Lang.tip[33].Value);

    public enum WandType {
        None,
        Tile,
        Wire,
        PaintBrush,
        PaintRoller
    }

    // TooltipLine IDefaultAmmunition.WeaponTooltipLine(Item weapon, Item ammo) => GetWandType(weapon) switch {
    //     WandType.Tile => new(Mod, $"WanndConsumes", Language.GetTextValue("Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name)),
    //     WandType.Wire => new(Mod, $"Tooltip0", Language.GetTextValue("Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name)),
    //     WandType.None or WandType.PaintBrush or WandType.PaintRoller or _=> new(Mod, $"PaintConsumes", Language.GetTextValue("Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name))
    // };

    public static WandType GetWandType(Item item) => item switch { { tileWand: not -1 } => WandType.Tile,
        { type: ItemID.Wrench or ItemID.BlueWrench or ItemID.GreenWrench or ItemID.YellowWrench or ItemID.MulticolorWrench or ItemID.WireKite } => WandType.Wire,
        { type: ItemID.Paintbrush or ItemID.SpectrePaintbrush } => WandType.PaintBrush,
        { type: ItemID.PaintRoller or ItemID.SpectrePaintRoller } => WandType.PaintRoller,
        _ => WandType.None
    };

    // public bool TryGetAlternate(Player player, Item wand, [MaybeNullWhen(false)] out Item alt){
    //     alt = GetWandType(wand) switch {
    //         WandType.Tile => System.Array.Find(player.inventory, item => item.type == wand.tileWand),
    //         WandType.Wire => System.Array.Find(player.inventory, item => item.type == ItemID.Wire),
    //         WandType.PaintBrush or WandType.PaintRoller => player.PickPaint(),
    //         WandType.None or _ => null
    //     };
    //     return alt is not null;
    // }

    private static readonly Dictionary<int, PlaceableCategory> s_ammos = new(); // type, category (ammo)
    internal static void ClearWandAmmos() => s_ammos.Clear();
    public static void RegisterWand(Item wand) => s_ammos.TryAdd(wand.tileWand, GetCategory_NoCheck(wand));

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

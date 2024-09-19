using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Terraria.ModLoader;
using SpikysLib.IL;
using MonoMod.Cil;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Default.Displays;
using SpikysLib;
using Terraria.Localization;
using SpikysLib.Constants;

namespace SPIC.Default.Infinities;

public enum PlaceableCategory {
    None,

    Tile,
    // Block,
    // Wall,
    Torch,
    Ore,

    Furniture,
    // LightSource,
    // Container,
    // Functional,
    // CraftingStation,
    // Decoration,
    // MusicBox,

    Wiring,
    Mechanical,
    Bucket,
    Seed,
    Paint
}
public static class PlaceableExtension {
    public static bool IsCommonTile(this PlaceableCategory category) => category != PlaceableCategory.None && category < PlaceableCategory.Furniture;
    public static bool IsFurniture(this PlaceableCategory category) => category != PlaceableCategory.None && !category.IsCommonTile() && category < PlaceableCategory.Mechanical;
    public static bool IsMisc(this PlaceableCategory category) => category != PlaceableCategory.None && !category.IsCommonTile() && !category.IsFurniture();
}

// TODO PreventItemDuplication
public sealed class PlaceableRequirements {
    public Count<PlaceableCategory> Tile = 999;
    public Count<PlaceableCategory> Ore = 499;
    public Count<PlaceableCategory> Torch = 99;
    public Count<PlaceableCategory> Furniture = 3;
    public Count<PlaceableCategory> Mechanical = 3;
    public Count<PlaceableCategory> Wiring = 999;
    public Count<PlaceableCategory> Bucket = 10;
    public Count<PlaceableCategory> Seed = 20;
    public Count<PlaceableCategory> Paint = 999;
}

public sealed class Placeable : Infinity<Item, PlaceableCategory>, IConfigProvider<PlaceableRequirements>, ITooltipLineDisplay {
    public static Placeable Instance = null!;
    public PlaceableRequirements Config { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;

    public override Color DefaultColor => Colors.RarityAmber;

    public override void Load() {
        IL_Player.PlaceThing_Tiles_PlaceIt_ConsumeFlexibleWandMaterial += IL_FixConsumeFlexibleWand;
        base.Load();
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        for (int t = 0; t < ItemLoader.ItemCount; t++) {
            Item i = new(t);
            if (i.tileWand != -1) RegisterWand(i);
        }
    }

    private static void IL_FixConsumeFlexibleWand(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(i => i.SaferMatchCall(Reflection.ItemLoader.ConsumeItem))) return; // Already there

        ILLabel? br = null;
        if (!cursor.TryGotoNext(i => i.MatchLdfld(Reflection.Item.stack)) || !cursor.TryGotoPrev(MoveType.After, i => i.MatchBrfalse(out br))) {
            SpysInfiniteConsumables.Instance.Logger.Error($"{nameof(IL_FixConsumeFlexibleWand)} failed to apply. Rubblemaker will not be infinite");
            return;
        }
        cursor.EmitLdloc1().EmitLdarg0();
        cursor.EmitCall(Reflection.ItemLoader.ConsumeItem);
        cursor.EmitBrfalse(br!);
    }

    public override void Unload() {
        base.Unload();
        ClearWandAmmos();
    }

    public override long GetRequirement(PlaceableCategory category) => category switch {
        PlaceableCategory.Tile => Config.Tile,
        PlaceableCategory.Wiring => Config.Wiring,
        // PlaceableCategory.Block or PlaceableCategory.Wall or PlaceableCategory.Wiring => Infinities.GetInstance).Tile),
        PlaceableCategory.Torch => Config.Torch,
        PlaceableCategory.Ore => Config.Ore,
        PlaceableCategory.Furniture => Config.Furniture,
        // PlaceableCategory.LightSource or PlaceableCategory.Functional or PlaceableCategory.Decoration
        //         or PlaceableCategory.Container or PlaceableCategory.CraftingStation or PlaceableCategory.MusicBox
        //     => Infinities.GetInstance).Furniture),
        PlaceableCategory.Bucket => Config.Bucket,
        PlaceableCategory.Mechanical => Config.Mechanical,
        PlaceableCategory.Seed => Config.Seed,
        PlaceableCategory.Paint => Config.Paint,
        _ => 0,
    };

    protected override PlaceableCategory GetCategoryInner(Item item) {
        PlaceableCategory category = GetCategory(item, false);
        return category == PlaceableCategory.None && IsWandAmmo(item.type, out int wandType) ? GetCategory(new(wandType), true) : category;
    }

    public static PlaceableCategory GetCategory(Item item, bool wand) {
        switch (item.type) {
        case ItemID.Hellstone or ItemID.DemoniteOre or ItemID.CrimtaneOre: return PlaceableCategory.Ore; // Main.tileSpelunker[tileType] == false
        }

        if (item.PaintOrCoating) return PlaceableCategory.Paint;
        if(!item.consumable && ItemID.Sets.AlsoABuildingItem[item.type] && ItemID.Sets.IsLavaImmuneRegardlessOfRarity[item.type] && item.maxStack != 1) return PlaceableCategory.Bucket;
        if (item.FitsAmmoSlot() && item.mech) return PlaceableCategory.Wiring;

        if (!wand && (!item.consumable || item.useStyle == ItemUseStyleID.None)) return PlaceableCategory.None;

        if (item.XMasDeco()) return PlaceableCategory.Furniture;

        if (item.createTile != -1) {
            int tileType = item.createTile;
            if (item.accessory) return PlaceableCategory.Furniture;
            if (TileID.Sets.Platforms[tileType]) return PlaceableCategory.Tile;
            if (Main.tileAlch[tileType] || TileID.Sets.CommonSapling[tileType] || ItemID.Sets.GrassSeeds[item.type]) return PlaceableCategory.Seed;
            if (Main.tileContainer[tileType]) return PlaceableCategory.Furniture;
            if (item.mech) return PlaceableCategory.Mechanical;
            if (Main.tileFrameImportant[tileType]) return PlaceableCategory.Furniture;
            if (Main.tileSpelunker[tileType] || ItemID.Sets.ExtractinatorMode[item.type] != -1) return PlaceableCategory.Ore;
            return PlaceableCategory.Tile;
        }
        if (item.createWall != -1) return PlaceableCategory.Tile;

        return PlaceableCategory.None;

        // if(item.XMasDeco()) return PlaceableCategory.Decoration;
        // if (item.createTile != -1) {
        //     int tileType = item.createTile;
        //     if (item.accessory) return PlaceableCategory.MusicBox;
        //     if (TileID.Sets.Platforms[tileType]) return PlaceableCategory.Block;
        //     if (Main.tileAlch[tileType] || TileID.Sets.CommonSapling[tileType] || ItemID.Sets.GrassSeeds[item.type]) return PlaceableCategory.Seed;
        //     if (Main.tileContainer[tileType]) return PlaceableCategory.Container;
        //     if (item.mech) return PlaceableCategory.Mechanical;
        //     if (Main.tileFrameImportant[tileType]) {
        //         bool GoodTile(int t) => t == tileType;
        //         if (TileID.Sets.Torch[tileType]) return PlaceableCategory.Torch;
        //         if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTorch, GoodTile)) return PlaceableCategory.LightSource;
        //         if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsChair, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTable, GoodTile))
        //             return PlaceableCategory.Functional;
        //         if (Systems.InfiniteRecipe.CraftingStations.Contains(tileType)) return PlaceableCategory.CraftingStation;
        //         if (TileID.Sets.HasOutlines[tileType]) return PlaceableCategory.Functional;
        //         return PlaceableCategory.Decoration;
        //     }
        //     if (Main.tileSpelunker[tileType] || ItemID.Sets.ExtractinatorMode[item.type] != -1) return PlaceableCategory.Ore;
        //     return PlaceableCategory.Block;
        // }
        // if (item.createWall != -1) return PlaceableCategory.Wall;
        // return PlaceableCategory.None;
    }

    private static readonly Dictionary<int, int> s_wandAmmos = []; // ammoType, wandType
    internal static void ClearWandAmmos() => s_wandAmmos.Clear();
    public static void RegisterWand(Item wand) => s_wandAmmos.TryAdd(wand.tileWand, wand.type);
    public static bool IsWandAmmo(int type, out int wandType) => s_wandAmmos.TryGetValue(type, out wandType);

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) {
        if (displayed == item.type) {
            if (item.XMasDeco()) return (new(Mod, "Tooltip0", Language.GetTextValue("CommonItemTooltip.PlaceableOnXmasTree")), TooltipLineID.Tooltip);
            return (new(Mod, "Placeable", Lang.tip[33].Value), TooltipLineID.Placeable);
        }
        (string name, TooltipLineID position) = GetWandType(item) switch {
            WandType.Wire => ("Tooltip0", TooltipLineID.Tooltip),
            WandType.PaintBrush or WandType.PaintRoller => ("PaintConsumes", TooltipLineID.Modded),
            WandType.Tile or WandType.Flexible or _ => ("WandConsumes", TooltipLineID.WandConsumes),
        };
        return (new(Mod, name, Lang.tip[52].Value + Lang.GetItemName(displayed)), position);
    }

    public enum WandType {
        None,
        Tile,
        Flexible,
        Wire,
        PaintBrush,
        PaintRoller
    }

    public static WandType GetWandType(Item item) => item switch {
        { tileWand: not -1 } => WandType.Tile,
        { type: ItemID.Wrench or ItemID.BlueWrench or ItemID.GreenWrench or ItemID.YellowWrench or ItemID.MulticolorWrench or ItemID.WireKite } => WandType.Wire,
        { type: ItemID.Paintbrush or ItemID.SpectrePaintbrush } => WandType.PaintBrush,
        { type: ItemID.PaintRoller or ItemID.SpectrePaintRoller } => WandType.PaintRoller,
        _ => item.GetFlexibleTileWand() is not null ? WandType.Flexible : WandType.None
    };

    // protected override void ModifyInfinity(Player player, Item consumable, ref InfinityValue value) {
    //     if(!InfinitySettings.Instance.PreventItemDuplication || value.Count <= value.Requirement.Count) return;
    //     if(consumable.createTile != -1 || consumable.createWall != -1 || IsWandAmmo(consumable.type, out int _) || (consumable.FitsAmmoSlot() && consumable.mech)) {
    //         extras.Add(this.GetLocalizationKey("TileDuplication"));
    //         infinity = 0;
    //     }
    // }
    
    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? ammo = GetWandType(item) switch {
            WandType.Tile => Main.LocalPlayer.FindItemRaw(item.tileWand),
            WandType.Wire => Main.LocalPlayer.FindItemRaw(ItemID.Wire),
            WandType.PaintBrush or WandType.PaintRoller => Main.LocalPlayer.PickPaint(),
            WandType.Flexible => item.GetFlexibleTileWand().TryGetPlacementOption(Main.LocalPlayer, Player.FlexibleWandRandomSeed, Player.FlexibleWandCycleOffset, out _, out Item i) ? i : null,
            _ => null
        };
        if (ammo is not null) displayed.Add(ammo);
    }

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index < 50 || 58 <= index) return;

        PlaceableCategory category = GetCategory(item);
        if (category == PlaceableCategory.Wiring || category == PlaceableCategory.Paint || IsWandAmmo(item.type, out _)) visibility = InfinityVisibility.Exclusive;
    }
}

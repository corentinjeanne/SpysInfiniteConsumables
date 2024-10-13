using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Terraria.ModLoader;
using SpikysLib.IL;
using MonoMod.Cil;
using SPIC.Default.Displays;
using SpikysLib;
using Terraria.Localization;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using SpikysLib.Configs;

namespace SPIC.Default.Infinities;

public enum PlaceableCategory {
    None,

    Tile,
    Torch,
    Ore,

    Furniture,

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

[CustomModConfigItem(typeof(ObjectMembersElement))]
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
    public Toggle<PreventItemDuplication> preventItemDuplication = new(true);
}

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class PlaceableDisplay {
    public bool infiniteTooltip;
}

public sealed class PreventItemDuplication {
    [DefaultValue(true)] public bool allowMiscDrops = true;
}

public sealed class Placeable : Infinity<Item, PlaceableCategory>, IConfigProvider<PlaceableRequirements>, IClientConfigProvider<PlaceableDisplay>, ITooltipLineDisplay {
    public static Placeable Instance = null!;

    public static bool PreventItemDuplication => Instance.Enabled && Instance.Config.preventItemDuplication;
    public PlaceableRequirements Config { get; set; } = null!;
    public PlaceableDisplay ClientConfig { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;

    public sealed override InfinityDefaults Defaults => new() { Color = Colors.RarityAmber };

    public override void Load() {
        IL_Player.PlaceThing_Tiles_PlaceIt_ConsumeFlexibleWandMaterial += IL_FixConsumeFlexibleWand;
        base.Load();
    }

    public override void SetStaticDefaults() {
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
        PlaceableCategory.Torch => Config.Torch,
        PlaceableCategory.Ore => Config.Ore,
        PlaceableCategory.Furniture => Config.Furniture,
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
        if (!item.consumable && ItemID.Sets.AlsoABuildingItem[item.type] && ItemID.Sets.IsLavaImmuneRegardlessOfRarity[item.type] && item.maxStack != 1) return PlaceableCategory.Bucket;
        if (item.FitsAmmoSlot() && item.mech) return PlaceableCategory.Wiring;
        if (!wand && (!item.consumable || item.useStyle == ItemUseStyleID.None)) return PlaceableCategory.None;

        if (item.XMasDeco()) return PlaceableCategory.Furniture;

        if (item.createTile != -1) {
            int tileType = item.createTile;
            if (TileID.Sets.Torch[tileType]) return PlaceableCategory.Torch;
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
        Item { tileWand: not -1 } => WandType.Tile,
        Item { type: ItemID.Wrench or ItemID.BlueWrench or ItemID.GreenWrench or ItemID.YellowWrench or ItemID.MulticolorWrench or ItemID.WireKite } => WandType.Wire,
        Item { type: ItemID.Paintbrush or ItemID.SpectrePaintbrush } => WandType.PaintBrush,
        Item { type: ItemID.PaintRoller or ItemID.SpectrePaintRoller } => WandType.PaintRoller,
        _ => item.GetFlexibleTileWand() is not null ? WandType.Flexible : WandType.None
    };

    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? ammo = GetAmmo(Main.LocalPlayer, item);
        if (ammo is not null) displayed.Add(ammo);
    }

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index < 50 || 58 <= index) return;

        PlaceableCategory category = InfinityManager.GetCategory(item, this);
        if (category == PlaceableCategory.Wiring || category == PlaceableCategory.Paint || IsWandAmmo(item.type, out _)) visibility = InfinityVisibility.Exclusive;
    }

    public static Item? GetAmmo(Player player, Item item) => GetWandType(item) switch {
        WandType.Tile => player.FindItemRaw(item.tileWand),
        WandType.Wire => player.FindItemRaw(ItemID.Wire),
        WandType.PaintBrush or WandType.PaintRoller => player.PickPaint(),
        WandType.Flexible => item.GetFlexibleTileWand().TryGetPlacementOption(player, Player.FlexibleWandRandomSeed, Player.FlexibleWandCycleOffset, out _, out Item i) ? i : null,
        _ => null
    };
}

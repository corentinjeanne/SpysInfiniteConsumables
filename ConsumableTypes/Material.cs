using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

public enum MaterialCategory {
    None = ConsumableType.NoCategory,
    Basic,
    Ore,
    Furniture,
    Miscellaneous,
    NonStackable
}

public class MaterialRequirements {
    [Label("$Mods.SPIC.Types.Material.basics")]
    public Configs.Requirement Basics = -1;
    [Label("$Mods.SPIC.Types.Placeable.ores")]
    public Configs.Requirement Ores = 500;
    [Label("$Mods.SPIC.Types.Placeable.furnitures")]
    public Configs.Requirement Furnitures = 20;
    [Label("$Mods.SPIC.Types.Material.misc")]
    public Configs.Requirement Miscellaneous = 50;
    [Range(-50, 0), Label("$Mods.SPIC.Types.Material.special")]
    public Configs.RequirementItems NonStackable = -2;
}

public class Material : ConsumableType<Material>, IPartialInfinity {

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Types.Material.name");

    public override int MaxStack(byte category) => (MaterialCategory)category switch {
        MaterialCategory.Basic => 999,
        MaterialCategory.Ore => 999,
        MaterialCategory.Furniture => 99,
        MaterialCategory.Miscellaneous => 999,
        MaterialCategory.NonStackable => 1,
        MaterialCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        MaterialRequirements reqs = (MaterialRequirements)ConfigRequirements;
        return (MaterialCategory)category switch {
            MaterialCategory.Basic => reqs.Basics,
            MaterialCategory.Ore => reqs.Ores,
            MaterialCategory.Furniture => reqs.Furnitures,
            MaterialCategory.Miscellaneous => reqs.Miscellaneous,
            MaterialCategory.NonStackable => reqs.NonStackable,
            MaterialCategory.None or _ => NoRequirement,
        };
    }

    public override byte GetCategory(Item item) {

        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return (byte)MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return (byte)MaterialCategory.Basic;

        if (!ItemID.Sets.IsAMaterial[type]) return (byte)MaterialCategory.None;

        if (Globals.ConsumptionItem.MaxStack(type) == 1) return (byte)MaterialCategory.NonStackable;

        PlaceableCategory placeable = (PlaceableCategory)item.GetCategory(Placeable.ID);

        if (placeable.IsFurniture()) return (byte)MaterialCategory.Furniture;
        if (placeable == PlaceableCategory.Ore) return (byte)MaterialCategory.Ore;
        if (placeable.IsCommonTile()
                || type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow
                || type == ItemID.Wire || type == ItemID.BottledWater
                || type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
            return (byte)MaterialCategory.Basic;

        return (byte)MaterialCategory.Miscellaneous;
    }

    public override long GetInfinity(Item item, long count)
        => InfinityManager.CalculateInfinity(item.type, MaxStack(InfinityManager.GetCategory(item, ID)), count, InfinityManager.GetRequirement(item, ID), 0.5f, InfinityManager.ARIDelegates.LargestMultiple);

    // TODO improve to use the available recipes
    public long GetFullInfinity(Player player, Item item) => Systems.InfiniteRecipe.HighestCost(item.type);

    public override InfinityDisplayFlag GetInfinityDisplayLevel(Item item, bool isACopy) {
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;

        Recipe recipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
        foreach(Item material in recipe.requiredItem){
            if(AreSameItems(item, material)) return InfinityDisplayFlag.All;
        }
        return base.GetInfinityDisplayLevel(item, isACopy);
    }
    public override Microsoft.Xna.Framework.Color DefaultColor() => Colors.RarityPink;

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Material", Lang.tip[36].Value);
    public override string LocalizedCategoryName(byte category) => ((MaterialCategory)category).ToString();

    public override MaterialRequirements CreateRequirements() => new();
}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

// TODO only display material infinity on mat for selected recipe (& if inf mat)

public enum MaterialCategory {
    None = ConsumableType.NoCategory,
    Basic,
    Ore,
    Furniture,
    Miscellaneous,
    NonStackable
}

public class MaterialRequirements {
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Basics")]
    public int Basics = -1;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Ores")]
    public int Ores = 500;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Furnitures")]
    public int Furnitures = 20;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Miscellaneous")]
    public int Miscellaneous = 50;
    [Range(-50, 0), Label("$Mods.SPIC.Configs.Requirements.Requirements.NonStackable")]
    public int NonStackable = -2;
}

public class Material : ConsumableType<Material> {

    public override int MaxStack(byte category) => (MaterialCategory)category switch {
        MaterialCategory.Basic => 999,
        MaterialCategory.Ore => 999,
        MaterialCategory.Furniture => 99,
        MaterialCategory.Miscellaneous => 999,
        MaterialCategory.NonStackable => 1,
        MaterialCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        MaterialRequirements reqs = (MaterialRequirements)Requirements;
        return (MaterialCategory)category switch {
            MaterialCategory.Basic => reqs.Basics,
            MaterialCategory.Ore => reqs.Ores,
            MaterialCategory.Furniture => reqs.Furnitures,
            MaterialCategory.Miscellaneous => reqs.Miscellaneous,
            MaterialCategory.NonStackable => reqs.NonStackable,
            MaterialCategory.None or _ => NoRequirement,
        };
    }

    
    public override bool Customs => false;

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
    public override bool IsFullyInfinite(Item item, long infinity) => infinity >= Systems.InfiniteRecipe.HighestCost(item.type);


    public override Microsoft.Xna.Framework.Color DefaultColor() => new(255, 120, 187);

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Material", Lang.tip[36].Value);
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.Material.{(MaterialCategory)category}";

    public override MaterialRequirements CreateRequirements() => new();
}

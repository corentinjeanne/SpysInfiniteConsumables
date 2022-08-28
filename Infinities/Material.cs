using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Infinities;

// TODO only display material infininity on mat for selected recipe (& if inf mat)

public enum MaterialCategory {
    None = Infinity.NoCategory,
    Basic,
    Ore,
    Furniture,
    Miscellaneous,
    NonStackable
}

public class Material : Infinity<Material> {

    public override int MaxStack(byte category) => (MaterialCategory)category switch {
        MaterialCategory.Basic => 999,
        MaterialCategory.Ore => 999,
        MaterialCategory.Furniture => 99,
        MaterialCategory.Miscellaneous => 999,
        MaterialCategory.NonStackable => 1,
        MaterialCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        Configs.Requirements inf = Configs.Requirements.Instance;
        return (MaterialCategory)category switch {
            MaterialCategory.Basic => inf.materials_Basics,
            MaterialCategory.Ore => inf.materials_Ores,
            MaterialCategory.Furniture => inf.materials_Furnitures,
            MaterialCategory.Miscellaneous => inf.materials_Miscellaneous,
            MaterialCategory.NonStackable => inf.materials_NonStackable,
            MaterialCategory.None or _ => Infinity.NoRequirement,
        };
    }

    public override bool Enabled => Configs.Requirements.Instance.InfiniteMaterials;

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


    public override Microsoft.Xna.Framework.Color Color => Configs.InfinityDisplay.Instance.color_Materials;

    public override TooltipLine TooltipLine => AddedLine("Material", Lang.tip[36].Value);
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.Material.{(MaterialCategory)category}";
}

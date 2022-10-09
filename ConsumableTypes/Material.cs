using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

public enum MaterialCategory : byte {
    None = Category.None,
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

public class Material : ConsumableType<Material>, IStandardConsumableType<MaterialCategory, MaterialRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.TinkerersWorkshop;

    public bool DefaultsToOn => false;
    public MaterialRequirements Settings { get; set; }

    public int MaxStack(MaterialCategory category) => category switch {
        MaterialCategory.Basic => 999,
        MaterialCategory.Ore => 999,
        MaterialCategory.Furniture => 99,
        MaterialCategory.Miscellaneous => 999,
        MaterialCategory.NonStackable => 1,
        MaterialCategory.None or _ => 999,
    };
    public int Requirement(MaterialCategory category) {
        return category switch {
            MaterialCategory.Basic => Settings.Basics,
            MaterialCategory.Ore => Settings.Ores,
            MaterialCategory.Furniture => Settings.Furnitures,
            MaterialCategory.Miscellaneous => Settings.Miscellaneous,
            MaterialCategory.NonStackable => Settings.NonStackable,
            MaterialCategory.None or _ => IConsumableType.NoRequirement,
        };
    }

    public MaterialCategory GetCategory(Item item) {

        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return MaterialCategory.Basic;

        if (!ItemID.Sets.IsAMaterial[type]) return MaterialCategory.None;

        if (item.maxStack == 1) return MaterialCategory.NonStackable;

        PlaceableCategory placeable = (PlaceableCategory)item.GetCategory(Placeable.ID);

        if (placeable.IsFurniture()) return MaterialCategory.Furniture;
        if (placeable == PlaceableCategory.Ore) return MaterialCategory.Ore;
        if (placeable.IsCommonTile()
                || type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow
                || type == ItemID.Wire || type == ItemID.BottledWater
                || type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
            return MaterialCategory.Basic;

        return MaterialCategory.Miscellaneous;
    }

    public long GetInfinity(Item item, long count)
        => InfinityManager.CalculateInfinity(
            (int)System.MathF.Min(item.maxStack, MaxStack(item.GetCategory<MaterialCategory>(UID))),
            count,
            InfinityManager.GetRequirement(item, ID),
            0.5f,
            InfinityManager.ARIDelegates.LargestMultiple
        );

    // TODO improve to use the available recipes
    public long GetMaxInfinity(Player player, Item item) => Systems.InfiniteRecipe.HighestCost(item.type);

    public DisplayFlags GetInfinityDisplayFlags(Item item, bool isACopy) {
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;
        
        Recipe recipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
        foreach(Item material in recipe.requiredItem){
            if(AreSameItems(item, material)) return DisplayFlags.Infinity;
        }

        return DefaultImplementation.GetInfinityDisplayFlags(item, isACopy);
    }
    public Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityPink;
    public TooltipLine TooltipLine => TooltipHelper.AddedLine("Material", Lang.tip[36].Value);


}

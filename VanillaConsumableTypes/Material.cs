using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;
namespace SPIC.VanillaConsumableTypes;
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
    public ItemCountWrapper Basics = new(1.0f);
    [Label("$Mods.SPIC.Types.Placeable.ores")]
    public ItemCountWrapper Ores = new(500);
    [Label("$Mods.SPIC.Types.Placeable.furnitures")]
    public ItemCountWrapper Furnitures = new(20,99);
    [Label("$Mods.SPIC.Types.Material.misc")]
    public ItemCountWrapper Miscellaneous = new(50);
    [Label("$Mods.SPIC.Types.Material.special"), Configs.UI.NoSwapping]
    public ItemCountWrapper NonStackable = new(2,1);
}

public class Material : ConsumableType<Material>, IStandardConsumableType<MaterialCategory, MaterialRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.TinkerersWorkshop;

    public bool DefaultsToOn => false;
    public MaterialRequirements Settings { get; set; }

    public IRequirement Requirement(MaterialCategory category) => category switch {
        MaterialCategory.Basic => new MultipleRequirement(Settings.Basics, 0.5f),
        MaterialCategory.Ore => new MultipleRequirement(Settings.Ores, 0.5f),
        MaterialCategory.Furniture => new MultipleRequirement(Settings.Furnitures, 0.5f),
        MaterialCategory.Miscellaneous => new MultipleRequirement(Settings.Miscellaneous, 0.5f),
        MaterialCategory.NonStackable => new MultipleRequirement(Settings.NonStackable, 0.5f),
        MaterialCategory.None or _ => null,
    };

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

    // TODO improve to use the available recipes
    public long GetMaxInfinity(Player player, Item item) => Systems.InfiniteRecipe.HighestCost(item.type);

    public bool OwnsItem(Player player, Item item, bool isACopy) {
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;
        
        Recipe recipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
        foreach(Item material in recipe.requiredItem){
            if(AreSameItems(item, material)) return true;
        }

        return DefaultImplementation.OwnsItem(player, item, isACopy);
    }
    public Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityPink;
    public TooltipLine TooltipLine => TooltipHelper.AddedLine("Material", Lang.tip[36].Value);


}

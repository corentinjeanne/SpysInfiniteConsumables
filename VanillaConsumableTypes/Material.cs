using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
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

public class Material : ItemGroup<Material, MaterialCategory>, IConfigurable<MaterialRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.TinkerersWorkshop;

    public override bool DefaultsToOn => false;
#nullable disable
    public MaterialRequirements Settings { get; set; }
#nullable restore

    public override IRequirement Requirement(MaterialCategory category) => category switch {
        MaterialCategory.Basic => new MultipleRequirement((ItemCount)Settings.Basics, 0.5f),
        MaterialCategory.Ore => new MultipleRequirement((ItemCount)Settings.Ores, 0.5f),
        MaterialCategory.Furniture => new MultipleRequirement((ItemCount)Settings.Furnitures, 0.5f),
        MaterialCategory.Miscellaneous => new MultipleRequirement((ItemCount)Settings.Miscellaneous, 0.5f),
        MaterialCategory.NonStackable => new MultipleRequirement((ItemCount)Settings.NonStackable, 0.5f),
        MaterialCategory.None or _ => new NoRequirement(),
    };

    public override MaterialCategory GetCategory(Item item) {

        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return MaterialCategory.Basic;

        if (!ItemID.Sets.IsAMaterial[type]) return MaterialCategory.None;

        if (item.maxStack == 1) return MaterialCategory.NonStackable;

        PlaceableCategory placeable = item.GetCategory<PlaceableCategory>(Placeable.ID);

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
    public override long GetMaxInfinity(Player player, Item item) => Systems.InfiniteRecipe.HighestCost(item.type);

    public override bool OwnsItem(Player player, Item item, bool isACopy) {
        // bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;

        // if (!Main.craftingHide) {
        //     Recipe recipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
        //     foreach (Item material in recipe.requiredItem) {
        //         if (AreSameItems(item, material)) return true;
        //     }
        // }

        return base.OwnsItem(player, item, isACopy);
    }
    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityPink;
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Material", Lang.tip[36].Value);


}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using SPIC.Configs;
using Terraria.Localization;

namespace SPIC.VanillaGroups;
public enum MaterialCategory : byte {
    None = CategoryHelper.None,
    Basic,
    Ore,
    Furniture,
    Miscellaneous,
    NonStackable
}

public class MaterialRequirements {
    [Label($"${Localization.Keys.Groups}.Material.Basics")]
    public ItemCountWrapper Basics = new(){Stacks=1};
    [Label($"${Localization.Keys.Groups}.Placeable.Ores")]
    public ItemCountWrapper Ores = new(){Items=499};
    [Label($"${Localization.Keys.Groups}.Placeable.Furnitures")]
    public ItemCountWrapper Furnitures = new(99){Items=20};
    [Label($"${Localization.Keys.Groups}.Material.Misc")]
    public ItemCountWrapper Miscellaneous = new(){Items=50};
    [Label($"${Localization.Keys.Groups}.Material.Special")]
    public ItemCountWrapper NonStackable = new(swapping: false){Items=2};
}

public class Material : ItemGroup<Material, MaterialCategory>, IConfigurable<MaterialRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.Material.Name");
    public override int IconType => ItemID.TinkerersWorkshop;

    public override bool DefaultsToOn => false;

    public override Requirement<ItemCount> GetRequirement(MaterialCategory category, Item consumable) => category switch {
        MaterialCategory.Basic => new MultipleRequirement<ItemCount>(this.Settings().Basics, 0.5f, LongToCount(consumable, GetMaxInfinity(consumable))),
        MaterialCategory.Ore => new MultipleRequirement<ItemCount>(this.Settings().Ores, 0.5f, LongToCount(consumable, GetMaxInfinity(consumable))),
        MaterialCategory.Furniture => new MultipleRequirement<ItemCount>(this.Settings().Furnitures, 0.5f, LongToCount(consumable, GetMaxInfinity(consumable))),
        MaterialCategory.Miscellaneous => new MultipleRequirement<ItemCount>(this.Settings().Miscellaneous, 0.5f, LongToCount(consumable, GetMaxInfinity(consumable))),
        MaterialCategory.NonStackable => new MultipleRequirement<ItemCount>(this.Settings().NonStackable, 0.5f, LongToCount(consumable, GetMaxInfinity(consumable))),
        MaterialCategory.None or _ => new NoRequirement<ItemCount>(),
    };

    public override MaterialCategory GetCategory(Item item) {

        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return MaterialCategory.Basic;

        if (!ItemID.Sets.IsAMaterial[type]) return MaterialCategory.None;

        if (item.maxStack == 1) return MaterialCategory.NonStackable;

        PlaceableCategory placeable = item.GetCategory(Placeable.Instance);

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
    public static long GetMaxInfinity(Item item) => Systems.InfiniteRecipe.HighestCost(item.type);

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityPink;
    public override TooltipLine TooltipLine => new(Mod, "Material", Lang.tip[36].Value);


}

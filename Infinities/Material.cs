using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.Infinities;
public enum MaterialCategory {
    None,
    Basic,
    Ore,
    Furniture,
    Miscellaneous,
    NonStackable
}

public sealed class MaterialRequirements {
    [LabelKey($"${Localization.Keys.Infinties}.Material.Basics"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Basics = 999;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Ores"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Ores = 499;
    [LabelKey($"${Localization.Keys.Infinties}.Placeable.Furnitures"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Furnitures = 20;
    [LabelKey($"${Localization.Keys.Infinties}.Material.Misc"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Miscellaneous = 50;
    [LabelKey($"${Localization.Keys.Infinties}.Material.Special"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count NonStackable = 2;
}

public sealed class Material : InfinityStatic<Material, Items, Item, MaterialCategory> {
    
    public override int IconType => ItemID.TinkerersWorkshop;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.RarityPink;


    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<MaterialRequirements>(this);
    }

    public override Requirement GetRequirement(MaterialCategory category) => category switch {
        MaterialCategory.Basic => new(Config.Value.Basics, 0.5f),
        MaterialCategory.Ore => new(Config.Value.Ores, 0.5f),
        MaterialCategory.Furniture => new(Config.Value.Furnitures, 0.5f),
        MaterialCategory.Miscellaneous => new(Config.Value.Miscellaneous, 0.5f),
        MaterialCategory.NonStackable => new(Config.Value.NonStackable, 0.5f),
        MaterialCategory.None or _ => new(),
    };

    public override MaterialCategory GetCategory(Item item) {
        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return MaterialCategory.Basic;

        if (!item.material) return MaterialCategory.None;

        if (item.maxStack == 1) return MaterialCategory.NonStackable;

        PlaceableCategory placeable = Group.GetCategory(item, Placeable.Instance);

        if (placeable.IsFurniture()) return MaterialCategory.Furniture;
        if (placeable == PlaceableCategory.Ore) return MaterialCategory.Ore;
        if (placeable.IsCommonTile()
                || type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow
                || type == ItemID.Wire || type == ItemID.BottledWater
                || type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
            return MaterialCategory.Basic;

        return MaterialCategory.Miscellaneous;
    }

    public Wrapper<MaterialRequirements> Config = null!;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "Material", Lang.tip[36].Value), TooltipLineID.Material);

    public override long GetConsumedFromContext(Player player, Item item, out bool exclusive) {
        if (Main.numAvailableRecipes != 0) {
            Item? material = Main.recipe[Main.availableRecipe[Main.focusRecipe]].requiredItem.Find(i => i.IsSimilar(item)); // TODO count for groups (and infinity)
            if (material is not null) {
                exclusive = true;
                return material.stack;
            }
        }
        return base.GetConsumedFromContext(player, item, out exclusive);
    }
}

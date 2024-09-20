using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using System.ComponentModel;
using SPIC.Default.Displays;
using Terraria.ModLoader;
using SpikysLib;
using System;

namespace SPIC.Default.Infinities;

public enum MaterialCategory {
    None,
    Basic,
    Ore,
    Furniture,
    Miscellaneous,
    NonStackable
}

// TODO PreventItemDuplication
public sealed class MaterialRequirements {
    public Count<MaterialCategory> Basic = 999;
    public Count<MaterialCategory> Ore = 499;
    public Count<MaterialCategory> Furniture = 20;
    public Count<MaterialCategory> Miscellaneous = 50;
    public Count<MaterialCategory> NonStackable = 2;
    [DefaultValue(0.5f), Range(0.01f, 1f)] public float Multiplier = 0.5f;
}

public sealed class Material : Infinity<Item, MaterialCategory>, IConfigProvider<MaterialRequirements>, ITooltipLineDisplay{
    public static Material Instance = null!;
    public MaterialRequirements Config { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;
    
    public sealed override InfinityDefaults Defaults => new() {
        Enabled = false,
        Color = new(254, 126, 229)
    };

    public override long GetRequirement(MaterialCategory category) => category switch {
        MaterialCategory.Basic => Config.Basic,
        MaterialCategory.Ore => Config.Ore,
        MaterialCategory.Furniture => Config.Furniture,
        MaterialCategory.Miscellaneous => Config.Miscellaneous,
        MaterialCategory.NonStackable => Config.NonStackable,
        _ => 0,
    };
    public sealed override long GetInfinity(int consumable, long count) => (long)(base.GetInfinity(consumable, count) * Config.Multiplier);

    protected override MaterialCategory GetCategoryInner(Item item) {
        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return MaterialCategory.Basic;

        if (!item.material) return MaterialCategory.None;

        if (item.maxStack == 1) return MaterialCategory.NonStackable;

        PlaceableCategory placeable = InfinityManager.GetCategory(item, Placeable.Instance);

        if (placeable.IsFurniture()) return MaterialCategory.Furniture;
        if (placeable == PlaceableCategory.Ore) return MaterialCategory.Ore;
        if (placeable.IsCommonTile()
                || type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow
                || type == ItemID.Wire || type == ItemID.BottledWater
                || type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
            return MaterialCategory.Basic;

        return MaterialCategory.Miscellaneous;
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Instance.Mod, "Material", Lang.tip[36].Value), TooltipLineID.Material);

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (Main.numAvailableRecipes == 0 || (Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked) || Main.InReforgeMenu || Main.LocalPlayer.tileEntityAnchor.InUse || Main.hidePlayerCraftingMenu) return;
        Recipe selectedRecipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
        Item? material = selectedRecipe.requiredItem.Find(i => i.IsSimilar(item));
        if (material is null) return;

        visibility = InfinityVisibility.Exclusive;
        long requirement = Math.Max(value.Requirement, (int)MathF.Ceiling(material.stack / Config.Multiplier));

        int group = selectedRecipe.acceptedGroups.FindIndex(g => RecipeGroup.recipeGroups[g].IconicItemId == consumable.type);
        if (group == -1) {
            value = value with { Requirement = requirement };
            return;
        }
        long count = PlayerHelper.OwnedItems[RecipeGroup.recipeGroups[selectedRecipe.acceptedGroups[0]].GetGroupFakeItemId()];
        value = value.For(count, requirement);
    }
}

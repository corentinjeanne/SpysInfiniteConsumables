using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Default.Components;

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

public sealed class Material : Infinity<Item, MaterialCategory> , IConfigurableComponents<MaterialRequirements> {
    public static Custom<Item, MaterialCategory> Custom = new(i => new(i.type));
    public override GroupInfinity<Item> Group => Consumable.Instance;
    public static Material Instance = null!;


    public override bool DefaultEnabled => false;
    public override Color DefaultColor => new(254, 126, 229, 255); // Nebula

    protected override Optional<Requirement> GetRequirement(MaterialCategory category) => category switch {
        MaterialCategory.Basic => new(InfinitySettings.Get(this).Basic, InfinitySettings.Get(this).Multiplier),
        MaterialCategory.Ore => new(InfinitySettings.Get(this).Ore, InfinitySettings.Get(this).Multiplier),
        MaterialCategory.Furniture => new(InfinitySettings.Get(this).Furniture, InfinitySettings.Get(this).Multiplier),
        MaterialCategory.Miscellaneous => new(InfinitySettings.Get(this).Miscellaneous, InfinitySettings.Get(this).Multiplier),
        MaterialCategory.NonStackable => new(InfinitySettings.Get(this).NonStackable, InfinitySettings.Get(this).Multiplier),
        _ => Requirement.None,
    };

    protected override Optional<MaterialCategory> GetCategory(Item item) {
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

    // public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Mod, "Material", Lang.tip[36].Value), TooltipLineID.Material);

    // public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
    //     if (Main.numAvailableRecipes == 0 || (Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked) || Main.InReforgeMenu || Main.LocalPlayer.tileEntityAnchor.InUse || Main.hidePlayerCraftingMenu) return;
    //     Recipe selectedRecipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
    //     Item? material = selectedRecipe.requiredItem.Find(i => i.IsSimilar(item));
    //     if (material is null) return;

    //     visibility = InfinityVisibility.Exclusive;
    //     requirement = requirement.ForInfinity(material.stack, 1);

    //     int group = selectedRecipe.acceptedGroups.FindIndex(g => RecipeGroup.recipeGroups[g].IconicItemId == item.type);
    //     if (group == -1) return;
    //     count = PlayerHelper.OwnedItems[RecipeGroup.recipeGroups[selectedRecipe.acceptedGroups[0]].GetGroupFakeItemId()];
    // }
}

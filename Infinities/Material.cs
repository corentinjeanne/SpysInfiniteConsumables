using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Reflection;

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
    [LabelKey($"${Localization.Keys.Infinities}.Material.Basic"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Basic = 999;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Ore"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Ore = 499;
    [LabelKey($"${Localization.Keys.Infinities}.Placeable.Furniture"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Furniture = 20;
    [LabelKey($"${Localization.Keys.Infinities}.Material.Miscellaneous"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count Miscellaneous = 50;
    [LabelKey($"${Localization.Keys.Infinities}.Material.NonStackable"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/2")]
    public Count NonStackable = 2;
}

public sealed class Material : InfinityStatic<Material, Items, Item, MaterialCategory> {
    
    public override int IconType => ItemID.TinkerersWorkshop;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.RarityPink;

    private static Dictionary<int, int> s_itemGroupCounts = null!;


    public override void Load() {
        base.Load();
        DisplayOverrides += CraftingMaterial;
    }
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        s_itemGroupCounts = (Dictionary<int, int>)typeof(Recipe).GetField("_ownedItems", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        Config = Group.AddConfig<MaterialRequirements>(this);
    }

    public override Requirement GetRequirement(MaterialCategory category) => category switch {
        MaterialCategory.Basic => new(Config.Value.Basic, 0.5f),
        MaterialCategory.Ore => new(Config.Value.Ore, 0.5f),
        MaterialCategory.Furniture => new(Config.Value.Furniture, 0.5f),
        MaterialCategory.Miscellaneous => new(Config.Value.Miscellaneous, 0.5f),
        MaterialCategory.NonStackable => new(Config.Value.NonStackable, 0.5f),
        _ => new(),
    };

    public override MaterialCategory GetCategory(Item item) {
        int type = item.type;
        switch (type) {
        case ItemID.FallenStar: return MaterialCategory.Miscellaneous;
        }
        if (item.IsACoin) return MaterialCategory.Basic;

        if (!item.material) return MaterialCategory.None;

        if (item.maxStack == 1) return MaterialCategory.NonStackable;

        PlaceableCategory placeable = Placeable.Instance.GetCategory(item);

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

    public static void CraftingMaterial(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        if (Main.numAvailableRecipes == 0 || (Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked) || Main.InReforgeMenu || Main.LocalPlayer.tileEntityAnchor.InUse || Main.hidePlayerCraftingMenu) return;
        Recipe selectedRecipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
        Item? material = selectedRecipe.requiredItem.Find(i => i.IsSimilar(item));
        if (material is null) return;

        visibility = InfinityVisibility.Exclusive;
        requirement = requirement.ForInfinity(material.stack, 1);

        int group = selectedRecipe.acceptedGroups.FindIndex(g => RecipeGroup.recipeGroups[g].IconicItemId == item.type);
        if (group == -1) return;
        count = s_itemGroupCounts[RecipeGroup.recipeGroups[selectedRecipe.acceptedGroups[0]].GetGroupFakeItemId()];

    }

    public static void CraftingMaterial(Item item, List<(IInfinity infinity, long consumed)> exclusiveGroups) {
        if (Main.numAvailableRecipes == 0) return;
        Item? material = Main.recipe[Main.availableRecipe[Main.focusRecipe]].requiredItem.Find(i => i.IsSimilar(item));
        if (material is not null) exclusiveGroups.Add((Instance, material.stack));
    }
}

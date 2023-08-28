using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;

namespace SPIC.Default.Globals;

// TODO remove recipes crafting a fully infinite item
public class InfiniteRecipe : ModSystem {

    public static readonly HashSet<int> CraftingStations = new();

    public override void PostAddRecipes() {
        CraftingStations.Clear();
        foreach (Recipe recipe in Main.recipe) {
            foreach (int t in recipe.requiredTile) {
                if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
            }

            recipe.AddConsumeItemCallback(OnItemConsume);
        }
    }

    public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
        if(CrossMod.MagicStorageIntegration.Enabled && CrossMod.MagicStorageIntegration.Version.CompareTo(new(0,5,7,9)) <= 0 && CrossMod.MagicStorageIntegration.InMagicStorage) return;
        if (Main.LocalPlayer.HasInfinite(type, amount, Material.Instance)) {
            amount = 0;
            return;
        }
        int group = recipe.acceptedGroups.FindIndex(g => RecipeGroup.recipeGroups[g].IconicItemId == type);
        if(group == -1) return;
        long total = 0;
        foreach (int groupItemType in RecipeGroup.recipeGroups[group].ValidItems) total += Main.LocalPlayer.GetInfinity(groupItemType, Material.Instance);
        if (total >= amount) amount = 0;
    }
}

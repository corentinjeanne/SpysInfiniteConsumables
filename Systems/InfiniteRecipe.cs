using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using SPIC.VanillaGroups;

namespace SPIC.Systems;

//TODO remove recipes crafting a fully infinite item
public class InfiniteRecipe : ModSystem {

    public static readonly HashSet<int> CraftingStations = new();

    public static long HighestCost(int type) => _hightestCost.ContainsKey(type) ? _hightestCost[type] : 0;


    public override void Load() {
        On.Terraria.Recipe.FindRecipes += HookRecipe_FindRecipes;
    }


    public override void PostAddRecipes() {
        CraftingStations.Clear();
        foreach (Recipe recipe in Main.recipe) {
            foreach (int t in recipe.requiredTile) {
                if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
            }

            recipe.AddConsumeItemCallback(OnItemConsume);

            foreach (Item mat in recipe.requiredItem) {
                if (!_hightestCost.ContainsKey(mat.type) || _hightestCost[mat.type] < mat.stack) _hightestCost[mat.type] = mat.stack;
            }
        }
    }


    public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
        if(CrossMod.MagicStorageIntegration.InMagicStorage && recipe.requiredItem[0].type == type) return;
        if (Main.LocalPlayer.HasInfinite(new(type), amount, Material.Instance)) {
            amount = 0;
            return;
        }
        foreach (int g in recipe.acceptedGroups) {
            if (RecipeGroup.recipeGroups[g].ContainsItem(type)) {
                foreach (int groupItemType in RecipeGroup.recipeGroups[g].ValidItems) {
                    if (Main.LocalPlayer.HasInfinite(new(groupItemType), amount, Material.Instance)) {
                        amount = 0;
                        return;
                    }
                }
            }
        }
    }


    private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
        if (canDelayCheck) {
            orig(canDelayCheck);
            return;
        }
        InfinityManager.ClearCache();
        orig(canDelayCheck);
    }
    
    
    private static readonly Dictionary<int, int> _hightestCost = new();
}

using System.Collections.Generic;
using SPIC.ConsumableTypes;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.Systems {

    //TODO remove recipes crafting a fully infinite item
    public class InfiniteRecipe : ModSystem {

        public static readonly HashSet<int> CraftingStations = new();
        private static readonly Dictionary<int, int> _hightestCost = new();
        public static long HighestCost(int type) => _hightestCost.ContainsKey(type) ? _hightestCost[type] : 0;
        
        public override void Load() {
            On.Terraria.Recipe.FindRecipes += HookRecipe_FindRecipes;
        }

        
        public override void PostAddRecipes() {
            CraftingStations.Clear();
            for (int i = 0; i < Main.recipe.Length; i++) {
                Recipe r = Main.recipe[i];
                foreach (int t in r.requiredTile) 
                    if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
                r.AddConsumeItemCallback(OnItemConsume);
                // r.AddCondition(CanCraft);
            }
            CalculateHighestCosts();
        }

        private static void CalculateHighestCosts(){
            foreach(Recipe recipe in Main.recipe){
                foreach (Item mat in recipe.requiredItem){
                    if(!_hightestCost.ContainsKey(mat.type) || _hightestCost[mat.type] < mat.stack) _hightestCost[mat.type] = mat.stack;
                }
            }
        }

        // public static readonly Recipe.Condition CanCraft = new(Terraria.Localization.NetworkText.Empty,
        //     recipe => {
        //         return !(Configs.Requirements.Instance.PreventItemDupication && infinityPlayer.HasFullyInfinite(recipe.createItem));
        //     }
        // );
        
        public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
            // Globals.InfinityPlayer infinityPlayer = Main.LocalPlayer.GetModPlayer<Globals.InfinityPlayer>();
            if (Main.LocalPlayer.HasInfinite(type, amount, Material.ID)) {
                amount = 0;
                return;
            }
            foreach (int g in recipe.acceptedGroups) {
                if (RecipeGroup.recipeGroups[g].ContainsItem(type)) {
                    foreach (int groupItemType in RecipeGroup.recipeGroups[g].ValidItems) {
                        if (Main.LocalPlayer.HasInfinite(groupItemType, amount, Material.ID)) {
                            amount = 0;
                            return;
                        }
                    }
                }
            }
        }

        private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
            SpysInfiniteConsumables.Instance.Logger.Debug("Find recipes");
            if (canDelayCheck) {
                orig(canDelayCheck);
                return;
            }
            InfinityManager.ClearCache();
            orig(canDelayCheck);
        }
    }

}
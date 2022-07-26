using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace SPIC.Systems {

    //TODO remove recipies crafting a fully infinite item
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
        public static readonly Recipe.Condition CanCraft = new(Terraria.Localization.NetworkText.Empty,
            recipe => {
                Globals.InfinityPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<Globals.InfinityPlayer>();
                return !(Configs.Requirements.Instance.PreventItemDupication && spicPlayer.HasFullyInfinite(recipe.createItem));
            }
        );
        
        public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
            if (!Configs.Requirements.Instance.InfiniteMaterials) return;

            Globals.InfinityPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<Globals.InfinityPlayer>();
            if (amount <= spicPlayer.GetTypeInfinities(type).Material) amount = 0;
            else {
                foreach (RecipeGroup group in RecipeGroup.recipeGroups.Values){
                    if(group.ContainsItem(type)){
                        foreach (int groupItemType in group.ValidItems){
                            if(amount <= spicPlayer.GetTypeInfinities(groupItemType).Material) amount = 0;
                        }
                    }
                }
            }
        }

        private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
            orig(canDelayCheck);
            if (canDelayCheck) return;

            CategoryManager.ClearAll();
            Main.LocalPlayer.GetModPlayer<Globals.InfinityPlayer>().ClearInfinities();
        }
    }

}
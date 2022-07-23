using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SPIC.Globals {

    public class InfiniteRecipe : ModSystem {

        public static readonly HashSet<int> CraftingStations = new();      
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
            }
        }
        // public static readonly Recipe.Condition CanCraft = new(Terraria.Localization.NetworkText.FromKey("Mods.SPIC.Recipe.InfiniteCraft"),
        //     recipe => {
        //         SpicPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<SpicPlayer>();
        //         if(!spicPlayer.HasFullyInfinite(recipe.createItem)) return true;
        //         int matInfinity = spicPlayer.GetInfinities(recipe.createItem).Material;
        //         if(1 > matInfinity) return true;
        //         foreach (int r in Main.availableRecipe) {
        //             if(r == 0) break;
        //             Recipe available = Main.recipe[r];
        //             Item amount = null;
        //             if ((amount = available.requiredItem.Find(i => i.type == recipe.createItem.type)) != null && amount.stack > matInfinity) return true;
        //         }
        //         return false;
        //     }
        // );
        
        public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
            if (!Configs.Requirements.Instance.InfiniteMaterials) return;

            SpicPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<SpicPlayer>();
            if (amount <= spicPlayer.GetInfinities(type).Material) amount = 0;
        }

        private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
            orig(canDelayCheck);
            if (canDelayCheck) return;
            RemoveInfiniteRecipies();

            CategoryHelper.ClearAll();
            Main.LocalPlayer.GetModPlayer<SpicPlayer>().ClearInfinities();
        }
        public static void RemoveInfiniteRecipies(){
            
            Dictionary<int, int> highestCost = new();
            foreach (int r in Main.availableRecipe) {
                if (r == 0) break;
                Recipe recipe = Main.recipe[r];
                foreach(Item item in recipe.requiredItem){
                    if(!highestCost.ContainsKey(item.type) || highestCost[item.type] < item.stack)
                        highestCost[item.type] = item.stack;
                }
            }

            SpicPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<SpicPlayer>();
            for (int i = 0; i < Main.availableRecipe.Length && Main.availableRecipe[i] != 0; i++) {
                Recipe recipe = Main.recipe[Main.availableRecipe[i]];
                if(!spicPlayer.HasFullyInfinite(recipe.createItem)) continue;
                int matInfinity = spicPlayer.GetInfinities(recipe.createItem).Material;
                if(highestCost.TryGetValue(recipe.createItem.type, out int cost) && cost > matInfinity) continue;
                Main.availableRecipe[i] = 0;
            }
        }
    }

}
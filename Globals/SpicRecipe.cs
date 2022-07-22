using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SPIC.Globals {

    public class InfiniteRecipe : ModSystem {

        public static readonly HashSet<int> CraftingStations = new();      
        public override void PostAddRecipes() {
            CraftingStations.Clear();
            for (int i = 0; i < Main.recipe.Length; i++) {
                Recipe r = Main.recipe[i];
                foreach (int t in r.requiredTile) 
                    if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
                r.AddConsumeItemCallback(OnItemConsume);
            }
        }
        public override void Load() {
            On.Terraria.Recipe.FindRecipes += HookRecipe_FindRecipes;
        }
        
        public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
            if (!Configs.Requirements.Instance.InfiniteMaterials) return;

            SpicPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<SpicPlayer>();
            if (amount <= spicPlayer.GetInfinities(type).Material) amount = 0;
        }

        public static readonly Recipe.Condition CanCraft = new(Terraria.Localization.NetworkText.FromKey("Mods.SPIC.Recipe.InfiniteCraft"), i => true);

        private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
            orig(canDelayCheck);
            if (canDelayCheck) return;

            CategoryHelper.ClearAll();
            Main.LocalPlayer.GetModPlayer<SpicPlayer>().ClearInfinities();
        }
    }

}
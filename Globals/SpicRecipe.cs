using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace SPIC.Globals {

    public class SpicRecipe : GlobalRecipe {

        public static readonly HashSet<int> CraftingStations = new();
        public override void Load() {
            On.Terraria.Recipe.FindRecipes += HookRecipe_FindRecipes;
        }
        public override void Unload(){
            CraftingStations.Clear();
        }
        
        public override void SetStaticDefaults() {
            CraftingStations.Clear();
            foreach(Recipe r in Main.recipe) {
                foreach(int t in r.requiredTile) {
                    if (!CraftingStations.Contains(t)) CraftingStations.Add(t);
                }
            }
        }

        public override void ConsumeItem(Recipe recipe, int type, ref int amount) {
            if (!Configs.Infinities.Instance.InfiniteCrafting) return;

            SpicPlayer spicPlayer = Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>();
            if (spicPlayer.HasInfiniteMaterial(type)) amount = 0;
        }

        private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
            orig(canDelayCheck);
            if (canDelayCheck) return;
            Category.ClearAll();
            Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>().FindInfinities();
        }

    }

}
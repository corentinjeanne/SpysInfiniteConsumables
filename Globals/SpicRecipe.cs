﻿using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SPIC.Globals {

    public class SpicRecipe : GlobalRecipe {

        public static readonly HashSet<int> CraftingStations = new();
        public override void Load() {
            On.Terraria.Recipe.FindRecipes += HookRecipe_FindRecipes;
            CraftingStations.Clear();
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
            if (!Configs.Requirements.Instance.InfiniteMaterials) return;

            SpicPlayer spicPlayer = Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>();
            if (spicPlayer.GetInfinities(type).Material <= amount) amount = 0;
        }

        private static void HookRecipe_FindRecipes(On.Terraria.Recipe.orig_FindRecipes orig, bool canDelayCheck) {
            orig(canDelayCheck);
            if (canDelayCheck) return;

            CategoryHelper.ClearAll();
            Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>().FindInfinities();

            // Configs.Infinities infinities = Configs.Infinities.Instance;
            // if(infinities.InfiniteMaterials && infinities.PreventItemDupication){
            //     for (int r = 0; r < Recipe.maxRecipes && Main.recipe[r].createItem.type != ItemID.None; r++) {
            //         if (Main.availableRecipe[r] == 0) continue;
            //         if (Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>().HasInfiniteMaterial(Main.recipe[Main.availableRecipe[r]].createItem.type))
            //             Main.availableRecipe[r] = 0;
            //     }
            // }

        }
    }

}
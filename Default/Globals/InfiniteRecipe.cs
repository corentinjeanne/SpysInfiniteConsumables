using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using SpikysLib.CrossMod;
using SpikysLib.Extensions;

namespace SPIC.Default.Globals;

public class InfiniteRecipe : ModSystem {

    public static readonly HashSet<int> CraftingStations = new();

    public override void PostAddRecipes() {
        CraftingStations.Clear();
        foreach (Recipe recipe in Main.recipe) {
            foreach (int t in recipe.requiredTile) CraftingStations.Add(t);
            recipe.AddConsumeItemCallback(OnItemConsume);
        }
    }

    public static void OnItemConsume(Recipe recipe, int type, ref int amount) {
        if(MagicStorageIntegration.Enabled && MagicStorageIntegration.Version.CompareTo(new(0,5,7,9)) <= 0 && MagicStorageIntegration.InMagicStorage) return;
        if (Main.LocalPlayer.HasInfinite(type, amount, Material.Instance)) {
            amount = 0;
            return;
        }
        int g = recipe.acceptedGroups.FindIndex(g => RecipeGroup.recipeGroups[g].IconicItemId == type);
        if(g == -1) return;
        long total = PlayerExtensions.OwnedItems.GetValueOrDefault(RecipeGroup.recipeGroups[recipe.acceptedGroups[g]].GetGroupFakeItemId(), 0);
        if (InfinityManager.GetInfinity(type, total, Material.Instance) >= amount) amount = 0;
    }
}

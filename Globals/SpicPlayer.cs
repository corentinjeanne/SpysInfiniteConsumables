using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Globals;

public sealed class SpicPlayer : ModPlayer {

    public override void Load() => On_Recipe.FindRecipes += HookRecipe_FindRecipes;

    public override void OnEnterWorld() {
        string version = Configs.InfinityDisplay.Instance.version;

        if(version.Length == 0) Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Download", Mod.Version.ToString()), Colors.RarityCyan);
        else if(Mod.Version > new System.Version(version)) Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Update", Mod.Version.ToString()), Colors.RarityCyan);
        else return;

        Configs.InfinityDisplay.Instance.version = Mod.Version.ToString();
        Configs.InfinityDisplay.Instance.SaveConfig();
    }

    private static void HookRecipe_FindRecipes(On_Recipe.orig_FindRecipes orig, bool canDelayCheck) {
        if (canDelayCheck) {
            orig(canDelayCheck);
            return;
        }
        InfinityManager.ClearInfinities();
        orig(canDelayCheck);
    }

    public override void PreUpdate() => InfinityManager.DecreaseCacheLock();
}
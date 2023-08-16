using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Globals;

public sealed class SpicPlayer : ModPlayer {

    public override void Load() => On_Recipe.FindRecipes += HookRecipe_FindRecipes;

    public override void OnEnterWorld() {
        string version = Configs.InfinityDisplay.Instance.version;
        bool updated = version.Length != 0 && Mod.Version > new System.Version(version);

        if (Configs.InfinityDisplay.Instance.WelcomeMessage == Configs.WelcomMessageFrequency.Always
                || (Configs.InfinityDisplay.Instance.WelcomeMessage == Configs.WelcomMessageFrequency.OncePerUpdate && updated)) {
            Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Welcome", Mod.Version.ToString()), Colors.RarityCyan);
            Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Message"), Colors.RarityCyan);
            if (updated) {
                Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Changelog", version), Colors.RarityCyan);
                for (int i = System.Array.IndexOf(SpysInfiniteConsumables.Versions, version) + 1; i < SpysInfiniteConsumables.Versions.Length; i++)
                    Main.NewText(Language.GetTextValue($"{Localization.Keys.Changelog}.{SpysInfiniteConsumables.Versions[i]}"), Colors.RarityCyan);
            }
        }
        if (updated) {
            Configs.InfinityDisplay.Instance.version = Mod.Version.ToString();
            Configs.InfinityDisplay.Instance.SaveConfig();
        }
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
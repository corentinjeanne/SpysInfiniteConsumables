using System;
using SpikysLib.Configs;
using SpikysLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace SPIC.Globals;

public sealed class SpicPlayer : ModPlayer {

    public override void Load() => On_Recipe.FindRecipes += HookRecipe_FindRecipes;

    public override void OnEnterWorld() {
        DisplayUpdate();
    }

    public void DisplayUpdate() {
        LocalizedLine line;
        if (Configs.Version.Instance.lastPlayedVersion.Length == 0) line = new(Language.GetText($"{Localization.Keys.Chat}.Download"));
        else if (Mod.Version > new Version(Configs.Version.Instance.lastPlayedVersion)) line = new(Language.GetText($"{Localization.Keys.Chat}.Update"));
        else return;
        Configs.Version.Instance.lastPlayedVersion = Mod.Version.ToString();
        Configs.Version.Instance.Save();

        InGameNotificationsTracker.AddNotification(new InGameNotification(Mod, line, new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Bug"), Colors.RarityAmber)) { timeLeft = 15 * 60 });
    }

    private static void HookRecipe_FindRecipes(On_Recipe.orig_FindRecipes orig, bool canDelayCheck) {
        if (canDelayCheck) {
            orig(canDelayCheck);
            return;
        }
        InfinityManager.ClearEndpoints();
        orig(canDelayCheck);
    }
}
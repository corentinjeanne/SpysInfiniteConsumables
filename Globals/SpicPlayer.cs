using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SpikysLib.Extensions;
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
        bool download;
        if (Configs.InfinityDisplay.Instance.version.Length == 0) download = true;
        else if (Mod.Version > new System.Version(Configs.InfinityDisplay.Instance.version)) download = false;
        else return;

        List<ITextLine> lines = new();
        if (download) lines.Add(new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Download")));
        else lines.Add(new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Update")));
        lines.Add(new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Bug")));
        InGameNotificationsTracker.AddNotification(new InGameNotification(ModContent.Request<Texture2D>($"SPIC/icon"), lines.ToArray()) { timeLeft = 15 * 60 });

        Configs.InfinityDisplay.Instance.version = Mod.Version.ToString();
        Configs.InfinityDisplay.Instance.Save();
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
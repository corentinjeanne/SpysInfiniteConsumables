using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
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
        bool download;
        if (Configs.InfinityDisplay.Instance.version.Length == 0) download = true;
        else if (Mod.Version > new System.Version(Configs.InfinityDisplay.Instance.version)) download = false;
        else return;

        List<ITextLine> lines = [
            download ? new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Download")) : new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Update")),
            new LocalizedLine(Language.GetText($"{Localization.Keys.Chat}.Bug")),
        ];
        LocalizedLine important = new(Language.GetText($"{Localization.Keys.Chat}.Important"), Colors.RarityAmber);
        if( important.Value.Length != 0) lines.Add(important);
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
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SPIC.VanillaGroups;

namespace SPIC.Globals;

public class DetectionPlayer : ModPlayer {

    public bool InItemCheck { get; private set; }

    public bool DetectingCategory { get;  private set;}

    private int _preUseMaxLife, _preUseMaxMana;
    private int _preUseExtraAccessories;
    private Microsoft.Xna.Framework.Vector2 _preUsePosition;
    private bool _preUseDemonHeart;
    private int _preUseDifficulty;
    private int _preUseInvasion;
    private int _preUseItemCount;
    private static NPCStats _preUseNPCStats;

    public bool teleport;



    public override void Load() {
        On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        On.Terraria.Player.Teleport += HookTeleport;
        On.Terraria.Player.Spawn += HookSpawn;
    }

    public override void PreUpdate() {
        InfinityDisplayItem.IncrementCounters();
    }

    public override bool PreItemCheck() {
        if (Config.CategoryDetection.Instance.DetectMissing && Player.HeldItem.GetCategory(Usable.Instance) == UsableCategory.Unknown) {
            DetectingCategory = true;
            SavePreUseStats();
        } else DetectingCategory = false;

        InItemCheck = true;
        return true;
    }
    public override void PostItemCheck() {
        InItemCheck = false;
        if (DetectingCategory) TryDetectCategory();
    }

    private void SavePreUseStats() {
        teleport = false;
        _preUseMaxLife = Player.statLifeMax2;
        _preUseMaxMana = Player.statManaMax2;
        _preUseExtraAccessories = Player.extraAccessorySlots;
        _preUseDemonHeart = Player.extraAccessory;
        _preUsePosition = Player.position;

        _preUseDifficulty = Utility.WorldDifficulty();
        _preUseInvasion = Main.invasionType;
        _preUseNPCStats = Utility.GetNPCStats();
        _preUseItemCount = Utility.CountItemsInWorld();
    }

    public void TryDetectCategory(bool mustDetect = false) {
        if (!DetectingCategory) return;

        void SaveUsable(UsableCategory category)
            => Config.CategoryDetection.Instance.SaveDetectedCategory(Player.HeldItem, category, Usable.Instance);

        void SaveBag() {
            Config.CategoryDetection.Instance.SaveDetectedCategory(Player.HeldItem, GrabBagCategory.Crate, GrabBag.Instance);
            Config.CategoryDetection.Instance.SaveDetectedCategory(Player.HeldItem, UsableCategory.None, Usable.Instance);
        }

        UsableCategory usable = TryDetectUsable();
        GrabBagCategory bag = TryDetectGrabBag();

        if (usable != UsableCategory.Unknown) SaveUsable(usable);
        else if (bag != GrabBagCategory.Unknown) SaveBag();
        else if (mustDetect) SaveUsable(UsableCategory.PlayerBooster);
        else return;

        DetectingCategory = false;
    }

    private UsableCategory TryDetectUsable() {
        NPCStats stats = Utility.GetNPCStats();

        if(teleport) return UsableCategory.Tool;
        if (_preUseNPCStats.Boss != stats.Boss || _preUseInvasion != Main.invasionType)
            return UsableCategory.Summoner;

        if (_preUseNPCStats.Total != stats.Total)
            return UsableCategory.Critter;

        if (_preUseMaxLife != Player.statLifeMax2 || _preUseMaxMana != Player.statManaMax2
                || _preUseExtraAccessories != Player.extraAccessorySlots || _preUseDemonHeart != Player.extraAccessory)
            return UsableCategory.PlayerBooster;

        // ? Other difficulties
        if (_preUseDifficulty != Utility.WorldDifficulty())
            return UsableCategory.WorldBooster;

        if (Player.position != _preUsePosition)
            return UsableCategory.Tool;

        return UsableCategory.Unknown;
    }

    private GrabBagCategory TryDetectGrabBag() {
        if (Utility.CountItemsInWorld() != _preUseItemCount) return GrabBagCategory.Crate;
        return GrabBagCategory.Unknown;
    }

    public int FindPotentialExplosivesType(int proj) {
        foreach (Dictionary<int, int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values) {
            foreach ((int t, int p) in projectiles) if (p == proj) return t;
        }
        foreach (Item item in Player.inventory) if (item.shoot == proj) return item.type;

        return 0;
    }

    public void RefilExplosive(int proj, Item refill) {
        int tot = Player.CountItems(refill.type);
        int used = 0;
        foreach (Projectile p in Main.projectile)
            if (p.owner == Player.whoAmI && p.type == proj) used += 1;

        Config.RequirementSettings requirements = Config.RequirementSettings.Instance;
        if ((refill.GetCategory(Usable.Instance) == UsableCategory.Explosive && !Player.HasInfinite(refill, 1, Usable.Instance) && !refill.GetInfinity(tot + used, Usable.Instance).Value.IsNone)
                || (refill.GetCategory(Ammo.Instance) == AmmoCategory.Explosive && !refill.GetInfinity(tot + used, Ammo.Instance).Value.IsNone)
            )
            Player.GetItem(Player.whoAmI, new(refill.type, used), new(NoText: true));

    }

    private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
        if (selItem < 0) goto origin;

        Item item = self.inventory[selItem];

        Config.CategoryDetection autos = Config.CategoryDetection.Instance;
        Config.RequirementSettings settings = Config.RequirementSettings.Instance;


        if (autos.DetectMissing && item.GetCategory(Placeable.Instance) == PlaceableCategory.None)
            autos.SaveDetectedCategory(item, PlaceableCategory.Liquid, Placeable.Instance);

        item.stack++;
        if (!self.HasInfinite(item, 1, Placeable.Instance)) {
            item.stack--;
        } else {
            if (settings.PreventItemDupication) return;
        }
    origin: orig(self, type, selItem);
    }

    private static void HookSpawn(On.Terraria.Player.orig_Spawn orig, Player self, PlayerSpawnContext context) {
        DetectionPlayer player = self.GetModPlayer<DetectionPlayer>();
        if(player.InItemCheck && player.DetectingCategory) player.teleport = true;
        orig(self, context);
    }

    private static void HookTeleport(On.Terraria.Player.orig_Teleport orig, Player self, Microsoft.Xna.Framework.Vector2 newPos, int Style = 0, int extraInfo = 0) {
        DetectionPlayer player = self.GetModPlayer<DetectionPlayer>();
        if(player.InItemCheck && player.DetectingCategory) player.teleport = true;
        orig(self, newPos, Style, extraInfo);
    }
}
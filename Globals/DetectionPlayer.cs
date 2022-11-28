using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SPIC.VanillaConsumableTypes;
using SPIC.ConsumableGroup;

namespace SPIC.Globals;

public class DetectionPlayer : ModPlayer {

    public bool InItemCheck { get; private set; }

    private bool _detectingCategory;

    private int _preUseMaxLife, _preUseMaxMana;
    private int _preUseExtraAccessories;
    private Microsoft.Xna.Framework.Vector2 _preUsePosition;
    private bool _preUseDemonHeart;
    private int _preUseDifficulty;
    private int _preUseInvasion;
    private int _preUseItemCount;
    private static Utility.NPCStats _preUseNPCStats;



    public override void Load() {
        On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
    }

    public override void PreUpdate() {
        InfinityDisplayItem.IncrementCounters();
    }

    public override bool PreItemCheck() {
        if (Config.CategoryDetection.Instance.DetectMissing && Player.HeldItem.GetCategory<UsableCategory>(Usable.ID) == UsableCategory.Unknown) {
            SavePreUseItemStats();
            _detectingCategory = true;
        } else _detectingCategory = false;

        InItemCheck = true;
        return true;
    }
    public override void PostItemCheck() {
        InItemCheck = false;
        if (_detectingCategory) TryDetectCategory();
    }

    private void SavePreUseItemStats() {
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

    // BUG recall when at spawn : use hook on teleport funcs ?
    public void TryDetectCategory(bool mustDetect = false) {
        if (!_detectingCategory) return;

        void SaveUsable(UsableCategory category)
            => Config.CategoryDetection.Instance.SaveDetectedCategory(Player.HeldItem, category, Usable.ID);

        void SaveBag() {
            Config.CategoryDetection.Instance.SaveDetectedCategory(Player.HeldItem, GrabBagCategory.Crate, GrabBag.ID);
            Config.CategoryDetection.Instance.SaveDetectedCategory(Player.HeldItem, UsableCategory.None, Usable.ID);
        }

        UsableCategory usable = TryDetectUsable();
        GrabBagCategory bag = TryDetectGrabBag();

        if (usable != UsableCategory.Unknown) SaveUsable(usable);
        else if (bag != GrabBagCategory.Unknown) SaveBag();
        else if (mustDetect) SaveUsable(UsableCategory.PlayerBooster);
        else return;

        InfinityManager.ClearCache(Player.HeldItem);
        _detectingCategory = false;
    }

    private UsableCategory TryDetectUsable() {
        Utility.NPCStats stats = Utility.GetNPCStats();
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
        if ((refill.GetCategory<UsableCategory>(Usable.ID) == UsableCategory.Explosive && !Player.HasInfinite(refill, 1, Usable.ID) && !refill.GetInfinity(tot + used, Usable.ID).Value.IsNone)
                || (refill.GetCategory<AmmoCategory>(Ammo.ID) == AmmoCategory.Explosive && !refill.GetInfinity(tot + used, Ammo.ID).Value.IsNone)
            )
            Player.GetItem(Player.whoAmI, new(refill.type, used), new(NoText: true));

    }

    private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
        if (selItem < 0) goto origin;

        Item item = self.inventory[selItem];

        Config.CategoryDetection autos = Config.CategoryDetection.Instance;
        Config.RequirementSettings settings = Config.RequirementSettings.Instance;


        if (autos.DetectMissing && item.GetCategory<PlaceableCategory>(Placeable.ID) == PlaceableCategory.None)
            autos.SaveDetectedCategory(item, PlaceableCategory.Liquid, Placeable.ID);

        InfinityManager.ClearCache(item);

        item.stack++;
        if (!self.HasInfinite(item, 1, Placeable.ID)) {
            item.stack--;
        } else {
            if (settings.PreventItemDupication) return;
        }
    origin: orig(self, type, selItem);
    }
}
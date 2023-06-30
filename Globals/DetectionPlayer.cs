using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SPIC.VanillaGroups;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace SPIC.Globals;


public class DetectionPlayer : ModPlayer {

    public bool InItemCheck { get; private set; }
    public static bool InRightClick { get; private set; }


    public override void Load() {
        On_Player.ItemCheck_Inner += HookItemCheck_Inner;
        // Terraria.UI.On_ItemSlot.RightClick_FindSpecialActions += HookRightClick_Inner;
        On_Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        On_Player.Teleport += HookTeleport;
        On_Player.Spawn += HookSpawn;
    }

    private static void HookItemCheck_Inner(On_Player.orig_ItemCheck_Inner orig, Player self) {
        DetectionPlayer detectionPlayer = self.GetModPlayer<DetectionPlayer>();
        detectionPlayer.InItemCheck = true;
        detectionPlayer.DetectingCategoryOf = null;

        if ((self.itemAnimation > 0 || !self.JustDroppedAnItem && self.ItemTimeIsZero)
                && Configs.CategoryDetection.Instance.DetectMissing && self.HeldItem.GetCategory(Usable.Instance) == UsableCategory.Unknown)
            detectionPlayer.PrepareDetection(self.HeldItem, true);
        orig(self);
        if (detectionPlayer.DetectingCategoryOf is not null) detectionPlayer.TryDetectCategory();
        detectionPlayer.InItemCheck = false;
    }

    public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) {
        if(price > 0 && Player.HasInfinite(CurrencyHelper.Coins, price, Currency.Instance)) price = 1;
    }
    public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) {
        if(price == 1) Player.GetItem(Player.whoAmI, new(ItemID.CopperCoin), new(NoText: true));
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) => InfinityManager.ClearCache(item);


    public override void OnEnterWorld(){
        string version = Configs.InfinityDisplay.Instance.general_lastLogs;
        if(version == "") version = Mod.Version.ToString() == "2.2.1" ? SpysInfiniteConsumables.Versions[^2] : SpysInfiniteConsumables.Versions[^1];
        bool newChanges = Mod.Version > new System.Version(version);

        if (Configs.InfinityDisplay.Instance.general_welcomeMessage == Configs.InfinityDisplay.WelcomMessageFrequency.Always
                || (Configs.InfinityDisplay.Instance.general_welcomeMessage == Configs.InfinityDisplay.WelcomMessageFrequency.OncePerUpdate && newChanges)) {
            Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Welcome", Mod.Version.ToString()), Colors.RarityCyan);
            Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Message"), Colors.RarityCyan);
            if (newChanges) {
                Main.NewText(Language.GetTextValue($"{Localization.Keys.Chat}.Changelog", version), Colors.RarityCyan);
                for (int i = System.Array.IndexOf(SpysInfiniteConsumables.Versions, version)+1; i < SpysInfiniteConsumables.Versions.Length; i++)
                    Main.NewText(Language.GetTextValue($"{Localization.Keys.Changelog}.{SpysInfiniteConsumables.Versions[i]}"), Colors.RarityCyan);
            }
            version = Mod.Version.ToString();
        }
        if (Configs.InfinityDisplay.Instance.general_lastLogs != version) {
            Configs.InfinityDisplay.Instance.general_lastLogs = version;
            Configs.InfinityDisplay.Instance.SaveConfig();
        }
    }

    public override void PreUpdate() => InfinityDisplayItem.IncrementCounters();

    public void PrepareDetection(Item item, bool consumable){
        DetectingCategoryOf = item;
        _teleport = false;
        _preUseData = GetDetectionData();
        _detectingConsumable = consumable;
    }

    public DetectionDataScreenShot GetDetectionData() => new(
        Player.statLifeMax2, Player.statManaMax2,
        Player.position,
        Player.extraAccessorySlots, Player.extraAccessory,
        Utility.CountProjectilesInWorld(), Utility.CountItemsInWorld(),
        Utility.WorldDifficulty(), Main.invasionType, Utility.GetNPCStats()
    );


    public bool TryDetectCategory(bool mustDetect = false) {
        if (DetectingCategoryOf is null) return false;

        void SaveUsable(UsableCategory category) {
            Configs.CategoryDetection.Instance.SaveDetectedCategory(DetectingCategoryOf, category, Usable.Instance);
            if(!_detectingConsumable) Configs.CategoryDetection.Instance.SaveDetectedCategory(DetectingCategoryOf, GrabBagCategory.None, GrabBag.Instance);
        }
        
        // void SaveBag(GrabBagCategory category) {
        //     Configs.CategoryDetection.Instance.SaveDetectedCategory(DetectingCategoryOf, category, GrabBag.Instance);
        //     if (_detectingConsumable) Configs.CategoryDetection.Instance.SaveDetectedCategory(DetectingCategoryOf, UsableCategory.None, Usable.Instance);
        // }

        DetectionDataScreenShot data = GetDetectionData();

        if (TryDetectUsable(data, out UsableCategory usable)) SaveUsable(usable);
        // else if (TryDetectGrabBag(data, out GrabBagCategory bag)) SaveBag(bag);
        else if (mustDetect && _detectingConsumable) SaveUsable(UsableCategory.PlayerBooster); // BUG consumed when detected (reproduce)
        else return false;
        DetectingCategoryOf = null;

        return true;
    }
    private bool TryDetectUsable(DetectionDataScreenShot data, out UsableCategory category) {

        if(data.Projectiles != _preUseData.Projectiles) category = UsableCategory.Tool;

        else if (data.NPCStats.Boss != _preUseData.NPCStats.Boss || data.Invasion != _preUseData.Invasion) category = UsableCategory.Summoner;
        else if (data.NPCStats.Total != _preUseData.NPCStats.Total) category = UsableCategory.Critter;

        else if (data.MaxLife != _preUseData.MaxLife || data.MaxMana != _preUseData.MaxMana || data.ExtraAccessories != _preUseData.ExtraAccessories || data.DemonHeart != _preUseData.DemonHeart) category = UsableCategory.PlayerBooster;
        else if (data.Difficulty != _preUseData.Difficulty) category = UsableCategory.WorldBooster;

        else if (_teleport || data.Position != _preUseData.Position) category = UsableCategory.Recovery;

        else category = UsableCategory.Unknown;
        return category != UsableCategory.Unknown;
    }

    // private bool TryDetectGrabBag(DetectionDataScreenShot data, out GrabBagCategory category) {
    //     category = data.ItemCount != _preUseData.ItemCount ? GrabBagCategory.Container : GrabBagCategory.Unknown;
    //     return category != GrabBagCategory.Unknown;
    // }


    public int FindPotentialExplosivesType(int proj) {
        foreach (Dictionary<int, int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values) {
            foreach ((int item, int shoot) in projectiles) if (shoot == proj) return item;
        }
        foreach (Item item in Player.inventory) if (item.shoot == proj) return item.type;
        return ItemID.None;
    }

    public void RefilExplosive(int projType, Item refill) {
        int owned = Player.CountItems(refill.type);
        int used = 0;
        foreach (Projectile proj in Main.projectile)
            if (proj.owner == Player.whoAmI && proj.type == projType) used += 1;

        if ((refill.GetCategory(Usable.Instance) == UsableCategory.Explosive && !refill.GetInfinity(owned + used, Usable.Instance).Value.IsNone)
                || (refill.GetCategory(Ammo.Instance) == AmmoCategory.Explosive && !refill.GetInfinity(owned + used, Ammo.Instance).Value.IsNone))
            Player.GetItem(Player.whoAmI, new(refill.type, used), new(NoText: true));
    }


    public void Teleported() => _teleport = true;


    // private bool HookRightClick_Inner(Terraria.UI.On_ItemSlot.orig_RightClick_FindSpecialActions orig, Item[] inv, int context, int slot, Player player) {
    //     DetectingCategoryOf = null;
    //     if (!Main.mouseRight || !Main.mouseRightRelease) return orig(inv, context, slot, player);
    //     InRightClick = true;
    //     DetectionPlayer modPlayer = player.GetModPlayer<DetectionPlayer>();
    //     if (Configs.CategoryDetection.Instance.DetectMissing && inv[slot].type != ItemID.None && inv[slot].GetCategory(GrabBag.Instance) == GrabBagCategory.Unknown)
    //         modPlayer.PrepareDetection(inv[slot], false);

    //     bool res = orig(inv, context, slot, player);
    //     if (modPlayer.DetectingCategoryOf is not null)
    //         modPlayer.TryDetectCategory();
    //     InRightClick = false;
    //     return res;
    // }

    private static void HookPutItemInInventory(On_Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
        if (selItem < 0){
            orig(self, type, selItem);
            return;
        }

        Item item = self.inventory[selItem];

        Configs.CategoryDetection detection = Configs.CategoryDetection.Instance;
        Configs.GroupSettings settings = Configs.GroupSettings.Instance;

        if (detection.DetectMissing && item.GetCategory(Placeable.Instance) == PlaceableCategory.None) detection.SaveDetectedCategory(item, PlaceableCategory.Liquid, Placeable.Instance);

        item.stack++;
        if (!self.HasInfinite(item, 1, Placeable.Instance)) item.stack--;
        else if (settings.PreventItemDupication) return;

        orig(self, type, selItem);
    }

    private static void HookSpawn(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context) {
        orig(self, context);
        self.GetModPlayer<DetectionPlayer>().Teleported();
    }
    private static void HookTeleport(On_Player.orig_Teleport orig, Player self, Vector2 newPos, int Style = 0, int extraInfo = 0) {
        orig(self, newPos, Style, extraInfo);
        self.GetModPlayer<DetectionPlayer>().Teleported();
    }


    public Item? DetectingCategoryOf;
    private bool _detectingConsumable;
    private DetectionDataScreenShot _preUseData;

    private bool _teleport;
}

public record struct DetectionDataScreenShot(int MaxLife, int MaxMana, Vector2 Position, int ExtraAccessories, bool DemonHeart, int Projectiles, int ItemCount, int Difficulty, int Invasion, NPCStats NPCStats);
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;
using SPIC.Default.Infinities;

namespace SPIC.Default.Globals;

public sealed class DetectionPlayer : ModPlayer {

    public bool InItemCheck { get; private set; }
    public bool InRightClick { get; private set; }


    public override void Load() {
        On_Player.ItemCheck_Inner += HookItemCheck_Inner;
        On_ItemSlot.RightClick_ItemArray_int_int += HookRightClick;
        On_Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        On_Player.Teleport += HookTeleport;
        On_Player.Spawn += HookSpawn;
        On_Player.PayCurrency += HookPayCurrency;
    }

    private bool HookPayCurrency(On_Player.orig_PayCurrency orig, Player self, long price, int customCurrency) {
        bool enabled;
        if(Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Nurse) enabled = Currency.Config.Value.Nurse;
        else if (Main.InReforgeMenu) enabled = Currency.Config.Value.Reforging;
        else if (Main.npcShop != 0) enabled = Currency.Config.Value.Shop;
        else enabled = Currency.Config.Value.Others;
        return enabled && self.HasInfinite(customCurrency, price, Currency.Instance) || orig(self, price, customCurrency);
    }

    private static void HookItemCheck_Inner(On_Player.orig_ItemCheck_Inner orig, Player self) {
        DetectionPlayer detectionPlayer = self.GetModPlayer<DetectionPlayer>();
        detectionPlayer.InItemCheck = true;
        detectionPlayer.DetectingCategoryOf = null;

        if ((self.itemAnimation > 0 || !self.JustDroppedAnItem && self.ItemTimeIsZero)
                && Configs.InfinitySettings.Instance.DetectMissingCategories && InfinityManager.GetCategory(self.HeldItem, Usable.Instance) == UsableCategory.Unknown)
            detectionPlayer.PrepareDetection(self.HeldItem, true);
        orig(self);
        if (detectionPlayer.DetectingCategoryOf is not null) detectionPlayer.TryDetectCategory();
        detectionPlayer.InItemCheck = false;
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) => InfinityManager.ClearInfinities();

    public override void OnEnterWorld() => ExplosionProjectile.ClearExploded();

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
            InfinityManager.SaveDetectedCategory(DetectingCategoryOf, category, Usable.Instance);
            if(!_detectingConsumable) InfinityManager.SaveDetectedCategory(DetectingCategoryOf, GrabBagCategory.None, GrabBag.Instance);
        }

        DetectionDataScreenShot data = GetDetectionData();

        if (TryDetectUsable(data, out UsableCategory usable)) SaveUsable(usable);
        else if (mustDetect && _detectingConsumable) SaveUsable(UsableCategory.Booster);
        else return false;
        DetectingCategoryOf = null;

        return true;
    }

    private bool TryDetectUsable(DetectionDataScreenShot data, out UsableCategory category) {

        if(data.Projectiles != _preUseData.Projectiles) category = UsableCategory.Tool;

        else if (data.NPCStats.Boss != _preUseData.NPCStats.Boss || data.Invasion != _preUseData.Invasion) category = UsableCategory.Summoner;
        else if (data.NPCStats.Total != _preUseData.NPCStats.Total) category = UsableCategory.Critter;

        else if (data.MaxLife != _preUseData.MaxLife || data.MaxMana != _preUseData.MaxMana || data.ExtraAccessories != _preUseData.ExtraAccessories || data.DemonHeart != _preUseData.DemonHeart) category = UsableCategory.Booster;
        else if (data.Difficulty != _preUseData.Difficulty) category = UsableCategory.Booster;

        else if (_teleport || data.Position != _preUseData.Position) category = UsableCategory.Tool;

        else category = UsableCategory.Unknown;
        return category != UsableCategory.Unknown;
    }

    public int FindPotentialExplosivesType(int proj) {
        foreach (Dictionary<int, int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values) {
            foreach ((int item, int shoot) in projectiles) if (shoot == proj) return item;
        }
        foreach (Item item in Player.inventory) if (item.shoot == proj) return item.type;
        return ItemID.None;
    }

    public void RefilExplosive(int projType, int refill) {
        int owned = Player.CountItems(refill);
        int used = 0;
        foreach (Projectile proj in Main.projectile)
            if (proj.owner == Player.whoAmI && proj.type == projType) used += 1;

        if (ShouldRefill(refill, owned, used, Ammo.Instance))
                /* || (InfinityManager.GetCategory(refill, Usable.Instance) == UsableCategory.Explosive && ShouldRefill(refill, owned, used, Usable.Instance))*/
            Player.GetItem(Player.whoAmI, new(refill, used), new(NoText: true));

        static bool ShouldRefill(int refill, int owned, int used, Infinity<Items, Item> infinity) => InfinityManager.GetInfinity(refill, owned, infinity) == 0 && InfinityManager.GetInfinity(refill, owned + used, Usable.Instance) != 0;
    }


    public void Teleported() => _teleport = true;


    private static void HookRightClick(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot){
        DetectionPlayer detectionPlayer = Main.LocalPlayer.GetModPlayer<DetectionPlayer>();
        detectionPlayer.InRightClick = true;
        orig(inv, context, slot);
        detectionPlayer.InRightClick = false;
    }

    private static void HookPutItemInInventory(On_Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
        if (selItem < 0){
            orig(self, type, selItem);
            return;
        }

        Item item = self.inventory[selItem];

        Configs.InfinitySettings settings = Configs.InfinitySettings.Instance;
        if (settings.DetectMissingCategories && InfinityManager.GetCategory(item, Placeable.Instance) == PlaceableCategory.None) InfinityManager.SaveDetectedCategory(item, PlaceableCategory.Liquid, Placeable.Instance);
        
        item.stack++;
        if (!self.HasInfinite(item, 1, Placeable.Instance)) item.stack--;
        else if (settings.PreventItemDuplication) return;

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
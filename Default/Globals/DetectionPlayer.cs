using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using SPIC.Default.Displays;

namespace SPIC.Default.Globals;

public sealed class DetectionPlayer : ModPlayer {

    public override void Load() {
        On_ItemSlot.RightClick_ItemArray_int_int += HookRightClick;
        On_Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        On_Player.Teleport += HookTeleport;
        On_Player.Spawn += HookSpawn;
        On_Player.PayCurrency += HookPayCurrency;
        On_Player.ShootFromCannon += HookShootFromCannon;
    }

    private bool HookPayCurrency(On_Player.orig_PayCurrency orig, Player self, long price, int customCurrency) {
        Infinity<int> infinity;
        if (Main.npc[Main.player[Main.myPlayer].talkNPC].type == NPCID.Nurse) infinity = Nurse.Instance;
        else if (Main.InReforgeMenu) infinity = Reforging.Instance;
        else if (Main.npcShop != 0) infinity = Shop.Instance;
        else infinity = Purchase.Instance;
        return self.HasInfinite(customCurrency, price, infinity) || orig(self, price, customCurrency);
    }

    public override bool PreItemCheck() {
        InItemCheck = true;
        usedCannon = false;
        DetectingCategoryOf = null;
        var world = InfiniteWorld.Instance;
        if (Player.whoAmI == Main.myPlayer) {
            world.contextPlayer = Player;
            aimedAtInfiniteTile = world.IsInfinite(Player.tileTargetX, Player.tileTargetY, TileFlags.Block);
        }

        if ((Player.itemAnimation > 0 || !Player.JustDroppedAnItem && Player.ItemTimeIsZero)
                && Configs.InfinitySettings.Instance.detectMissingCategories && InfinityManager.GetCategory(Player.HeldItem, Usable.Instance) == UsableCategory.Unknown)
            PrepareDetection(Player.HeldItem);
        return true;
    }
    public override void PostItemCheck() {
        if (DetectingCategoryOf is not null) TryDetectCategory();
        InItemCheck = false;
        InfiniteWorld.Instance.contextPlayer = null;
        aimedAtInfiniteTile = false;
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) => InfinityManager.ClearCache();

    public override void PreUpdate() => Dots.PreUpdate();

    public override void OnEnterWorld() => ExplosionProjectile.ClearExploded();

    public void PrepareDetection(Item item) {
        DetectingCategoryOf = item;
        teleported = false;
        _preUseData = GetDetectionData();
    }

    public DetectionDataScreenShot GetDetectionData() => new(
        Player.statLifeMax2, Player.statManaMax2,
        Player.position,
        Player.extraAccessorySlots, Player.extraAccessory,
        Utility.CountProjectilesInWorld(), Player.CountBuffs(),
        Utility.WorldDifficulty(), Main.invasionType, Utility.GetNPCStats()
    );


    public bool TryDetectCategory(bool mustDetect = false) {
        if (DetectingCategoryOf is null) return false;

        void SaveUsable(UsableCategory category) => Usable.Instance.SaveDetectedCategory(DetectingCategoryOf, category);

        if (teleported) SaveUsable(UsableCategory.Tool);
        else if (TryDetectUsable(_preUseData, GetDetectionData(), out UsableCategory usable)) SaveUsable(usable);
        else if (mustDetect) SaveUsable(UsableCategory.Booster);
        else return false;
        DetectingCategoryOf = null;

        return true;
    }

    private static bool TryDetectUsable(DetectionDataScreenShot preUse, DetectionDataScreenShot postUse, out UsableCategory category) {
        if (postUse.NPCStats.Boss != preUse.NPCStats.Boss || postUse.Invasion != preUse.Invasion) category = UsableCategory.Summoner;
        else if (postUse.NPCStats.Total != preUse.NPCStats.Total) category = UsableCategory.Critter;

        else if (postUse.MaxLife != preUse.MaxLife || postUse.MaxMana != preUse.MaxMana || postUse.ExtraAccessories != preUse.ExtraAccessories || postUse.DemonHeart != preUse.DemonHeart) category = UsableCategory.Booster;
        else if (postUse.Difficulty != preUse.Difficulty) category = UsableCategory.Booster;

        else if (postUse.Position != preUse.Position) category = UsableCategory.Tool;
        
        else if (postUse.Buffs != preUse.Buffs) category = UsableCategory.Potion;
        else if (postUse.Projectiles != preUse.Projectiles) category = UsableCategory.Tool;

        else category = UsableCategory.Unknown;
        return category != UsableCategory.Unknown;
    }

    public static Item FindAmmo(Player player, int proj) {
        foreach (Dictionary<int, int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values) {
            foreach ((int item, int shoot) in projectiles) if (shoot == proj) return new(item);
        }
        foreach (Item item in player.inventory) if (item.shoot == proj) return item;
        return new();
    }

    public static void RefillExplosive(Player player, int projType, Item refill) {
        long owned = player.CountConsumables(refill, ConsumableItem.Instance);
        int used = 0;
        foreach (Projectile proj in Main.projectile)
            if (proj.owner == player.whoAmI && proj.type == projType) used += 1;

        if (ShouldRefill(refill, owned, used, Ammo.Instance))
            /* || (InfinityManager.GetCategory(refill, Usable.Instance) == UsableCategory.Explosive && ShouldRefill(refill, owned, used, Usable.Instance))*/
            player.GetItem(player.whoAmI, new(refill.type, used), new(NoText: true));

        static bool ShouldRefill(Item refill, long owned, int used, Infinity<Item> infinity) => InfinityManager.GetInfinity(refill, owned, Ammo.Instance) == 0 && InfinityManager.GetInfinity(refill, owned + used, Usable.Instance) != 0;
    }

    private void HookShootFromCannon(On_Player.orig_ShootFromCannon orig, Player self, int x, int y) {
        orig(self, x, y);
        self.GetModPlayer<DetectionPlayer>().usedCannon = true;
    }

    private static void HookRightClick(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        DetectionPlayer detectionPlayer = Main.LocalPlayer.GetModPlayer<DetectionPlayer>();
        detectionPlayer.InRightClick = true;
        orig(inv, context, slot);
        detectionPlayer.InRightClick = false;
    }

    private static void HookPutItemInInventory(On_Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
        if (selItem < 0) {
            orig(self, type, selItem);
            return;
        }

        Item item = self.inventory[selItem];

        if (InfinityManager.GetCategory(item, Placeable.Instance) == PlaceableCategory.None) Placeable.Instance.SaveDetectedCategory(item, PlaceableCategory.Bucket);

        item.stack++;
        if (!self.HasInfinite(item, 1, Placeable.Instance)) item.stack--;
        else if (Placeable.PreventItemDuplication && !Placeable.Instance.Config.preventItemDuplication.Value.allowMiscDrops) return;

        orig(self, type, selItem);
    }

    private static void HookSpawn(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context) {
        orig(self, context);
        self.GetModPlayer<DetectionPlayer>().teleported = true;
    }
    private static void HookTeleport(On_Player.orig_Teleport orig, Player self, Vector2 newPos, int Style = 0, int extraInfo = 0) {
        orig(self, newPos, Style, extraInfo);
        self.GetModPlayer<DetectionPlayer>().teleported = true;
    }

    public Item? DetectingCategoryOf;
    private DetectionDataScreenShot _preUseData;

    public bool InItemCheck { get; private set; }
    public bool InRightClick { get; private set; }
    public bool teleported;
    public bool usedCannon;
    public bool aimedAtInfiniteTile;
}

public record struct DetectionDataScreenShot(
    int MaxLife, int MaxMana,
    Vector2 Position,
    int ExtraAccessories, bool DemonHeart,
    int Projectiles, int Buffs,
    int Difficulty, int Invasion, NPCStats NPCStats);
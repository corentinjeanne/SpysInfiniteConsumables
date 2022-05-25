using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Globals {
    public class SpicPlayer : ModPlayer {

        public int preUseMaxLife, preUseMaxMana;
        public int preUseExtraAccessories;
        public Microsoft.Xna.Framework.Vector2 preUsePosition;
        public bool preUseDemonHeart;
        private int _checkingForCategory;
        public static int preUseDifficulty;
        public static int preUseInvasion;
        public static NPCStats preUseNPCStats;

        public bool CheckingForCategory => _checkingForCategory != ItemID.None;

        public bool InItemCheck { get; private set; }


        private readonly HashSet<int> _infiniteConsumables = new();
        private readonly HashSet<int> _infiniteAmmos = new();
        private readonly HashSet<int> _infiniteWandAmmos = new();
        private readonly HashSet<int> _infiniteGrabBabs = new();
        private readonly HashSet<int> _infiniteMaterials = new();
        public bool HasInfiniteConsumable(int type) => _infiniteConsumables.Contains(type);
        public bool HasInfiniteAmmo(int type) => _infiniteAmmos.Contains(type);
        public bool HasInfiniteWandAmmo(int type) => _infiniteWandAmmos.Contains(type);
        public bool HasInfiniteGrabBag(int type) => _infiniteGrabBabs.Contains(type);
        public bool HasInfiniteMaterial(int type) => _infiniteMaterials.Contains(type);

        public bool HasInfiniteWandAmmo(Item item) {

            Categories.Categories categories = item.GetCategories();
            // Multi tiles wands
            if (!categories.WandAmmo.HasValue && categories.Consumable?.IsTile() == true)
                return HasInfiniteConsumable(item.type);
            
            return HasInfiniteWandAmmo(item.type);
        }

        public override void Load() {
            On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        }

        public override bool PreItemCheck() {			
            InItemCheck = true;
            if (CheckingForCategory) SavePreUseItemStats();

            return true;
        }
        public override void PostItemCheck() {
            InItemCheck = false;
            if (CheckingForCategory) {
                if (Player.itemTime > 1) TryStopDetectingCategory();
                else StopDetectingCategory();
            }
        }

        public void FindInfinities() {
            _infiniteAmmos.Clear();
            _infiniteConsumables.Clear();
            _infiniteGrabBabs.Clear();
            _infiniteMaterials.Clear();
            _infiniteWandAmmos.Clear();

            HashSet<int> typesChecked = new();
            foreach (Item item in Player.inventory) {
                if (item.IsAir || typesChecked.Contains(item.type)) continue;

                Categories.Categories categories = item.GetCategories();
                if (Player.HasInfinite(item.type, categories.Consumable.GetValueOrDefault())) _infiniteConsumables.Add(item.type);
                if (Player.HasInfinite(item.type, categories.Ammo))                           _infiniteAmmos.Add(item.type);
                if (Player.HasInfinite(item.type, categories.WandAmmo.GetValueOrDefault()))   _infiniteWandAmmos.Add(item.type);
                if (Player.HasInfinite(item.type, categories.GrabBag.GetValueOrDefault()))    _infiniteGrabBabs.Add(item.type);
                if (Player.HasInfinite(item.type, categories.Material))                       _infiniteMaterials.Add(item.type);
                
                typesChecked.Add(item.type);
            }
        }
        public void StartDetectingCategory(int type) {
            _checkingForCategory = type;
            SavePreUseItemStats();
        }
        public void TryStopDetectingCategory() {
            Categories.Consumable? cat = CheckForCategory();
            if (cat.HasValue || Player.itemTime <= 1) StopDetectingCategory(cat);
        }
        public void StopDetectingCategory(Categories.Consumable? detectedCategory = null) {
            if (!CheckingForCategory) return;
            Configs.CategorySettings.Instance.SaveConsumableCategory(_checkingForCategory, detectedCategory ?? CheckForCategory() ?? Categories.Consumable.PlayerBooster);
            _checkingForCategory = ItemID.None;
        }

        private void SavePreUseItemStats() {
            preUseMaxLife = Player.statLifeMax2;
            preUseMaxMana = Player.statManaMax2;
            preUseExtraAccessories = Player.extraAccessorySlots;
            preUseDemonHeart = Player.extraAccessory;
            preUsePosition = Player.position;

            preUseDifficulty = Utility.WorldDifficulty();
            preUseInvasion = Main.invasionType;
            preUseNPCStats = Utility.GetNPCStats();
        }
        public Categories.Consumable? CheckForCategory() {

            NPCStats stats = Utility.GetNPCStats();
            if (preUseNPCStats.boss != stats.boss || preUseInvasion != Main.invasionType)
                return Categories.Consumable.Summoner;

            if (preUseNPCStats.total != stats.total)
                return Categories.Consumable.Critter;

            // Player Boosters
            if (preUseMaxLife != Player.statLifeMax2 || preUseMaxMana != Player.statManaMax2
                    || preUseExtraAccessories != Player.extraAccessorySlots || preUseDemonHeart != Player.extraAccessory)
                return Categories.Consumable.PlayerBooster;

            // World boosters
            // TODO Other difficulties
            if (preUseDifficulty != Utility.WorldDifficulty())
                return Categories.Consumable.WorldBooster;

            // Some tools
            if (Player.position != preUsePosition)
                return Categories.Consumable.Tool;

            // No new category detected
            return null;
        }

        public int FindPotentialExplosivesType(int proj) {

            foreach (Dictionary<int,int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values){
                foreach((int t, int p) in projectiles){
                    if(p == proj) return t;
                }
            }
            foreach(Item item in Player.inventory){
                if (item.shoot == proj) return item.type;
            }
            return 0;
        }
        public  void RefilExplosive(int type) {
            int tot = Player.CountAllItems(type);
            int used = 0;
            foreach (Projectile p in Main.projectile) {
                if (p.type == type) used += 1;
            }

            Categories.Categories categories = CategoryHelper.GetCategories(type);
            if ((categories.Consumable == Categories.Consumable.Tool && ConsumableExtension.IsInfinite(tot + used, type, categories.Consumable.Value))
                    || (categories.Ammo != Categories.Ammo.None && AmmoExtension.IsInfinite(tot + used, type, categories.Ammo))) {
                Player.GetItem(Player.whoAmI, new(type, used), new(NoText: true));
            }
        }
        
        private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
            if (selItem > -1) {

                Configs.CategorySettings.Instance.SaveConsumableCategory(type, Categories.Consumable.Bucket);
                
                self.inventory[selItem].stack++;
                if (Configs.Infinities.Instance.PreventItemDupication && self.HasInfinite(self.inventory[selItem].type, Categories.Consumable.Bucket))
                    return;
                
                self.inventory[selItem].stack--;
            }
            orig(self, type, selItem);
        }
    }
}
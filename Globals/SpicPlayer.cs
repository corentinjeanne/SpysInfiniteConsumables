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
        public bool CheckingForCategory => _checkingForCategory != Terraria.ID.ItemID.None;
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
            SpicItem spicItem = item.GetGlobalItem<SpicItem>();

            // Multi tiles wands
            if (!spicItem.WandAmmo.HasValue && spicItem.Consumable?.IsTile() == true)
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
                if(item.IsAir) continue;
                if (typesChecked.Contains(item.type)) continue;
                SpicItem spicItem = item.GetGlobalItem<SpicItem>();
                if (Player.HasInfinite(item.type, spicItem.Consumable.GetValueOrDefault())) _infiniteConsumables.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.Ammo))                           _infiniteAmmos.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.WandAmmo.GetValueOrDefault()))   _infiniteWandAmmos.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.GrabBag.GetValueOrDefault()))    _infiniteGrabBabs.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.Material))                       _infiniteMaterials.Add(item.type);
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
            RebuildCategories(_checkingForCategory);
            _checkingForCategory = ItemID.None;
        }

        private void SavePreUseItemStats() {
            preUseMaxLife = Player.statLifeMax2;
            preUseMaxMana = Player.statManaMax2;
            preUseExtraAccessories = Player.extraAccessorySlots;
            preUseDemonHeart = Player.extraAccessory;
            preUsePosition = Player.position;

            Systems.SpicWorld.SavePreUseItemStats();
        }
        public Categories.Consumable? CheckForCategory() {

            NPCStats stats = Utility.GetNPCStats();
            if (Systems.SpicWorld.preUseNPCStats.boss != stats.boss || Systems.SpicWorld.preUseInvasion != Main.invasionType)
                return Categories.Consumable.Summoner;

            if (Systems.SpicWorld.preUseNPCStats.total != stats.total)
                return Categories.Consumable.Critter;

            // Player Boosters
            if (preUseMaxLife != Player.statLifeMax2 || preUseMaxMana != Player.statManaMax2
                    || preUseExtraAccessories != Player.extraAccessorySlots || preUseDemonHeart != Player.extraAccessory)
                return Categories.Consumable.PlayerBooster;

            // World boosters
            if (Systems.SpicWorld.preUseDifficulty != Utility.WorldDifficulty())
                return Categories.Consumable.WorldBooster;

            // Some tools
            if (Player.position != preUsePosition)
                return Categories.Consumable.Tool;

            // No new category detected
            return null;
        }

        public void FindPotentialExplosives(int proj) {
            int type = ItemID.None;

            foreach (Dictionary<int,int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values){
                foreach((int t, int p) in projectiles){
                    if(p == proj){
                        type = t;
                        goto ProcessExplosive;
                    }
                }
            }
            foreach(Item item in Player.inventory){
                if (item.shoot == proj) {
                    type = item.type;
                    goto ProcessExplosive;
                }
            }

            ProcessExplosive:
            if (type != ItemID.None && Configs.CategorySettings.Instance.SaveExplosive(type)){
                RebuildCategories(type);

                // Refill if needed
                int tot = Player.CountAllItems(type);
                int air = 0;
                foreach(Projectile p in Main.projectile){
                    if(p.type == proj)
                        air += 1;
                }
                SpicItem spicItem = new Item(type).GetGlobalItem<SpicItem>();

                if ((spicItem.Consumable == Categories.Consumable.Tool && ConsumableExtension.HasInfinite(tot+air, type, spicItem.Consumable.Value))
                        || (spicItem.Ammo != Categories.Ammo.None && AmmoExtension.HasInfinite(tot+air, type, spicItem.Ammo))){
                    Player.GetItem(Player.whoAmI, new(type, air), new(NoText: true));
                }
            }
        } 

        public void RebuildCategories(int type) {
            if(Main.mouseItem != null) Main.mouseItem.GetGlobalItem<SpicItem>().BuildCategories(Main.mouseItem);
            foreach(Item i in Player.inventory){
                if(i.type == type) i.GetGlobalItem<SpicItem>().BuildCategories(i);
            }
        }
        
        private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
            if (selItem > -1) {
                Configs.CategorySettings.Instance.SaveConsumableCategory(type, Categories.Consumable.Bucket);
                self.GetModPlayer<SpicPlayer>().RebuildCategories(type);
                
                self.inventory[selItem].stack++;
                if (Configs.Infinities.Instance.PreventItemDupication && self.HasInfinite(self.inventory[selItem].type, Categories.Consumable.Bucket))
                    return;
                
                self.inventory[selItem].stack--;
            }
            orig(self, type, selItem);
        }
    }
}
using System;
using System.Collections.Generic;

using Terraria;
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
                if (Player.HasInfinite(item.type, spicItem.Consumable ?? Categories.Consumable.None)) _infiniteConsumables.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.Ammo))                                     _infiniteAmmos.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.WandAmmo ?? Categories.WandAmmo.None))     _infiniteWandAmmos.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.GrabBag ?? Categories.GrabBag.None))       _infiniteGrabBabs.Add(item.type);
                if (Player.HasInfinite(item.type, spicItem.Material))                                 _infiniteMaterials.Add(item.type);
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
            Configs.ConsumableConfig.Instance.SaveConsumableCategory(_checkingForCategory, detectedCategory ?? CheckForCategory() ?? Categories.Consumable.PlayerBooster);
            RebuildCategories(_checkingForCategory);
            _checkingForCategory = Terraria.ID.ItemID.None;
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

        public void RebuildCategories(int type) {
        foreach(Item i in Player.inventory){
                if(i.type == type) i.GetGlobalItem<SpicItem>().BuildCategories(i);
            }
            FindInfinities();
        }
        
        private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
            if (selItem > -1) {
                Configs.ConsumableConfig.Instance.SaveConsumableCategory(type, Categories.Consumable.Bucket);
                self.GetModPlayer<SpicPlayer>().RebuildCategories(type);
                self.inventory[selItem].stack++;
                if (Configs.ConsumableConfig.Instance.PreventItemDupication && self.HasInfinite(self.inventory[selItem].type, Categories.Consumable.Bucket)) {
                    return;
                }
                self.inventory[selItem].stack--;
            }
            orig(self, type, selItem);
        }
    }
}
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Globals {
    public class SpicPlayer : ModPlayer {

        private int _preUseMaxLife, _preUseMaxMana;
        private int _preUseExtraAccessories;
        private Microsoft.Xna.Framework.Vector2 _preUsePosition;
        private bool _preUseDemonHeart;
        private int _preUseDifficulty;
        private int _preUseInvasion;
        private int _preUseItemCount;
        private static NPCStats _preUseNPCStats;

        private bool _detectingCategory;

        public bool InItemCheck { get; private set; }

        private readonly Dictionary<int, Categories.ItemInfinities> _infinities = new();
        public Categories.ItemInfinities GetInfinities(Item item) => _infinities.ContainsKey(item.type) ? _infinities[item.type] : new(Player, item);
        public Categories.ItemInfinities GetInfinities(int type) => _infinities.ContainsKey(type) ? _infinities[type] : new(Player, new(type));

        private readonly Dictionary<int, long> _infiniteCurrencies = new();
        public long GetCurrencyInfinity(int currency) => _infiniteCurrencies.ContainsKey(currency) ? _infiniteCurrencies[currency] : CurrencyExtension.GetCurrencyInfinity(Player, currency);

        public override void Load() {
            On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        }

        public override bool PreItemCheck() {

            if (Configs.CategoryDetection.Instance.DetectMissing && !Player.HeldItem.GetCategories().Consumable.HasValue) {
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


        // public static bool IsInfinite(long count) => count != -1;
        public void FindInfinities() {
            _infinities.Clear();
            _infiniteCurrencies.Clear();
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
        
        // BUG recall when at spawn -> err: booster vs Tool
        public void TryDetectCategory(bool mustDetect = false) {
            if (!_detectingCategory) return;

            void SaveConsumable(Categories.Consumable category)
                => Configs.CategoryDetection.Instance.DetectedConsumable(Player.HeldItem, category);
            
            void SaveBag() {
                Configs.CategoryDetection.Instance.DetectedGrabBag(Player.HeldItem);
                Configs.CategoryDetection.Instance.DetectedConsumable(Player.HeldItem, Categories.Consumable.None);
            }

            Categories.Consumable? consumable = TryDetectConsumable();
            Categories.GrabBag? bag = TryDetectGrabBag();

            if(consumable.HasValue) SaveConsumable(consumable.Value);
            else if (bag.HasValue) SaveBag();
            else if (mustDetect) SaveConsumable(Categories.Consumable.PlayerBooster);
            else return; // Nothing detected

            _detectingCategory = false;
            FindInfinities();
        }

        private Categories.Consumable? TryDetectConsumable(){
            NPCStats stats = Utility.GetNPCStats();
            if (_preUseNPCStats.boss != stats.boss || _preUseInvasion != Main.invasionType)
                return Categories.Consumable.Summoner;

            if (_preUseNPCStats.total != stats.total)
                return Categories.Consumable.Critter;

            if (_preUseMaxLife != Player.statLifeMax2 || _preUseMaxMana != Player.statManaMax2
                    || _preUseExtraAccessories != Player.extraAccessorySlots || _preUseDemonHeart != Player.extraAccessory)
                return Categories.Consumable.PlayerBooster;

            // TODO Other difficulties
            if (_preUseDifficulty != Utility.WorldDifficulty())
                return Categories.Consumable.WorldBooster;

            if (Player.position != _preUsePosition)
                return Categories.Consumable.Tool;

            return null;
        }

        private Categories.GrabBag? TryDetectGrabBag(){
            if (Utility.CountItemsInWorld() != _preUseItemCount)
                return Categories.GrabBag.Crate;
            return null;
        }

        public int FindPotentialExplosivesType(int proj) {
            foreach (Dictionary<int,int> projectiles in AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Values){
                foreach((int t, int p) in projectiles) if(p == proj) return t;
            }
            foreach(Item item in Player.inventory) if (item.shoot == proj) return item.type;
            
            return 0;
        }
        
        public  void RefilExplosive(int proj, Item refill) {
            int tot = Player.CountItems(refill.type);
            int used = 0;
            foreach (Projectile p in Main.projectile)
                if (p.owner == Player.whoAmI && p.type == proj) used += 1;
            
            Configs.Requirements infinities = Configs.Requirements.Instance;
            Categories.ItemCategories categories = CategoryHelper.GetCategories(refill);
            if (infinities.InfiniteConsumables && (
                    (categories.Consumable == Categories.Consumable.Tool && refill.GetConsumableInfinity(tot + used) > 0)
                    || (categories.Ammo != Categories.Ammo.None && refill.GetAmmoInfinity(tot + used) > 0)
            )) 
                Player.GetItem(Player.whoAmI, new(refill.type, used), new(NoText: true));
            
        }
        
        private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
            if (selItem < 0) goto origin;

            Item item = self.inventory[selItem];

            Configs.CategoryDetection autos = Configs.CategoryDetection.Instance;
            Configs.Requirements infinities = Configs.Requirements.Instance;
            Categories.ItemCategories categories = CategoryHelper.GetCategories(self.inventory[selItem]);


            if(autos.DetectMissing && categories.Placeable == Categories.Placeable.None)
                autos.DetectedPlaceable(item, Categories.Placeable.Liquid);
            
            if (infinities.InfinitePlaceables && item.GetPlaceableInfinity(self.CountItems(item.type)+1) != -1)
                item.stack++;

            if (infinities.PreventItemDupication) return;
            origin: orig(self, type, selItem);
        }
    }
}
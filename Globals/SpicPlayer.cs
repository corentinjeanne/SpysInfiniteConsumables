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
        private Item _detectingCategory;
        private static int _preUseDifficulty;
        private static int _preUseInvasion;
        private static int _preUseItemCount;

        private static NPCStats _preUseNPCStats;

        public bool DetectingCategory => _detectingCategory != null;

        public bool InItemCheck { get; private set; }


        private readonly HashSet<int> _infiniteConsumables = new();
        private readonly HashSet<int> _infinitePlaceables = new();
        private readonly HashSet<int> _infiniteAmmos = new();
        private readonly HashSet<int> _infiniteGrabBabs = new();
        private readonly Dictionary<int, long> _infiniteMaterials = new();
        private readonly Dictionary<int, long> _infiniteCurrencies = new();

        public bool HasInfiniteConsumable(int type) => _infiniteConsumables.Contains(type);
        public bool HasInfiniteAmmo(int type) => _infiniteAmmos.Contains(type);
        public bool HasInfinitePlaceable(int type) => _infinitePlaceables.Contains(type);
        public bool HasInfiniteGrabBag(int type) => _infiniteGrabBabs.Contains(type);
        public bool HasInfiniteMaterial(int type) => _infiniteMaterials.TryGetValue(type, out _);
        public bool HasInfiniteMaterial(int type, out long inf) => _infiniteMaterials.TryGetValue(type, out inf);
        public bool HasInfiniteMaterial(int type, long cost) => _infiniteMaterials.TryGetValue(type, out long inf) && cost <= inf;
        public bool HasInfiniteCurrency(int id, out long inf) => _infiniteCurrencies.TryGetValue(id, out inf);
        public bool HasInfiniteCurrency(int id, long cost) => _infiniteCurrencies.TryGetValue(id, out long inf) && cost <= inf;


        public override void Load() {
            On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
        }

        public override bool PreItemCheck() {			
            InItemCheck = true;
            if (DetectingCategory) SavePreUseItemStats();
            return true;
        }
        public override void PostItemCheck() {
            InItemCheck = false;
            if (!DetectingCategory) return;

            if (Player.itemTime > 1) TryDetectCategory();
            else TryDetectCategory(true);

        }


        public static bool IsInfinite(long count) => count != 0;
        public void FindInfinities() {
            _infiniteAmmos.Clear();
            _infiniteConsumables.Clear();
            _infinitePlaceables.Clear();
            _infiniteGrabBabs.Clear();
            _infiniteMaterials.Clear();
            _infiniteCurrencies.Clear();

            HashSet<int> typesChecked = new();
            HashSet<int> currenciesChecked = new();

            void LookIn(Item[] inventory){
                foreach (Item item in inventory) {
                    if(item.IsAir) continue;

                    long inf;
                    if(item.IsPartOfACurrency(out int currency) && !currenciesChecked.Contains(currency)){

                        if (IsInfinite(inf = Player.GetCurrencyInfinity(item))) _infiniteCurrencies.Add(currency, inf);
                        currenciesChecked.Add(currency);
                    }
                    if (!typesChecked.Contains(item.type)) {
                        int count = Player.CountItems(item.type, true);
                        if (IsInfinite(item.GetAmmoInfinity(count))) _infiniteAmmos.Add(item.type);
                        if (IsInfinite(item.GetConsumableInfinity(count))) _infiniteConsumables.Add(item.type);
                        if (IsInfinite(item.GetPlaceableInfinity(count))) _infinitePlaceables.Add(item.type);
                        if (IsInfinite(item.GetGrabBagInfinity(count))) _infiniteGrabBabs.Add(item.type);
                        if (IsInfinite(inf = item.GetMaterialInfinity(count))) _infiniteMaterials.Add(item.type, inf);
                        typesChecked.Add(item.type);
                    }
                }
            }
            LookIn(Player.inventory);
            Item[] chest = Player.Chest();
            if(chest != null) LookIn(chest);
        }

        public void StartDetectingCategory(Item item) {
            _detectingCategory = item;
            SavePreUseItemStats();
        }
        private void StopDetectingCategory() {
            // if (!DetectingCategory) return;
            _detectingCategory = null;
        }
        
        // BUG recall when at spawn -> err: booster vs Tool
        public void TryDetectCategory(bool mustDetect = false) {
            if (!DetectingCategory) return;

            void SaveConsumable(Categories.Consumable category)
                => Configs.CategorySettings.Instance.SaveConsumableCategory(_detectingCategory, category);
            void SaveBag()
                => Configs.CategorySettings.Instance.SaveGrabBagCategory(_detectingCategory);

            NPCStats stats = Utility.GetNPCStats();
            if (_preUseNPCStats.boss != stats.boss || _preUseInvasion != Main.invasionType)
                SaveConsumable(Categories.Consumable.Summoner);
            else if (_preUseNPCStats.total != stats.total)
                SaveConsumable(Categories.Consumable.Critter);

            // Player Boosters
            else if (_preUseMaxLife != Player.statLifeMax2 || _preUseMaxMana != Player.statManaMax2
                    || _preUseExtraAccessories != Player.extraAccessorySlots || _preUseDemonHeart != Player.extraAccessory)
                SaveConsumable(Categories.Consumable.PlayerBooster);

            // World boosters
            // TODO Other difficulties
            else if (_preUseDifficulty != Utility.WorldDifficulty())
                SaveConsumable(Categories.Consumable.WorldBooster);

            // Some tools
            else if (Player.position != _preUsePosition)
                SaveConsumable(Categories.Consumable.Tool);

            // Bags opened with leftclick
            else if (Utility.CountItemsInWorld() != _preUseItemCount)
                SaveBag();

            // Must find a category
            else if (mustDetect)
                SaveConsumable(Categories.Consumable.PlayerBooster);

            // No new category detected
            else return;

            // Any category was detected
            StopDetectingCategory();
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
            
            Configs.Infinities infinities = Configs.Infinities.Instance;
            Categories.Categories categories = Category.GetCategories(refill);
            if (infinities.InfiniteConsumables && (
                    (categories.Consumable == Categories.Consumable.Tool && IsInfinite(refill.GetConsumableInfinity(tot + used)))
                    || (categories.Ammo != Categories.Ammo.None && IsInfinite(refill.GetAmmoInfinity(tot + used)))
            )) 
                Player.GetItem(Player.whoAmI, new(refill.type, used), new(NoText: true));
            
        }
        
        private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
            if (selItem < 0) goto origin;

            Item item = self.inventory[selItem];

            Configs.CategorySettings autos = Configs.CategorySettings.Instance;
            Configs.Infinities infinities = Configs.Infinities.Instance;
            Categories.Categories categories = Category.GetCategories(self.inventory[selItem]);


            if(autos.AutoCategories && categories.Placeable == Categories.Placeable.None)
                autos.SavePlaceableCategory(item, Categories.Placeable.Liquid);
            
            if (infinities.InfinitePlaceables && IsInfinite(item.GetPlaceableInfinity(self.CountItems(item.type)+1)))
                item.stack++;

            if (infinities.PreventItemDupication) return;
            origin: orig(self, type, selItem);
        }
    }
}
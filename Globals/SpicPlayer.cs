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
        private Item _checkingForCategory;
        private static int _preUseDifficulty;
        private static int _preUseInvasion;
        private static NPCStats _preUseNPCStats;

        public bool CheckingForCategory => _checkingForCategory != null;

        public bool InItemCheck { get; private set; }


        private readonly HashSet<int> _infiniteConsumables = new();
        private readonly HashSet<int> _infinitePlaceables = new();
        private readonly HashSet<int> _infiniteAmmos = new();
        private readonly HashSet<int> _infiniteGrabBabs = new();
        public bool HasInfiniteConsumable(int type) => _infiniteConsumables.Contains(type);
        public bool HasInfiniteAmmo(int type) => _infiniteAmmos.Contains(type);
        public bool HasInfinitePlaceable(int type) => _infinitePlaceables.Contains(type);
        public bool HasInfiniteGrabBag(int type) => _infiniteGrabBabs.Contains(type);

        private readonly HashSet<int> _infiniteMaterials = new();
        public bool HasInfiniteMaterial(int type) => _infiniteMaterials.Contains(type);

        private readonly Dictionary<int, int> _infiniteCurrencies = new();
        // public bool HasInfiniteCurrency(int id) => _infiniteCurrencies.ContainsKey(id);


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
            _infinitePlaceables.Clear();
            _infiniteGrabBabs.Clear();
            _infiniteMaterials.Clear();
            _infiniteCurrencies.Clear();

            HashSet<int> typesChecked = new();
            HashSet<int> currenciesChecked = new();

            void LookIn(Item[] inventory){
                foreach (Item item in inventory) {
                    if (item.IsAir || typesChecked.Contains(item.type)) continue;

                    if (Player.HasInfiniteAmmo(item)) _infiniteAmmos.Add(item.type);
                    if (Player.HasInfiniteConsumable(item)) _infiniteConsumables.Add(item.type);
                    if (Player.HasInfinitePlaceable(item)) _infinitePlaceables.Add(item.type);
                    if (Player.HasInfiniteGrabBag(item)) _infiniteGrabBabs.Add(item.type);

                    if (Player.HasInfiniteMaterial(item)) _infiniteMaterials.Add(item.type);
                    typesChecked.Add(item.type);
                }
            }
            LookIn(Player.inventory);

            Item[] chest = Player.Chest();
            if(chest != null) LookIn(chest);
        }
        public void StartDetectingCategory(Item item) {
            _checkingForCategory = item;
            SavePreUseItemStats();
        }
        public void TryStopDetectingCategory() {
            if (!CheckingForCategory) return;
            Categories.Consumable? cat = CheckForCategory();
            if (cat.HasValue || Player.itemTime <= 1) StopDetectingCategory(cat);
        }
        public void StopDetectingCategory(Categories.Consumable? detectedCategory = null) {
            if (!CheckingForCategory) return;
            Configs.CategorySettings.Instance.SaveConsumableCategory(_checkingForCategory, detectedCategory ?? CheckForCategory() ?? Categories.Consumable.PlayerBooster);
            _checkingForCategory = null;
        }

        private void SavePreUseItemStats() {
            preUseMaxLife = Player.statLifeMax2;
            preUseMaxMana = Player.statManaMax2;
            preUseExtraAccessories = Player.extraAccessorySlots;
            preUseDemonHeart = Player.extraAccessory;
            preUsePosition = Player.position;

            _preUseDifficulty = Utility.WorldDifficulty();
            _preUseInvasion = Main.invasionType;
            _preUseNPCStats = Utility.GetNPCStats();
        }
        public Categories.Consumable? CheckForCategory() {

            NPCStats stats = Utility.GetNPCStats();
            if (_preUseNPCStats.boss != stats.boss || _preUseInvasion != Main.invasionType)
                return Categories.Consumable.Summoner;

            if (_preUseNPCStats.total != stats.total)
                return Categories.Consumable.Critter;

            // Player Boosters
            if (preUseMaxLife != Player.statLifeMax2 || preUseMaxMana != Player.statManaMax2
                    || preUseExtraAccessories != Player.extraAccessorySlots || preUseDemonHeart != Player.extraAccessory)
                return Categories.Consumable.PlayerBooster;

            // World boosters
            // ? Other difficulties
            if (_preUseDifficulty != Utility.WorldDifficulty())
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
        public  void RefilExplosive(int proj, Item refill) {
            int tot = Player.CountAllItems(refill.type);
            int used = 0;
            foreach (Projectile p in Main.projectile) {
                if (p.owner == Player.whoAmI && p.type == proj) used += 1;
            }

            Categories.Categories categories = Category.GetCategories(refill);
            if ((categories.Consumable == Categories.Consumable.Tool && ConsumableExtension.IsInfiniteConsumable(tot + used, refill))
                    || (categories.Ammo != Categories.Ammo.None && AmmoExtension.IsInfiniteAmmo(tot + used, refill))) {
                Player.GetItem(Player.whoAmI, new(refill.type, used), new(NoText: true));
            }
        }
        
        private static void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
            if (selItem < 0) goto origin;

            Item item = self.inventory[selItem];
            SpicPlayer spicPlayer = self.GetModPlayer<SpicPlayer>();

            Configs.CategorySettings autos = Configs.CategorySettings.Instance;
            Configs.Infinities infinities = Configs.Infinities.Instance;
            Categories.Categories categories = Category.GetCategories(self.inventory[selItem]);


            if(autos.AutoCategories && categories.Placeable == Categories.Placeable.None)
                autos.SavePlaceableCategory(item, Categories.Placeable.Liquid);
            
            item.stack++;
            if (!(infinities.InfinitePlaceables && spicPlayer.HasInfinitePlaceable(item.type)))
                item.stack--;
            else if (infinities.PreventItemDupication) return;

            origin:
            orig(self, type, selItem);
        }
    }
}
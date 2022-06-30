using System.Collections.Generic;

using Terraria;


namespace SPIC {

    namespace Categories {
        public struct Categories {
            public readonly Ammo Ammo;
            public readonly Consumable? Consumable;
            public readonly Placeable Placeable;
            public readonly GrabBag? GrabBag;
            public readonly Material Material;
            public readonly Currency Currency;

            public Categories(Item item){
                Ammo = item.GetAmmoCategory();
                Consumable = item.GetConsumableCategory();
                Placeable = item.GetPlaceableCategory();
                GrabBag = item.GetGrabBagCategory();
                Material = item.GetMaterialCategory();
                Currency = item.GetCurrencyCategory();
            }
        }
        public struct Infinities {
            public readonly int Ammo;
            public readonly int Consumable;
            public readonly int GrabBag;
            public readonly int Placeable;
            public readonly int Material;
            public readonly int Currency;


            public Infinities(Item item){
                Ammo = item.GetAmmoInfinity();
                Consumable = item.GetConsumableInfinity();
                GrabBag = item.GetGrabBagInfinity();
                Placeable = item.GetPlaceableInfinity();
                Material = item.GetMaterialInfinity();
                Currency = item.GetCurrencyInfinity();
            }
        }
        
    }

    public static class Category {

        private static readonly Dictionary<int, Categories.Categories> _categories = new();
        private static readonly Dictionary<int, Categories.Infinities> _infinities = new();
        public static void ClearAll(){
            _categories.Clear();
            _infinities.Clear();
        }

        public static void UpdateItem(Item item){
            if(!_categories.ContainsKey(item.type)) return;
            _categories.Remove(item.type, out _);
            _categories.Add(item.type, new(item));

            if (!_infinities.ContainsKey(item.type)) return;
            _infinities.Remove(item.type, out _);
            _infinities.Add(item.type, new(item));
        }
        
        public static Categories.Categories GetCategories(this Item item){
            if(!_categories.ContainsKey(item.type)) _categories.Add(item.type, new(item));
            return _categories[item.type];
        }

        public static Categories.Infinities GetInfinities(this Item item){
            if (!_infinities.ContainsKey(item.type)) _infinities.Add(item.type, new(item));
            return _infinities[item.type];
        }
        
        public static bool IsInfinite(int items, int infinity, bool exact = false)
            => infinity != 0 && (exact ? items == infinity : items >= infinity);
    }
}
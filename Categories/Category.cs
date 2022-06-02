using System.Collections.Generic;

using Terraria;

namespace SPIC {

    namespace Categories {
        public struct Categories {
            public readonly Ammo Ammo;
            public readonly Consumable? Consumable;
            public readonly GrabBag? GrabBag;
            public readonly WandAmmo? WandAmmo;
            public readonly Material Material;

            public Categories(Item item){
                Ammo = item.GetAmmoCategory();
                Consumable = item.GetConsumableCategory();
                GrabBag = item.GetGrabBagCategory();
                WandAmmo = item.GetWandAmmoCategory();
                Material = item.GetMaterialCategory();
            }
        }
        public struct Infinities {
            public readonly int Ammo;
            public readonly int Consumable;
            public readonly int GrabBag;
            public readonly int WandAmmo;
            public readonly int Material;


            public Infinities(Item item){
                Ammo = item.GetAmmoInfinity();
                Consumable = item.GetConsumableInfinity();
                GrabBag = item.GetGrabBagInfinity();
                WandAmmo = item.GetWandAmmoInfinity();
                Material = item.GetMaterialInfinity();
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

        public static void UpdateItem(int type) => UpdateItem(new Item(type));
        public static void UpdateItem(Item item){
            if(!_categories.ContainsKey(item.type)) return;
            _categories.Remove(item.type, out _);
            _categories.Add(item.type, new(item));

            if (!_infinities.ContainsKey(item.type)) return;
            _infinities.Remove(item.type, out _);
            _infinities.Add(item.type, new(item));
        }
        
        public static Categories.Categories GetCategories(int type) => GetCategories(new Item(type));
        
        public static Categories.Categories GetCategories(this Item item){
            if(!_categories.ContainsKey(item.type)) _categories.Add(item.type, new(item));
            return _categories[item.type];
        }
        public static Categories.Infinities GetInfinities(int type) => GetInfinities(new Item(type));
        
        public static Categories.Infinities GetInfinities(this Item item){
            if(!_infinities.ContainsKey(item.type)) _infinities.Add(item.type, new(item));
            return _infinities[item.type];
        }
    }
}
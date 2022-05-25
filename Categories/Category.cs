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
        
    }

    public static class CategoryHelper {

        private static readonly Dictionary<int, Categories.Categories> _categories = new();
        public static void ClearCategories() => _categories.Clear();

        public static void UpdateItem(int type) => UpdateItem(new Item(type));
        public static void UpdateItem(Item item){
            if(!_categories.ContainsKey(item.type)) return;
            _categories.Remove(item.type, out _);
            _categories.Add(item.type, new(item));
        }
        
        public static Categories.Categories GetCategories(int type)
            => _categories.ContainsKey(type) ? _categories[type] : GetCategories(new Item(type));
        
        public static Categories.Categories GetCategories(this Item item){
            if(!_categories.ContainsKey(item.type)) _categories.Add(item.type, new(item));
            return _categories[item.type];
        }
    }
}
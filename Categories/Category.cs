using System;
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
                Ammo = item.GetAmmoRequirement();
                Consumable = item.GetConsumableRequirement();
                GrabBag = item.GetGrabBagRequirement();
                Placeable = item.GetPlaceableRequirement();
                Material = item.GetMaterialRequirement();
                Currency = item.GetCurrencyRequirement();
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

        public static Categories.Infinities GetRequirements(this Item item){
            if (!_infinities.ContainsKey(item.type)) _infinities.Add(item.type, new(item));
            return _infinities[item.type];
        }

        public delegate long AboveRequirementInfinity(long count, int requirement, params int[] args);
        public static class ARIDelegates{
            public static long NotInfinite(long count, int requirement, params int[] args) => 0;
            public static long ItemCount(long count, int requirement, params int[] args) => count;
            public static long Requirement(long count, int requirement, params int[] args) => count = requirement;

            public static long LargestMultiple(long count, int requirement, params int[] args)
                => count / requirement * requirement;
            public static long LargestPower(long count, int requirement, params int[] args)
                => requirement * (long)MathF.Pow(args[0],(int)MathF.Log(count / (float)requirement, args[0]));
            
        }

        public static long Infinity(int type, int theoricalMaxStack, long count, int requirement, float multiplier = 1, AboveRequirementInfinity aboveRequirement = null, params int[] args) {
            if(requirement == 0) return 0;

            requirement = Utility.RequirementToItems(requirement, type, theoricalMaxStack);

            long infinity = 0;
            if(count == requirement) infinity = requirement;
            else if (count > requirement) {
                aboveRequirement ??= ARIDelegates.Requirement;
                infinity = aboveRequirement.Invoke(count, requirement, args);
            }
            return (long)(infinity * multiplier);
        }
    }
}
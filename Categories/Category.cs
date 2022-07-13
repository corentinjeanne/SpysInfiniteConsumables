using System;
using System.Collections.Generic;

using Terraria;

using SPIC.Categories;
namespace SPIC {

    namespace Categories {
        public struct ItemCategories {
            public readonly Ammo Ammo;
            public readonly Consumable? Consumable;
            public readonly Placeable Placeable;
            public readonly GrabBag? GrabBag;
            public readonly Material Material;
            public readonly Currency Currency;

            public ItemCategories(Item item){
                Ammo = item.GetAmmoCategory();
                Consumable = item.GetConsumableCategory();
                Placeable = item.GetPlaceableCategory();
                GrabBag = item.GetGrabBagCategory();
                Material = item.GetMaterialCategory();
                Currency = item.GetCurrencyCategory();
            }
        }
        public struct ItemRequirements {
            public readonly int Ammo;
            public readonly int Consumable;
            public readonly int GrabBag;
            public readonly int Placeable;
            public readonly int Material;
            public readonly int Currency;


            public ItemRequirements(Item item){
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

        private static readonly Dictionary<int, ItemCategories> _categories = new();
        private static readonly Dictionary<int, ItemRequirements> _requirements = new();
        public static void ClearAll(){
            _categories.Clear();
            _requirements.Clear();
        }

        public static void UpdateItem(Item item){
            _categories.Remove(item.type, out _);
            _requirements.Remove(item.type, out _);
            _categories.Add(item.type, new(item));
            _requirements.Add(item.type, new(item));
        }
        
        public static ItemCategories GetCategories(this Item item){
            if(!_categories.ContainsKey(item.type)) _categories.Add(item.type, new(item));
            return _categories[item.type];
        }

        public static ItemRequirements GetRequirements(this Item item){
            if (!_requirements.ContainsKey(item.type)) _requirements.Add(item.type, new(item));
            return _requirements[item.type];
        }

        public delegate long AboveRequirementInfinity(long count, int requirement, params int[] args);
        public static class ARIDelegates{
            public static long NotInfinite(long count, int requirement, params int[] args) => 0;
            public static long ItemCount(long count, int requirement, params int[] args) => count;
            public static long Requirement(long count, int requirement, params int[] args) => requirement;

            public static long LargestMultiple(long count, int requirement, params int[] args)
                => count / requirement * requirement;
            public static long LargestPower(long count, int requirement, params int[] args)
                => requirement * (long)MathF.Pow(args[0],(int)MathF.Log(count / (float)requirement, args[0]));
        }

        public static long CalculateInfinity(int type, int theoricalMaxStack, long count, int requirement, float multiplier = 1, AboveRequirementInfinity aboveRequirement = null, params int[] args) {

            requirement = Utility.RequirementToItems(requirement, type, theoricalMaxStack);
            if(requirement == 0 || count < requirement) return 0;

            long infinity = count == requirement ? requirement :
                (aboveRequirement ?? ARIDelegates.Requirement).Invoke(count, requirement, args);
            return (long)(infinity * multiplier);
        }
    }
}
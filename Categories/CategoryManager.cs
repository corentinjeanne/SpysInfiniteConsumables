using System;
using System.Collections.Generic;

using Terraria;

using SPIC.Categories;

namespace SPIC {

    namespace Categories {

        public struct TypeCategories {
            public readonly Ammo Ammo;
            public readonly Consumable? Consumable;
            public readonly Placeable Placeable;
            public readonly GrabBag? GrabBag;
            public readonly Material Material;

            public TypeCategories(Item item){
                Ammo = item.GetAmmoCategory();
                Consumable = item.GetConsumableCategory();
                Placeable = item.GetPlaceableCategory();
                GrabBag = item.GetGrabBagCategory();
                Material = item.GetMaterialCategory();
            }
        }

        public struct TypeRequirements {
            public readonly int Ammo;
            public readonly int Consumable;
            public readonly int GrabBag;
            public readonly int Placeable;
            public readonly int Material;

            public bool HasAnInfinity => Ammo != 0 ||  Consumable != 0 ||  Placeable != 0 ||  GrabBag != 0 ||  Material != 0;
            public TypeRequirements(Item item){
                Ammo = item.GetAmmoRequirement();
                Consumable = item.GetConsumableRequirement();
                GrabBag = item.GetGrabBagRequirement();
                Placeable = item.GetPlaceableRequirement();
                Material = item.GetMaterialRequirement();
            }
        }

        public struct TypeInfinities {

            // ? use a cutom struct
            public readonly int Ammo;
            public readonly int Consumable;
            public readonly int GrabBag;
            public readonly int Placeable;
            public readonly int Material;

            public bool AllInfinite => (Ammo == -2 || Ammo >= 0) && (Consumable == -2 || Consumable >= 0) && (Placeable == -2 || Placeable >= 0) && (GrabBag == -2 || GrabBag >= 0) && (Material == -2 || Material >= 0);
            public TypeInfinities(Player player, Item item) {
                int count = player.CountItems(item.type, true);
                Ammo = item.GetAmmoInfinity(count);
                Consumable = item.GetConsumableInfinity(count);
                GrabBag = item.GetGrabBagInfinity(count);
                Placeable = item.GetPlaceableInfinity(count);
                Material = item.GetMaterialInfinity(count);
                Ammo = item.GetAmmoInfinity(count);
            }
        }
    }

    public static class CategoryManager {

        public const uint CategoryCount = 6;

        private static readonly Dictionary<int, TypeCategories> _typeCategories = new();
        private static readonly Dictionary<int, TypeRequirements> _typeRequirements = new();
        
        private static readonly Dictionary<int, Currency> _currencyCategory = new();
        private static readonly Dictionary<int, int> _currencyRequirement = new();
        public static void ClearAll(){
            _typeCategories.Clear();
            _typeRequirements.Clear();
            _currencyCategory.Clear();
            _currencyRequirement.Clear();
        }
        public static void UpdateType(Item item){
            _typeCategories[item.type] = new(item);
            _typeRequirements[item.type] = new(item);
        }
        public static void UpdateCurrency(int currency){
            _currencyCategory[currency] = CurrencyExtension.GetCurrencyCategory(currency);
            _currencyRequirement[currency] = CurrencyExtension.GetCurrencyRequirement(currency);
        }
        
        public static TypeCategories GetTypeCategories(this Item item){
            if(!_typeCategories.ContainsKey(item.type)) _typeCategories.Add(item.type, new(item));
            return _typeCategories[item.type];
        }
        public static TypeRequirements GetTypeRequirements(this Item item){
            if (!_typeRequirements.ContainsKey(item.type)) _typeRequirements.Add(item.type, new(item));
            return _typeRequirements[item.type];
        }

        public static Currency GetCurrencyCategory(int currency){
            if(!_currencyCategory.ContainsKey(currency)) _currencyCategory[currency] = CurrencyExtension.GetCurrencyCategory(currency);
            return _currencyCategory[currency];
        }
        public static int GetCurrencyRequirement(int currency){
            if (!_currencyRequirement.ContainsKey(currency)) _currencyRequirement[currency] = CurrencyExtension.GetCurrencyRequirement(currency);
            return _currencyRequirement[currency];
        }
        public static bool HasAnInfinity(this Item item)
            => GetTypeRequirements(item).HasAnInfinity || GetCurrencyRequirement(item.CurrencyType()) != 0;
        
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
        public static long CalculateInfinity(int type, int theoricalMaxStack, long count, int requirement, float multiplier, AboveRequirementInfinity aboveRequirement = null, params int[] args)
            => CalculateInfinity (
                (int)MathF.Min(Globals.ConsumptionItem.MaxStack(type), theoricalMaxStack),
                count, requirement, multiplier, aboveRequirement, args
            );
        
        public static long CalculateInfinity(int maxStack, long count, int requirement, float multiplier, AboveRequirementInfinity aboveRequirement = null, params int[] args) {
            requirement = Utility.RequirementToItems(requirement, maxStack);
            if(requirement == 0) return -2;
            if(count < requirement) return -1;
            long infinity = count == requirement ? requirement :
                (aboveRequirement ?? ARIDelegates.Requirement).Invoke(count, requirement, args);
            return (long)(infinity * multiplier);
        }
        

        public static bool CanDisplayInfinities(this Item item, bool isACopy = false){
            Player player = Main.LocalPlayer;
            // bool crafting = !Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked && !Main.InReforgeMenu && !Main.LocalPlayer.tileEntityAnchor.InUse && !Main.hidePlayerCraftingMenu;
            // Recipe recipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
            if (isACopy) {
                return item.playerIndexTheItemIsReservedFor == Main.myPlayer && (
                    (Main.mouseItem.type == item.type && Main.mouseItem.stack == item.stack)
                    || Array.Find(player.inventory, i => i.type == item.type && i.stack == item.stack) is not null
                    || (player.InChest(out var chest) && Array.Find(chest, i => i.type == item.type && i.stack == item.stack) is not null)
                    // || crafting && (recipe.requiredItem.Find(i => i.type == item.type && i.stack == item.stack) is not null)
                    || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item))
                );
            } else {
                return item.playerIndexTheItemIsReservedFor == Main.myPlayer && (
                    Main.mouseItem == item
                    || Array.IndexOf(player.inventory, item) != -1
                    || (player.InChest(out Item[] chest) && Array.IndexOf(chest, item) != -1)
                    // || (crafting && recipe.requiredItem.Contains(item))
                    || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item))
                );
            }
        }
    }
}
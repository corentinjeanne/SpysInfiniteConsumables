using System;
using System.Collections.Generic;

using Terraria;

using SPIC.Categories;

namespace SPIC {

    namespace Categories {

        public enum Category {
            None,
            Ammo,
            Consumable,
            Placeable,
            GrabBag,
            Material,
            Currency
        }

        public struct ItemCategories {
            public readonly Ammo Ammo;
            public readonly Consumable? Consumable;
            public readonly Placeable Placeable;
            public readonly GrabBag? GrabBag;
            public readonly Material Material;

            public ItemCategories(Item item){
                Ammo = item.GetAmmoCategory();
                Consumable = item.GetConsumableCategory();
                Placeable = item.GetPlaceableCategory();
                GrabBag = item.GetGrabBagCategory();
                Material = item.GetMaterialCategory();
            }
        }

        public struct ItemRequirements {
            public readonly int Ammo;
            public readonly int Consumable;
            public readonly int GrabBag;
            public readonly int Placeable;
            public readonly int Material;

            public bool HasAnInfinity => Ammo != 0 ||  Consumable != 0 ||  Placeable != 0 ||  GrabBag != 0 ||  Material != 0;
            public ItemRequirements(Item item){
                Ammo = item.GetAmmoRequirement();
                Consumable = item.GetConsumableRequirement();
                GrabBag = item.GetGrabBagRequirement();
                Placeable = item.GetPlaceableRequirement();
                Material = item.GetMaterialRequirement();
            }
        }

        public struct ItemInfinities {

            // ? use a cutom struct (nullable, etc)
            public readonly int Ammo = -2;
            public readonly int Consumable = -2;
            public readonly int GrabBag = -2;
            public readonly int Placeable = -2;
            public readonly int Material = -2;

            public bool FullyInfinite => (Ammo == -2 || Ammo >= 0) && (Consumable == -2 || Consumable >= 0) && (Placeable == -2 || Placeable >= 0) && (GrabBag == -2 || GrabBag >= 0) && (Material == -2 || Material >= 0);
            public ItemInfinities(Player player, Item item) {
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

    public static class CategoryHelper {

        private static readonly Dictionary<int, ItemCategories> _categories = new();
        private static readonly Dictionary<int, ItemRequirements> _requirements = new();
        private static readonly Dictionary<int, Currency> _currencyCategories = new();
        private static readonly Dictionary<int, int> _currencyRequirements = new();
        public static void ClearAll(){
            _categories.Clear();
            _requirements.Clear();
            _currencyCategories.Clear();
            _currencyRequirements.Clear();
        }

        public static void UpdateItem(Item item){
            _categories.Remove(item.type, out _);
            _requirements.Remove(item.type, out _);
            _categories[item.type] = new(item);
            _requirements[item.type] = new(item);
        }
        public static void UpdateCurrency(int currency){
            _currencyCategories.Remove(currency, out _);
            _currencyRequirements.Remove(currency, out _);
            _currencyCategories[currency] = CurrencyExtension.GetCurrencyCategory(currency);
            _currencyRequirements[currency] = CurrencyExtension.GetCurrencyRequirement(currency);
        }
        
        public static ItemCategories GetCategories(this Item item){
            if(!_categories.ContainsKey(item.type)) _categories.Add(item.type, new(item));
            return _categories[item.type];
        }
        public static ItemRequirements GetRequirements(this Item item){
            if (!_requirements.ContainsKey(item.type)) _requirements.Add(item.type, new(item));
            return _requirements[item.type];
        }

        public static Currency GetCategory(int currency){
            if(!_currencyCategories.ContainsKey(currency)) _currencyCategories[currency] = CurrencyExtension.GetCurrencyCategory(currency);
            return _currencyCategories[currency];
        }
        public static int GetRequirement(int currency){
            if (!_currencyRequirements.ContainsKey(currency)) _currencyRequirements[currency] = CurrencyExtension.GetCurrencyRequirement(currency);
            return _currencyRequirements[currency];
        }
        public static bool HasAnInfinity(this Item item)
            => item.GetRequirements().HasAnInfinity || GetRequirement(item.CurrencyType()) != 0;
        

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
                (int)MathF.Min(Globals.SpicItem.MaxStack(type), theoricalMaxStack),
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
            bool crafting = !Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked && !Main.InReforgeMenu && !Main.LocalPlayer.tileEntityAnchor.InUse && !Main.hidePlayerCraftingMenu;
            Recipe recipe = Main.recipe[Main.availableRecipe[Main.focusRecipe]];
            if (isACopy) {
                return item.playerIndexTheItemIsReservedFor == Main.myPlayer && (
                    (Main.mouseItem.type == item.type && Main.mouseItem.stack == item.stack)
                    || Array.Find(player.inventory, i => i.type == item.type && i.stack == item.stack) is not null
                    || (player.InChest(out var chest) && Array.Find(chest, i => i.type == item.type && i.stack == item.stack) is not null)
                    || crafting && (recipe.requiredItem.Find(i => i.type == item.type && i.stack == item.stack) is not null)
                    || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item, isACopy))
                );
            } else {
                return item.playerIndexTheItemIsReservedFor == Main.myPlayer && (
                    Main.mouseItem == item
                    || Array.IndexOf(player.inventory, item) != -1
                    || (player.InChest(out Item[] chest) && Array.IndexOf(chest, item) != -1)
                    || (crafting && recipe.requiredItem.Contains(item))
                    || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item, isACopy))
                );
            }

        }
    }
}
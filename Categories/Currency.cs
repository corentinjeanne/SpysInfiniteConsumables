using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.UI;
using Terraria.GameContent.Creative;

using System.Reflection;

using SPIC.Categories;
using Terraria.ID;

namespace SPIC {

    namespace Categories {
        public enum Currency {
            None,
            Coin,
            SingleCoin,
        }
    }

    public static class CurrencyExtension {

        public static int MaxStack(this Currency Currency) => Currency switch {
            Currency.Coin => 100,
            Currency.SingleCoin => 999,
            _ => 999,
        };
        

        public static int Requirement(this Currency Currency) {
            Configs.Requirements inf = Configs.Requirements.Instance;
            return Currency switch {
                Currency.Coin => inf.currency_Coins,
                Currency.SingleCoin => inf.currency_Single,
                _ => 0,
            };
        }

        public static Currency GetCurrencyCategory(int currency) {
            if(currency == -1) return Currency.Coin;
            if(!_currencies.ContainsKey(currency)) return Currency.None;
            return _currencies[currency].values.Count == 1 ? Currency.SingleCoin : Currency.Coin;
        }

        public static int GetCurrencyRequirement(int currency) {
            // TODO move journey requirements
            Currency Currency = CategoryHelper.GetCategory(currency);
            return Currency.Requirement();
        }

        public static long GetCurrencyInfinity(this Player player, int currency)
            => GetCurrencyInfinity(currency, player.CountCurrency(currency,true, true));

        public static long GetCurrencyInfinity(int currency, long currencyCount) {
            Currency category = CategoryHelper.GetCategory(currency);
            return category == Currency.Coin ?
                CategoryHelper.CalculateInfinity(category.MaxStack(), currencyCount, CategoryHelper.GetRequirement(currency), 0.1f, CategoryHelper.ARIDelegates.LargestPower, 100):
                CategoryHelper.CalculateInfinity(category.MaxStack(), currencyCount, CategoryHelper.GetRequirement(currency), 0.2f, CategoryHelper.ARIDelegates.LargestMultiple);
        }

        public static int CurrencyType(this Item item) {
            if (item.IsACoin) return -1;
            foreach (int key in _currencies.Keys) {
                if (_currencies[key].system.Accepts(item)) return key;
            }
            return -2;
        }
        public static bool IsPartOfACurrency(this Item item, out int currency) => (currency = item.CurrencyType()) != -2;


        public static long CountCurrency(this Item[] container, int currency, params int[] ignoreSlots) {
            switch (currency) {
            case -2: return 0L;
            case -1:
                return Utils.CoinsCount(out _, container, ignoreSlots); /* container.CountCoins(ignoreSlots); */
            default: 
                CustomCurrencySystem system = _currencies[currency].system;
                long cap = system.CurrencyCap;
                system.SetCurrencyCap(long.MaxValue);
                long count = system.CountCurrency(out _, container, ignoreSlots);
                system.SetCurrencyCap(cap);
                return count;
            }
        }

        public static long CountCurrency(this Player player, int currency, bool includeBanks = true, bool includeChest = false) {
            long count = player.inventory.CountCurrency(currency, 58);
            count += new Item[]{Main.mouseItem}.CountCurrency(currency);
            if(includeBanks)
                count += player.bank.item.CountCurrency(currency) + player.bank2.item.CountCurrency(currency) + player.bank3.item.CountCurrency(currency) + player.bank4.item.CountCurrency(currency);
            if (includeChest && player.InChest(out Item[] chest)) count += chest.CountCurrency(currency);

            return count;
        }

        public static List<KeyValuePair<int,long>> CurrencyCountToItems(int currency, long amount) {
            List<KeyValuePair<int,int>> values = new();
            switch (currency) {
                case -2: return new();
                case -1:
                    values = new() {
                        new(ItemID.PlatinumCoin, 1000000),
                        new(ItemID.GoldCoin, 10000),
                        new(ItemID.SilverCoin, 100),
                        new(ItemID.CopperCoin, 1)
                    };
                    break;
                default:
                    foreach (var v in _currencies[currency].values)
                        values.Add(v);
                    values.Sort((a, b) => a.Value < b.Value ? 0 : 1);
                    break;
            }

            List<KeyValuePair<int, long>> stacks = new();
            foreach(var coin in values){
                int count = (int)(amount / coin.Value);
                if(count == 0) continue;
                amount -= count * coin.Value;
                stacks.Add(new(coin.Key, count));
            }
            return stacks;
        }

        internal struct CustomCurrencyData {
            public readonly CustomCurrencySystem system;
            public readonly Dictionary<int, int> values;

            public CustomCurrencyData(CustomCurrencySystem system, Dictionary<int, int> values) {
                this.system = system; this.values = values;
            }
        }

        public static List<int> Currencies => new(_currencies.Keys);
        private static Dictionary<int, CustomCurrencyData> _currencies;
        internal static void ClearCurrencies() => _currencies = null;

        internal static void GetCurrencies() {
            FieldInfo cur = typeof(CustomCurrencyManager).GetField("_currencies", BindingFlags.NonPublic | BindingFlags.Static);
            Dictionary<int, CustomCurrencySystem> currencies = (Dictionary<int, CustomCurrencySystem>)cur.GetValue(null);
            _currencies = new();
            foreach (var (key, system) in currencies){
                FieldInfo values = typeof(CustomCurrencySystem).GetField("_valuePerUnit", BindingFlags.NonPublic | BindingFlags.Instance);
                _currencies[key] = new(system, (Dictionary<int, int>)values.GetValue(system));
            }
        }
    }
}
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
            Configs.Infinities inf = Configs.Infinities.Instance;
            return Currency switch {
                Currency.Coin => inf.currency_Coins,
                Currency.SingleCoin => inf.currency_Single,
                _ => 0,
            };
        }
        public static int CurrencyType(this Item item) {
            if (item.IsACoin) return -1;
            
            foreach (int key in _currencies.Keys) {
                if (_currencies[key].system.Accepts(item)) return key;
            }
            return -2;
        }

        public static bool IsPartOfACurrency(this Item item, out int currency) {
            return (currency = item.CurrencyType()) != -2;
        }

        internal struct CustomCurrencyData {
            public readonly CustomCurrencySystem system;
            public readonly Dictionary<int, int> values;

            public CustomCurrencyData(CustomCurrencySystem system, Dictionary<int, int> values) {
                this.system = system; this.values = values;
            }
        }
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

        public static Currency GetCurrencyCategory(this Item item) {

            return !item.IsPartOfACurrency(out int currency)
                ? Currency.None
                : currency == -1 ? Currency.Coin : _currencies[currency].values.Count == 1 ? Currency.SingleCoin : Currency.Coin;
        }

        public static int GetCurrencyRequirement(this Item item) {
            Configs.Infinities config = Configs.Infinities.Instance;
            Currency Currency = Category.GetCategories(item).Currency;
            return Currency != Currency.None && config.JourneyRequirement
                ? CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]
                : Currency.Requirement();
        }
        public static long CountCurrency(this Player player, int currency) {
            switch (currency) {
            case -2: return 0;
            case -1:
                // FIXME overflow with max value
                return Utils.CoinsCombineStacks(out _,
                    Utils.CoinsCount(out _, player.inventory),
                    Utils.CoinsCount(out _, player.bank.item),
                    Utils.CoinsCount(out _, player.bank2.item),
                    Utils.CoinsCount(out _, player.bank3.item),
                    Utils.CoinsCount(out _, player.bank4.item)
                );
            default: {
                CustomCurrencySystem system = _currencies[currency].system;
                long cap = system.CurrencyCap;
                system.SetCurrencyCap(long.MaxValue);
                long count = system.CombineStacks(out _,
                    system.CountCurrency(out _, player.inventory),
                    system.CountCurrency(out _, player.bank.item),
                    system.CountCurrency(out _, player.bank2.item),
                    system.CountCurrency(out _, player.bank3.item),
                    system.CountCurrency(out _, player.bank4.item)
                );
                system.SetCurrencyCap(cap);
                return count;
            }
            }
        }

        public static long GetCurrencyInfinity(this Player player, Item item) {
            if (!item.IsPartOfACurrency(out int currency)) return 0;
            long count = player.CountCurrency(currency);

            Currency category = Category.GetCategories(item).Currency;
            return category == Currency.Coin ?
                Category.Infinity(item.type, category.MaxStack(), count, Category.GetRequirements(item).Currency, 0.1f, Category.ARIDelegates.LargestPower, 100):
                Category.Infinity(item.type, category.MaxStack(), count, Category.GetRequirements(item).Currency, 0.2f, Category.ARIDelegates.LargestMultiple);
        }

        public static List<KeyValuePair<int,long>> ToCurrencyStacks(int currency, long amount) {
            List<KeyValuePair<int,int>> values = new();
            if(currency == -1){
                values = new() {
                    new(ItemID.PlatinumCoin, 1000000),
                    new(ItemID.GoldCoin, 10000),
                    new(ItemID.SilverCoin, 100),
                    new(ItemID.CopperCoin, 1)
                };
            }
            else {
                foreach (var v in _currencies[currency].values)
                    values.Add(v);
                values.Sort((a, b) => a.Value < b.Value ? 0 : 1);
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
    }
}
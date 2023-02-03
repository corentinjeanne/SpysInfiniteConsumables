using System.Reflection;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.UI;

namespace SPIC;

public struct CustomCurrencyData {
    public readonly CustomCurrencySystem system;
    public readonly Dictionary<int, int> values;

    public CustomCurrencyData(CustomCurrencySystem system, Dictionary<int, int> values) {
        this.system = system; this.values = values;
    }
}

public static class CurrencyHelper {

    public const int Coins = -1;
    public const int None = -2;


    public static int CurrencyType(this Item item) {
        if (item.IsACoin) return Coins;
        foreach (int key in _currencies.Keys) {
            if (_currencies[key].system.Accepts(item)) return key;
        }
        return None;
    }
    public static bool IsPartOfACurrency(this Item item, out int currency) => (currency = item.CurrencyType()) != None;
    
    public static long CurrencyValue(this Item item) => item.CurrencyType() switch {
        None => 0,
        Coins => item.value / 5,
        int t => _currencies[t].values[item.type]
    };
    public static int LowestValueType(int currency) {
        if (currency == None) return ItemID.None;
        if (currency == Coins) return ItemID.CopperCoin;
        
        int minType = 0;
        int minValue = 0;
        foreach ((int key, int value) in CurrencySystems(currency).values) {
            if (minValue == 0 || value < minValue) (minType, minValue) = (key, value);
        }
        return minType;
    }

    public static long CountCurrency(this Item[] container, int currency, params int[] ignoreSlots) {
        long count;
        switch (currency) {
        case None: return 0L;
        case Coins:
            count = 0L;
            for (int i = 0; i < container.Length; i++) {
                if (System.Array.IndexOf(ignoreSlots, i) == -1 && container[i].IsACoin)
                    count += (long)container[i].value / 5 * container[i].stack;
            }
            return count;
        default:
            CustomCurrencySystem system = _currencies[currency].system;
            long cap = system.CurrencyCap;
            system.SetCurrencyCap(long.MaxValue);
            count = system.CountCurrency(out _, container, ignoreSlots);
            system.SetCurrencyCap(cap);
            return count;
        }
    }
    public static long CountCurrency(this Player player, int currency, bool includeBanks = true, bool includeChest = false) {
        long count = player.inventory.CountCurrency(currency, 58);
        count += new Item[] { Main.mouseItem }.CountCurrency(currency);
        if (includeBanks) count += player.bank.item.CountCurrency(currency)
                                + player.bank2.item.CountCurrency(currency)
                                + player.bank3.item.CountCurrency(currency)
                                + player.bank4.item.CountCurrency(currency);
        if (includeChest && player.InChest(out Item[]? chest)) count += chest.CountCurrency(currency);

        return count;
    }


    public static List<KeyValuePair<int, long>> CurrencyCountToItems(int currency, long amount) {
        List<KeyValuePair<int, int>> values = new();
        switch (currency) {
        case None: return new();
        case Coins:
            values = new() {
                        new(ItemID.PlatinumCoin, 1000000),
                        new(ItemID.GoldCoin, 10000),
                        new(ItemID.SilverCoin, 100),
                        new(ItemID.CopperCoin, 1)
                    };
            break;
        default:
            foreach (var v in _currencies[currency].values) values.Add(v);
            values.Sort((a, b) => a.Value < b.Value ? 0 : 1);
            break;
        }

        List<KeyValuePair<int, long>> stacks = new();
        foreach (var coin in values) {
            int count = (int)(amount / coin.Value);
            if (count == 0) continue;
            amount -= count * coin.Value;
            stacks.Add(new(coin.Key, count));
        }
        return stacks;
    }


    public static string PriceText(int currency, long count) {
        if (count == 0 || currency == None) return "";

        List<KeyValuePair<int, long>> coins = CurrencyCountToItems(currency, count);
        List<string> parts = new();
        switch (currency) {
        case Coins:
            foreach (KeyValuePair<int, long> coin in coins) parts.Add($"{coin.Value} {Lang.inter[18 - coin.Key + ItemID.CopperCoin].Value}");
            break;
        default:
            foreach (KeyValuePair<int, long> coin in coins) parts.Add($"{coin.Value} {Lang.GetItemNameValue(coin.Key)}");
            break;
        }
        return string.Join(' ', parts);
    }


    public static CustomCurrencyData CurrencySystems(int currency) => _currencies[currency];
    public static List<int> Currencies => new(_currencies.Keys);


    internal static void GetCurrencies() {
        FieldInfo curField = typeof(CustomCurrencyManager).GetField("_currencies", BindingFlags.NonPublic | BindingFlags.Static)!;
        FieldInfo valuesField = typeof(CustomCurrencySystem).GetField("_valuePerUnit", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Dictionary<int, CustomCurrencySystem> currencies = (Dictionary<int, CustomCurrencySystem>)curField.GetValue(null)!;
        _currencies = new();
        foreach (var (key, system) in currencies) {
            _currencies[key] = new(system, (Dictionary<int, int>)valuesField.GetValue(system)!);
        }
    }
    internal static void ClearCurrencies() => _currencies = null;

#nullable disable
    private static Dictionary<int, CustomCurrencyData> _currencies;
#nullable restore
}
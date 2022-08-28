using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public enum CurrencyCategory {
    None = Infinity.NoCategory,
    Coin,
    SingleCoin,
}

public class Currency : Infinity<Currency> {

    public override int MaxStack(byte category) => (CurrencyCategory)category switch {
        CurrencyCategory.Coin => 100,
        CurrencyCategory.SingleCoin => 999,
        CurrencyCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        Configs.Requirements inf = Configs.Requirements.Instance;
        return (CurrencyCategory)category switch {
            CurrencyCategory.Coin => inf.currency_Coins,
            CurrencyCategory.SingleCoin => inf.currency_Single,
            CurrencyCategory.None or _ => Infinity.NoRequirement
        };
    }

    public override int Type(Item item) => item.CurrencyType();
    public override Item ItemFromType(int type) {
        if (type == -1) return new(Terraria.ID.ItemID.CopperCoin);
        foreach ((int key, _) in CurrencyHelper.CurrencySystems(type).values) return new(key);
        return default;
    }
    public override long CountItems(Player player, int type) => player.CountCurrency(type, true);


    public override bool Enabled => Configs.Requirements.Instance.InfiniteCurrencies;

    public override byte GetCategory(Item item) => GetCategory(Type(item));
    public override byte GetCategory(int currency) {
        if (currency == -1) return (byte)CurrencyCategory.Coin;
        if (!CurrencyHelper.Currencies.Contains(currency)) return (byte)CurrencyCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? (byte)CurrencyCategory.SingleCoin : (byte)CurrencyCategory.Coin;
    }

    // public override long GetInfinity(Player player, int type) => GetInfinity(..., CountItems(player, type));
    // public override long GetInfinity(Player player, Item item) => GetInfinity(player, Type(item)); 

    public override long GetInfinity(Item item, long count) {
        CurrencyCategory category = (CurrencyCategory)InfinityManager.GetCategory(item, ID);
        int requirement = InfinityManager.GetRequirement(item, ID);
        return category == CurrencyCategory.Coin ?
            InfinityManager.CalculateInfinity(MaxStack((byte)category), count, requirement, 0.1f, InfinityManager.ARIDelegates.LargestPower, 100) :
            InfinityManager.CalculateInfinity(MaxStack((byte)category), count, requirement, 0.2f, InfinityManager.ARIDelegates.LargestMultiple);
    }

    public override bool IsFullyInfinite(Item item, long infinity) {
        int currency = Type(item);
        long cost;
        if (Main.npcShop != 0) cost = Globals.SpicNPC.HighestPrice(currency);
        else if (Main.InReforgeMenu) cost = Main.reforgeItem.value;
        else cost = Globals.SpicNPC.HighestItemValue(currency);
        return infinity >= cost;
    }

    public override KeyValuePair<int, long>[] GetPartialInfinity(Item item, long infinity)
        => CurrencyHelper.CurrencyCountToItems(item.CurrencyType(), infinity).ToArray();

    public override Microsoft.Xna.Framework.Color Color => Configs.InfinityDisplay.Instance.color_Currencies;

    public override TooltipLine TooltipLine => AddedLine("Currencycat", Terraria.Localization.Language.GetTextValue("Mods.SPIC.Categories.Currency.name"));
    public override string MissingLinePosition => "Consumable";
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.Currency.{(CurrencyCategory)category}";
}

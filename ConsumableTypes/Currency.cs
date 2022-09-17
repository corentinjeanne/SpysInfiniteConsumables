using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

public enum CurrencyCategory {
    None = ConsumableType.NoCategory,
    Coin,
    SingleCoin,
}

public class CurrencyRequirements {
    [Label("$Mods.SPIC.Types.Currency.coins")]
    public Configs.Requirement Coins = -10;
    [Label("$Mods.SPIC.Types.Currency.custom")]
    public Configs.Requirement Single = 50;
}

public class Currency : ConsumableType<Currency>, IPartialInfinity {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Types.Currency.name");

    public override int MaxStack(byte category) => (CurrencyCategory)category switch {
        CurrencyCategory.Coin => 100,
        CurrencyCategory.SingleCoin => 999,
        CurrencyCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        CurrencyRequirements reqs = (CurrencyRequirements)ConfigRequirements;
        return (CurrencyCategory)category switch {
            CurrencyCategory.Coin => reqs.Coins,
            CurrencyCategory.SingleCoin => reqs.Single,
            CurrencyCategory.None or _ => NoRequirement
        };
    }

    public override long CountItems(Player player, Item item) {
        long value = item.CurrencyValue();
        return value == 0 ? 0 : player.CountCurrency(item.CurrencyType(), true) / value;
    }

    // public override byte GetCategory(int currency) {
    public override byte GetCategory(Item item) { // => GetCategory(Type(item));
        int currency = item.CurrencyType();
        if (currency == -1) return (byte)CurrencyCategory.Coin;
        if (!CurrencyHelper.Currencies.Contains(currency)) return (byte)CurrencyCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? (byte)CurrencyCategory.SingleCoin : (byte)CurrencyCategory.Coin;
    }
    // public override int GetRequirement(int type) => Requirement(InfinityManager.GetCategory(type, UID));
    // public override int GetRequirement(Item item) => GetRequirement(Type(item));


    // public override long GetInfinity(int type, long count) {
    public override long GetInfinity(Item item, long count) { //  => GetInfinity(Type(item), count);
        CurrencyCategory category = (CurrencyCategory)InfinityManager.GetCategory(item, ID);
        float mult;
        InfinityManager.AboveRequirementInfinity ari;
        int[] args;
        if (category == CurrencyCategory.Coin) (mult, ari, args) = (0.1f, InfinityManager.ARIDelegates.LargestPower, new[] { 100 });
        else (mult, ari, args) = (0.2f, InfinityManager.ARIDelegates.LargestMultiple, null);

        return InfinityManager.CalculateInfinity(MaxStack((byte)category), count, InfinityManager.GetRequirement(item, ID), mult, ari, args);
    }

    public long GetFullInfinity(Player player, Item item) {
        int currency = item.CurrencyType();
        long value = item.CurrencyValue();
        long cost;
        if (Main.InReforgeMenu) cost = Main.reforgeItem.value;
        else if (Main.npcShop != 0) cost = Globals.SpicNPC.HighestPrice(currency);
        else cost = Globals.SpicNPC.HighestItemValue(currency);
        return value == 0 ? NotInfinite : cost / value + (cost % value == 0 ? 0 : 1);
    }

    public KeyValuePair<int, long>[] GetPartialInfinity(Item item, long infinity)
        => CurrencyHelper.CurrencyCountToItems(item.CurrencyType(), infinity * item.CurrencyValue()).ToArray();

    public override Microsoft.Xna.Framework.Color DefaultColor() => Colors.CoinGold;// new(255, 255, 70);

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.Categories.Currency.name"));
    public override string MissingLinePosition => "Consumable";
    public override string LocalizedCategoryName(byte category) => ((CurrencyCategory)category).ToString();

    public override CurrencyRequirements CreateRequirements() => new();
}
/*
use type => lowest value (e.g. copper coin)
use item => in item amount
Get inf silver coins => 100 
Get inf gold coins => 1
... 
*/
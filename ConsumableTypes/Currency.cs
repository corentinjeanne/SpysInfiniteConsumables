using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;

public enum CurrencyCategory : byte {
    None = Category.None,
    Coin,
    SingleCoin,
}

public class CurrencyRequirements {
    [Label("$Mods.SPIC.Types.Currency.coins")]
    public Configs.Requirement Coins = -10;
    [Label("$Mods.SPIC.Types.Currency.custom")]
    public Configs.Requirement Single = 50;
}

public class Currency : ConsumableType<Currency>, IStandardConsumableType<CurrencyCategory, CurrencyRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.LuckyCoin;

    public bool DefaultsToOn => false;
    public CurrencyRequirements Settings { get; set; }

    public int MaxStack(CurrencyCategory category) => category switch {
        CurrencyCategory.Coin => 100,
        CurrencyCategory.SingleCoin => 999,
        CurrencyCategory.None or _ => 999,
    };

    public int Requirement(CurrencyCategory category) => category switch {
        CurrencyCategory.Coin => Settings.Coins,
        CurrencyCategory.SingleCoin => Settings.Single,
        CurrencyCategory.None or _ => IConsumableType.NoRequirement
    };
    

    public long CountItems(Player player, Item item) {
        long value = item.CurrencyValue();
        return value == 0 ? 0 : player.CountCurrency(item.CurrencyType(), true) / value;
    }

    public CurrencyCategory GetCategory(Item item) {
        int currency = item.CurrencyType();
        if (currency == -1) return CurrencyCategory.Coin;
        if (!CurrencyHelper.Currencies.Contains(currency)) return CurrencyCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coin;
    }

    public long GetInfinity(Item item, long count) {
        CurrencyCategory category = InfinityManager.GetCategory<CurrencyCategory>(item, ID);
        float mult;
        InfinityManager.AboveRequirementInfinity ari;
        int[] args;
        if (category == CurrencyCategory.Coin) (mult, ari, args) = (0.1f, InfinityManager.ARIDelegates.LargestPower, new[] { 100 });
        else (mult, ari, args) = (0.2f, InfinityManager.ARIDelegates.LargestMultiple, null);

        return InfinityManager.CalculateInfinity(MaxStack(category), count, InfinityManager.GetRequirement(item, ID), mult, ari, args);
    }

    public long GetMaxInfinity(Player player, Item item) {
        int currency = item.CurrencyType();
        long value = item.CurrencyValue();
        long cost;
        if (Main.InReforgeMenu) cost = Main.reforgeItem.value;
        else if (Main.npcShop != 0) cost = Globals.SpicNPC.HighestPrice(currency);
        else cost = Globals.SpicNPC.HighestItemValue(currency);
        return value == 0 ? IConsumableType.NotInfinite : cost / value + (cost % value == 0 ? 0 : 1);
    }

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.CoinGold;

    public TooltipLine TooltipLine => TooltipHelper.AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.Types.Currency.name"));
    public string LinePosition => "Consumable";
}
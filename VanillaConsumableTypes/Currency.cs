using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using Terraria.Localization;

namespace SPIC.VanillaConsumableTypes;

public enum CurrencyCategory : byte {
    None = Category.None,
    Coin,
    SingleCoin,
}

public class CurrencyRequirements {
    [Label("$Mods.SPIC.Types.Currency.coins")]
    public int Coins = 10;
    [Label("$Mods.SPIC.Types.Currency.custom")]
    public int Single = 50;
}

public class Currency : StandardGroup<Currency, int, CurrencyCategory>, IConfigurable<CurrencyRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.LuckyCoin;

    public override bool DefaultsToOn => false;
#nullable disable
    public CurrencyRequirements Settings { get; set; }
#nullable restore

    public override IRequirement Requirement(CurrencyCategory category) => category switch {
        CurrencyCategory.Coin => new PowerRequirement(new CurrencyCount(CurrencyHelper.None, Settings.Coins * 100), 100, 0.1f),
        CurrencyCategory.SingleCoin => new MultipleRequirement(new CurrencyCount(CurrencyHelper.None, Settings.Single), 0.2f),
        CurrencyCategory.None or _ => new NoRequirement()     
    };

    public override long CountConsumables(Player player, int currency) => currency == CurrencyHelper.None ? 0 : player.CountCurrency(currency, true);

    public override CurrencyCategory GetCategory(int currency) {
        if (currency == CurrencyHelper.Coins) return CurrencyCategory.Coin;
        if (!CurrencyHelper.Currencies.Contains(currency)) return CurrencyCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coin;
    }

    public override int ToConsumable(Item item) => item.CurrencyType();

    public override int CacheID(int consumable) => consumable;

    public override long GetMaxInfinity(Player player, int currency) {
        if (Main.InReforgeMenu) return Main.reforgeItem.value;
        else if (Main.npcShop != 0) return Globals.SpicNPC.HighestPrice(currency);
        else return Globals.SpicNPC.HighestItemValue(currency);
    }

    public override ICount LongToCount(int consumable, long count) => new CurrencyCount(consumable, count);

    public override string Key(int consumable) => new ItemDefinition(CurrencyHelper.LowestValueType(consumable)).ToString();

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.CoinGold;

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.ItemTooltip.curency"));
    public override string LinePosition => "Consumable";
}
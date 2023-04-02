using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using Terraria.Localization;
using SPIC.Configs;

namespace SPIC.VanillaGroups;

public enum CurrencyCategory : byte {
    None = CategoryHelper.None,
    Coin,
    SingleCoin,
}

public class CurrencyRequirements {
    [Label($"${Localization.Keys.Groups}.Currency.Coins")]
    public UniversalCountWrapper Coins = new() {Value = 10};
    [Label($"${Localization.Keys.Groups}.Currency.Custom")]
    public UniversalCountWrapper Single = new() {Value = 10};
}

public class Currency : StandardGroup<Currency, int, CurrencyCount, CurrencyCategory>, IConfigurable<CurrencyRequirements>, IColorable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.Currency.Name");
    public override int IconType => ItemID.LuckyCoin;

    public override bool DefaultsToOn => false;

    public override Requirement<CurrencyCount> GetRequirement(CurrencyCategory category, int currency) => category switch {
        CurrencyCategory.Coin => new PowerRequirement<CurrencyCount>(this.Settings().Coins.As<CurrencyCount>().Multiply(100), 10, 1 / 50f, LongToCount(currency, GetMaxInfinity(currency))),
        CurrencyCategory.SingleCoin => new MultipleRequirement<CurrencyCount>(this.Settings().Single.As<CurrencyCount>(), 0.2f, LongToCount(currency, GetMaxInfinity(currency))),
        CurrencyCategory.None or _ => new NoRequirement<CurrencyCount>()
    };

    public override long CountConsumables(Player player, int currency) => currency == CurrencyHelper.None ? 0 : player.CountCurrency(currency, true);

    public override CurrencyCategory GetCategory(int currency) {
        if (currency == CurrencyHelper.Coins) return CurrencyCategory.Coin;
        if (!CurrencyHelper.Currencies.Contains(currency)) return CurrencyCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coin;
    }

    public override int ToConsumable(Item item) => item.CurrencyType();

    public override int CacheID(int consumable) => consumable;

    public static long GetMaxInfinity(int currency) {
        if (Main.InReforgeMenu) return Main.reforgeItem.value;
        else if (Main.npcShop != 0) return Globals.ConsumptionNPC.HighestShopValue(currency);
        else return long.MaxValue;
    }

    public override CurrencyCount LongToCount(int consumable, long count) => new(consumable, count);

    public override string Key(int consumable) => new ItemDefinition(CurrencyHelper.LowestValueType(consumable)).ToString();

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.CoinGold;

    public override TooltipLineID LinePosition => TooltipLineID.Consumable;
}
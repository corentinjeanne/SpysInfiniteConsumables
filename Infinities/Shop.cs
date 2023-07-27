using Terraria.ModLoader.Config;

using SPIC.Configs;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SPIC.Infinities;

public enum ShopCategory {
    None,
    Coin,
    SingleCoin,
}

public class ShopRequirements {
    [LabelKey($"${Localization.Keys.Infinties}.Shop.Coins"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/20")]
    public Count Coins = 1000;
    [LabelKey($"${Localization.Keys.Infinties}.Shop.Custom"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/5")]
    public Count Single = 20;

    public const float CoinMult = 1 / 20f;
    public const float SingleCoinMult = 1 / 5f;
}

public class Shop : InfinityStatic<Shop, Currencies, int, ShopCategory> {

    public override int IconType => ItemID.LuckyCoin;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.CoinGold;


    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<ShopRequirements>(this);
    }

    public override Requirement GetRequirement(ShopCategory category) => category switch {
        ShopCategory.Coin => new(Config.Value.Coins, ShopRequirements.CoinMult),
        ShopCategory.SingleCoin => new(Config.Value.Single, ShopRequirements.SingleCoinMult),
        ShopCategory.None or _ => new()
    };

    public override ShopCategory GetCategory(int currency) {
        if (currency == CurrencyHelper.Coins) return ShopCategory.Coin;
        if (!CurrencyHelper.Currencies.Contains(currency)) return ShopCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? ShopCategory.SingleCoin : ShopCategory.Coin;
    }
    
    public Wrapper<ShopRequirements> Config = null!;
}
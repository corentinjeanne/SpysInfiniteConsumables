using Terraria.ModLoader.Config;

using SPIC.Configs;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.Localization;

namespace SPIC.Infinities;

public enum ShopCategory {
    None,
    Coins,
    SingleCoin,
}

public sealed class ShopRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.Shop.Coins"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/20")]
    public Count Coins = 1000;
    [LabelKey($"${Localization.Keys.Infinities}.Shop.SingleCoin"), TooltipKey($"${Localization.Keys.UI}.InfinityMultiplier"), TooltipArgs("1/5")]
    public Count SingleCoin = 20;

    public const float CoinMult = 1 / 20f;
    public const float SingleCoinMult = 1 / 5f;
}

public sealed class Shop : InfinityStatic<Shop, Currencies, int, ShopCategory> {

    public override int IconType => ItemID.LuckyCoin;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.CoinGold;

    public override void Load() {
        base.Load();
        DisplayOverrides += CoinSlots;
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<ShopRequirements>(this);
    }

    public override Requirement GetRequirement(ShopCategory category) => category switch {
        ShopCategory.Coins => new(Config.Value.Coins, ShopRequirements.CoinMult),
        ShopCategory.SingleCoin => new(Config.Value.SingleCoin, ShopRequirements.SingleCoinMult),
        _ => new()
    };

    public override ShopCategory GetCategory(int currency) {
        if (currency == CurrencyHelper.Coins) return ShopCategory.Coins;
        if (!CurrencyHelper.Currencies.Contains(currency)) return ShopCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? ShopCategory.SingleCoin : ShopCategory.Coins;
    }

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => item.CurrencyType() == CustomCurrencyID.DefenderMedals ? ((TooltipLine, TooltipLineID?))(new(Mod, "Tooltip0", Language.GetTextValue("ItemTooltip.DefenderMedal")), TooltipLineID.Tooltip) : base.GetTooltipLine(item);

    public Wrapper<ShopRequirements> Config = null!;

    public static void CoinSlots(Player player, Item item, int consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (50 <= index && index < 54) visibility = InfinityVisibility.Exclusive;
    }
}
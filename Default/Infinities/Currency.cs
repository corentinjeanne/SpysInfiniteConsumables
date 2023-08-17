using Terraria.ModLoader.Config;
using SPIC.Configs;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.Localization;

namespace SPIC.Default.Infinities;

public enum CurrencyCategory {
    None,
    Coins,
    SingleCoin,
}

public sealed class CurrencyRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Multiplier.Label"), LabelArgs($"${Localization.Keys.Infinities}.Currency.Coins")]
    public float Coins = 1/20f;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Multiplier.Label"), LabelArgs($"${Localization.Keys.Infinities}.Currency.SingleCoin")]
    public float SingleCoin = 1/5f;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Shop.Label")]
    public bool Shop = true;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Nurse.Label")]
    public bool Nurse = true;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Reforging.Label")]
    public bool Reforging = true;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Others.Label")]
    public bool Others = true;
}

public sealed class Currency : InfinityStatic<Currency, Currencies, int, CurrencyCategory> {

    public override int IconType => ItemID.LuckyCoin;
    public override bool Enabled { get; set; } = false;
    public override Color Color { get; set; } = Colors.CoinGold;

    public override void Load() {
        base.Load();
        DisplayOverrides += CoinSlots;
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<CurrencyRequirements>(this);
    }

    public override Requirement GetRequirement(CurrencyCategory category) => category switch {
        CurrencyCategory.Coins => new(10000, Config.Value.Coins),
        CurrencyCategory.SingleCoin => new(20, Config.Value.SingleCoin),
        _ => new()
    };

    public override CurrencyCategory GetCategory(int currency) {
        if (currency == CurrencyHelper.Coins) return CurrencyCategory.Coins;
        if (!CurrencyHelper.Currencies.Contains(currency)) return CurrencyCategory.None;
        return CurrencyHelper.CurrencySystems(currency).values.Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coins;
    }

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => displayed == CustomCurrencyID.DefenderMedals ? ((TooltipLine, TooltipLineID?))(new(Mod, "Tooltip0", Language.GetTextValue("ItemTooltip.DefenderMedal")), TooltipLineID.Tooltip) : base.GetTooltipLine(item, displayed);

    public static Wrapper<CurrencyRequirements> Config = null!;

    public static void CoinSlots(Player player, Item item, int consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (50 <= index && index < 54) visibility = InfinityVisibility.Exclusive;
    }
}
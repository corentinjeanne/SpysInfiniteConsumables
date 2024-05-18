using Terraria.ModLoader.Config;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.Localization;
using System.ComponentModel;
using SPIC.Default.Displays;
using SpikysLib.Extensions;

namespace SPIC.Default.Infinities;

public enum CurrencyCategory {
    None,
    Coins,
    SingleCoin,
}

public sealed class CurrencyRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Multiplier"), LabelArgs($"${Localization.Keys.Infinities}.Currency.Coins")]
    [DefaultValue(1/20f)] public float Coins = 1/20f;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Multiplier"), LabelArgs($"${Localization.Keys.Infinities}.Currency.SingleCoin")]
    [DefaultValue(1/5f)] public float SingleCoin = 1/5f;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Shop")]
    [DefaultValue(true)] public bool Shop = true;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Nurse")]
    [DefaultValue(true)] public bool Nurse = true;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Reforging")]
    [DefaultValue(true)] public bool Reforging = true;
    [LabelKey($"${Localization.Keys.Infinities}.Currency.Others")]
    [DefaultValue(true)] public bool Others = true;
}

public sealed class Currency : Infinity<int, CurrencyCategory>, ITooltipLineDisplay {

    public override Group<int> Group => Currencies.Instance;
    public static Currency Instance = null!;
    public static CurrencyRequirements Config = null!;

    public override int IconType => ItemID.LuckyCoin;
    public override bool Enabled { get; set; } = false;
    public override Color Color { get; set; } = Colors.CoinGold;

    public override Requirement GetRequirement(CurrencyCategory category) => category switch {
        CurrencyCategory.Coins => new(10000, Config.Coins),
        CurrencyCategory.SingleCoin => new(20, Config.SingleCoin),
        _ => Requirement.None
    };

    public override CurrencyCategory GetCategory(int currency) {
        if (currency == SpikysLib.Currencies.Coins) return CurrencyCategory.Coins;
        if (!SpikysLib.Currencies.CustomCurrencies.Contains(currency)) return CurrencyCategory.None;
        return SpikysLib.Currencies.CurrencySystems(currency).Values.Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coins;
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => displayed == CustomCurrencyID.DefenderMedals ? ((TooltipLine, TooltipLineID?))(new(Mod, "Tooltip0", Language.GetTextValue("ItemTooltip.DefenderMedal")), TooltipLineID.Tooltip) : Tooltip.DefaultTooltipLine(this);

    public override void ModifyDisplay(Player player, Item item, int consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (50 <= index && index < 54) visibility = InfinityVisibility.Exclusive;
    }
}
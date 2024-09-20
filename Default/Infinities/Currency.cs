using SpikysLib;
using Terraria;
using Terraria.ModLoader.Config;
using Terraria.ID;
using SPIC.Default.Displays;
using Terraria.ModLoader;
using Terraria.Localization;
using SPIC.Configs;
using SPIC.Default.Presets;

namespace SPIC.Default.Infinities;

public sealed class Currency : ConsumableInfinity<int>, ICountToString, ITooltipLineDisplay {
    public static Currency Instance = null!;

    public sealed override InfinityDefaults Defaults => new() {
        Enabled = false,
        Color = Colors.CoinGold
    };
    public sealed override ConsumableDefaults ConsumableDefaults => new() {
        Preset = CurrencyDefaults.Instance,
        DisplayedInfinities = DisplayedInfinities.Consumable
    };

    public override int GetId(int consumable) => consumable;
    public override int ToConsumable(int id) => id;
    public override int ToConsumable(Item item) => item.CurrencyType();
    public override ItemDefinition ToDefinition(int consumable) => new(CurrencyHelper.LowestValueType(consumable));

    public override long CountConsumables(Player player, int consumable) => player.CountCurrency(consumable, true, true);

    public string CountToString(int consumable, long value, long outOf) {
        if (outOf == 0) return CurrencyHelper.PriceText(consumable, value);
        if (InfinityManager.GetCategory(consumable, Shop.Instance) == CurrencyCategory.SingleCoin) return $"{value}/{CurrencyHelper.PriceText(consumable, outOf)}";
        return $"{CurrencyHelper.PriceText(consumable, value)}/{CurrencyHelper.PriceText(consumable, outOf)}";
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => displayed == CustomCurrencyID.DefenderMedals ?
        ((TooltipLine, TooltipLineID?))(new(Instance.Mod, "Tooltip0", Language.GetTextValue("ItemTooltip.DefenderMedal")), TooltipLineID.Tooltip) :
        Displays.Tooltip.DefaultTooltipLine(Instance);
}
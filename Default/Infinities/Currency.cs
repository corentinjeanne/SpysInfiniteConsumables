using SpikysLib;
using Terraria;
using Terraria.ModLoader.Config;
using Terraria.ID;
using SPIC.Default.Displays;
using Terraria.ModLoader;
using Terraria.Localization;
using SPIC.Configs;
using SPIC.Default.Presets;
using System.Collections.Generic;
using SpikysLib.Configs;
using Microsoft.Xna.Framework;
using SpikysLib.Collections;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

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

    internal static void PortConfig(Dictionary<InfinityDefinition, Toggle<Dictionary<ProviderDefinition, object>>> infinities, ConsumableInfinities config) {
        InfinityDefinition currency = new("SPIC/Currency");
        bool enabled = config.infinities[currency].Key;
        LegacyCurrencyRequirements requirements = JObject.FromObject(config.infinities[currency].Value).ToObject<LegacyCurrencyRequirements>()!;
        RequirementRequirements coinsRequirements = new() { multiplier = requirements.Coins };
        CurrencyRequirements currencyRequirements = new() { coinsMultiplier = requirements.Coins, singleMultiplier = requirements.SingleCoin };
        config.infinities = new() {
            { new("SPIC/Shop"), new(requirements.Shop, new(){ { ProviderDefinition.Config, currencyRequirements } }) },
            { new("SPIC/Reforging"), new(requirements.Reforging, new(){ { ProviderDefinition.Config, coinsRequirements } }) },
            { new("SPIC/Nurse"), new(requirements.Nurse, new(){ { ProviderDefinition.Config, coinsRequirements } }) },
            { new("SPIC/Purchase"), new(requirements.Others, new(){ { ProviderDefinition.Config, currencyRequirements } }) },
        };
        infinities.GetOrAdd(currency, () => new(enabled)).Value[ProviderDefinition.Infinities] = config;
    }
    
    protected override void ModifyDisplayedInfinity(Item item, int consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (50 <= index && index < 54) visibility = InfinityVisibility.Exclusive;
    }

    internal static void PortClientConfig(Dictionary<InfinityDefinition, NestedValue<Color, Dictionary<ProviderDefinition, object>>> infinities, GroupColors colors) {
        InfinityDefinition currency = new("SPIC/Currency");
        infinities.GetOrAdd(currency, () => new()).Key = colors.Colors[currency];
    }
}

public sealed class LegacyCurrencyRequirements {
    [DefaultValue(1 / 20f)] public float Coins = 1 / 20f;
    [DefaultValue(1 / 5f)] public float SingleCoin = 1 / 5f;
    [DefaultValue(true)] public bool Shop = true;
    [DefaultValue(true)] public bool Nurse = true;
    [DefaultValue(true)] public bool Reforging = true;
    [DefaultValue(true)] public bool Others = true;
}

// using Terraria.ID;
// using System.ComponentModel;
// using SpikysLib;
// using Terraria.GameContent.UI;
// using SpikysLib.Configs;
// using SPIC.Configs;
// using Microsoft.Xna.Framework;
// using Microsoft.CodeAnalysis;
// using Terraria.ModLoader;
// using Terraria;
// using Terraria.Localization;
// using SPIC.Default.Displays;
// using SpikysLib.Constants;

// namespace SPIC.Default.Infinities;

// public enum CurrencyCategory {
//     None,
//     Coins,
//     SingleCoin,
// }

// // TODO turn into infinities
// public sealed class CurrencyRequirements {
//     [DefaultValue(1/20f)] public float CoinsMultiplier = 1/20f;
//     [DefaultValue(1/5f)] public float SingleCoinMultiplier = 1/5f;
//     [DefaultValue(true)] public bool Shop = true; // TODO PreventItemDuplication
//     [DefaultValue(true)] public bool Nurse = true;
//     [DefaultValue(true)] public bool Reforging = true;
//     [DefaultValue(true)] public bool Others = true; // TODO PreventItemDuplication

//     // Compatibility version < v3.2
//     [DefaultValue(1/20f)] private float Coins { set => ConfigHelper.MoveMember(value != 1 / 20f, c => CoinsMultiplier = value); }
//     [DefaultValue(1/5)] private float SingleCoin { set => ConfigHelper.MoveMember(value != 1 / 5, c => SingleCoinMultiplier = value); }
// }

// public sealed class Currency : Infinity<int>, IConfigurableComponents<CurrencyRequirements> {
//     public static Customs<int, CurrencyCategory> Customs = new(i => new(CurrencyHelper.LowestValueType(i)));
//     public static Category<int, CurrencyCategory> Category = new(GetRequirement, GetCategory);
//     public static Currency Instance = null!;
//     public static TooltipDisplay TooltipDisplay = new(GetTooltipLine, CountToString);

//     public override bool DefaultEnabled => false;
//     public override Color DefaultColor => Colors.CoinGold;

//     protected override Optional<int> GetId(int consumable) => consumable;
//     protected override Optional<int> ToConsumable(int id) => id;
//     protected override Optional<int> ToConsumable(Item item) => item.CurrencyType();

//     private static Optional<Requirement> GetRequirement(CurrencyCategory category) => category switch {
//         CurrencyCategory.Coins => new(10000, InfinitySettings.Get(Instance).CoinsMultiplier),
//         CurrencyCategory.SingleCoin => new(20, InfinitySettings.Get(Instance).SingleCoinMultiplier),
//         _ => Requirement.None
//     };

//     private static Optional<CurrencyCategory> GetCategory(int currency) {
//         if (currency == CurrencyHelper.Coins) return CurrencyCategory.Coins;
//         if (!CustomCurrencyManager.TryGetCurrencySystem(currency, out var system)) return CurrencyCategory.None;
//         return system.ValuePerUnit().Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coins;
//     }

//     public static string CountToString(int consumable, long count, long value) {
//         if (count == 0) return CurrencyHelper.PriceText(consumable, value);
//         if (InfinityManager.GetCategory(consumable, Category) == CurrencyCategory.SingleCoin) return $"{count}/{CurrencyHelper.PriceText(consumable, value)}";
//         return $"{CurrencyHelper.PriceText(consumable, count)}/{CurrencyHelper.PriceText(consumable, value)}";
//     }

//     public static (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => displayed == CustomCurrencyID.DefenderMedals ?
//         ((TooltipLine, TooltipLineID?))(new(Instance.Mod, "Tooltip0", Language.GetTextValue("ItemTooltip.DefenderMedal")), TooltipLineID.Tooltip) :
//         Displays.Tooltip.DefaultTooltipLine(Instance);

//     protected override Optional<InfinityVisibility> GetVisibility(Item item) {
//         int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
//         return InventorySlots.Coins.Contains(index) ? new(InfinityVisibility.Exclusive) : default;
//     }
// }
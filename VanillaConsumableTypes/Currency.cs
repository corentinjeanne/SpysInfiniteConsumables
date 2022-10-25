using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;
namespace SPIC.VanillaConsumableTypes;

public enum CurrencyCategory : byte {
    None = Category.None,
    Coin,
    SingleCoin,
}

public class CurrencyRequirements {
    [Label("$Mods.SPIC.Types.Currency.coins")]
    public ItemCountWrapper Coins = new(10, 100);
    [Label("$Mods.SPIC.Types.Currency.custom")]
    public ItemCountWrapper Single = new(50);
}

// TODO >>> fix display
public class Currency : ConsumableType<Currency>, IStandardConsumableType<CurrencyCategory, CurrencyRequirements> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.LuckyCoin;

    public bool DefaultsToOn => false;
    public CurrencyRequirements Settings { get; set; }
    
    public IRequirement Requirement(CurrencyCategory category) => category switch {
        CurrencyCategory.Coin => new PowerRequirement(((ItemCount)Settings.Coins) * 100, 100, 0.1f),
        CurrencyCategory.SingleCoin => new MultipleRequirement(Settings.Single, 0.2f),
        CurrencyCategory.None or _ => null     
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

    public long GetMaxInfinity(Player player, Item item) {
        int currency = item.CurrencyType();
        long value = item.CurrencyValue();
        long cost;
        if (Main.InReforgeMenu) cost = Main.reforgeItem.value;
        else if (Main.npcShop != 0) cost = Globals.SpicNPC.HighestPrice(currency);
        else cost = Globals.SpicNPC.HighestItemValue(currency);
        return value == 0 ? Infinity.None.EffectiveRequirement.Items : cost / value + (cost % value == 0 ? 0 : 1);
    }

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.CoinGold;

    public TooltipLine TooltipLine => TooltipHelper.AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.ItemTooltip.curency"));
    public string LinePosition => "Consumable";

    // void IConsumableType.ModifyTooltip(Item item, List<TooltipLine> tooltips) {
    //     Player player = Main.LocalPlayer;
    //     Category category = item.GetCategory(UID);
    //     IRequirement requirement = item.GetRequirement(UID);

    //     Infinity infinity;
    //     ItemCount itemCount;

    //     if (((IDefaultDisplay)this).OwnsItem(player, item, true)) {
    //         infinity = InfinityManager.GetInfinity(player, item, UID);
    //         itemCount = new(CountItems(player, item), item.maxStack);
    //     } else {
    //         infinity = Infinity.None;
    //         itemCount = ItemCount.None;
    //     }

    //     ItemCount next = infinity.Value.IsNone || infinity.Value.Items < GetMaxInfinity(player, item) ?
    //         requirement.NextRequirement(infinity.EffectiveRequirement) :
    //         null;

    //     DisplayFlags displayFlags = DefaultImplementation.GetDisplayFlags(category, infinity, next) & ((IDefaultDisplay)this).DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
    //     if ((displayFlags & DefaultImplementation.OnLineDisplayFlags) == 0) return;

    //     TooltipLine line = tooltips.FindorAddLine(TooltipLine, LinePosition, out bool addedLine);
    //     DefaultImplementation.DisplayOnLine(ref line.Text, ref line.OverrideColor, ((IDefaultDisplay)this).Color, displayFlags, category, infinity, next, itemCount);
    //     if (addedLine) line.OverrideColor *= 0.75f;
    // }
}
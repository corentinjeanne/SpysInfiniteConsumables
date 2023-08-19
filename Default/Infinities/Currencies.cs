using System.Collections.Generic;
using SPIC.Default.Displays;
using Terraria;

namespace SPIC.Default.Infinities;

public sealed class Currencies : Group<Currencies, int> {
    public override int ToConsumable(Item item) => item.CurrencyType();
    public override Item ToItem(int consumable) => new(CurrencyHelper.LowestValueType(consumable));
    public override int GetType(int consumable) => consumable;
    public override int FromType(int type) => type;
    
    public override long CountConsumables(Player player, int consumable) => player.CountCurrency(consumable, true, true);
    
    public override string CountToString(int consumable, long count, CountStyle style, bool rawValue = false) {
        if(rawValue && InfinityManager.GetCategory(consumable, Currency.Instance) == CurrencyCategory.SingleCoin) return count.ToString();
        switch (style) {
        case CountStyle.Sprite:
            List<KeyValuePair<int, long>> items = CurrencyHelper.CurrencyCountToItems(consumable, count);
            List<string> parts = new();
            foreach ((int t, long c) in items) parts.Add($"{c}[i:{t}]");
            return string.Join(' ', parts);
        default:
            return CurrencyHelper.PriceText(consumable, count);
        }
    }
}
using SPIC.Default.Displays;
using SpikysLib;
using Terraria;

namespace SPIC.Default.Infinities;

public sealed class Currencies : Group<int>, ICountToString {

    public static Currencies Instance = null!;

    public override int ToConsumable(Item item) => item.CurrencyType();
    public override Item ToItem(int consumable) => new(CurrencyHelper.LowestValueType(consumable));
    public override int GetType(int consumable) => consumable;
    public override int FromType(int type) => type;
    
    public override long CountConsumables(Player player, int consumable) => player.CountCurrency(consumable, true, true);
    
    public string CountToString(int consumable, long count, long value) {
        if(count == 0) return CurrencyHelper.PriceText(consumable, value);
        if(InfinityManager.GetCategory(consumable, Currency.Instance) == CurrencyCategory.SingleCoin) return $"{count}/{CurrencyHelper.PriceText(consumable, value)}";
        return $"{CurrencyHelper.PriceText(consumable, count)}/{CurrencyHelper.PriceText(consumable, value)}";
    }
}
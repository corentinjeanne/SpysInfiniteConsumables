using SpikysLib.Reflection;
using TItem = Terraria.Item;
using TItemLoader = Terraria.ModLoader.ItemLoader;
using TShoppingSettings = Terraria.ShoppingSettings;

namespace SPIC.Reflection;

public class Item {
    public static readonly Field<TItem, int> stack = new(nameof(TItem.stack));
}
public class ItemLoader {
    public static readonly StaticMethod<bool> ConsumeItem = new(typeof(TItemLoader), nameof(TItemLoader.ConsumeItem), typeof(TItem), typeof(Terraria.Player));
}

public class ShoppingSettings {
    public static readonly Field<TShoppingSettings, double> PriceAdjustment = new(nameof(TShoppingSettings.PriceAdjustment));
}
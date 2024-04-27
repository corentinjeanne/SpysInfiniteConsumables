using SpikysLib.Reflection;
using TItem = Terraria.Item;
using TItemLoader = Terraria.ModLoader.ItemLoader;

namespace SPIC.Reflection;

public class Item {
    public static readonly Field<TItem, int> stack = new(nameof(TItem.stack));
}
public class ItemLoader {
    public static readonly StaticMethod<bool> ConsumeItem = new(typeof(TItemLoader), nameof(TItemLoader.ConsumeItem), typeof(TItem), typeof(Terraria.Player));
}
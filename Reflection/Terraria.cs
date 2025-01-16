using SpikysLib.Reflection;
using TItem = Terraria.Item;
using TItemLoader = Terraria.ModLoader.ItemLoader;
using TShoppingSettings = Terraria.ShoppingSettings;
using TProjectileID = Terraria.ID.ProjectileID;
using TMain = Terraria.Main;

namespace SPIC.Reflection;

public static class Main {
    public static readonly StaticField<Terraria.Tilemap> tile = new(typeof(TMain), nameof(TMain.tile));
}
public static class Item {
    public static readonly Field<TItem, int> stack = new(nameof(TItem.stack));
}
public static class ItemLoader {
    public static readonly StaticMethod<bool> ConsumeItem = new(typeof(TItemLoader), nameof(TItemLoader.ConsumeItem), typeof(TItem), typeof(Terraria.Player));
}

public static class ShoppingSettings {
    public static readonly Field<TShoppingSettings, double> PriceAdjustment = new(nameof(TShoppingSettings.PriceAdjustment));
}

public static class ProjectileID {
    public static class Sets {
        public static readonly StaticField<TProjectileID.Sets.FallingBlockTileItemInfo[]> FallingBlockTileItem = new(typeof(TProjectileID.Sets), nameof(TProjectileID.Sets.FallingBlockTileItem));
    }
}
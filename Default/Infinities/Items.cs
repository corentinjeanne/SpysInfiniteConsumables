using SPIC.Default.Displays;
using Terraria;
using Terraria.Localization;

namespace SPIC.Default.Infinities;

public sealed class Items : Group<Items, Item> {
    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);

    public override Item ToConsumable(Item item) => item;
    public override Item ToItem(Item consumable) => consumable;
    public override int GetType(Item consumable) => consumable.type;
    public override Item FromType(int type) => new(type);

    public override string CountToString(int consumable, long count, CountStyle style, bool rawValue = false) {
        if (rawValue) return count.ToString();
        return style switch {
            CountStyle.Sprite => $"{count}[i:{consumable}]",
            _ or CountStyle.Name => Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", count),
        };
    }


}
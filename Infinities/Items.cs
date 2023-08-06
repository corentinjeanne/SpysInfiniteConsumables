using SPIC.Displays;
using Terraria;
using Terraria.Localization;

// ? Add transformations Infinity

namespace SPIC.Infinities;

public sealed class Items : Group<Items, Item> {
    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);
    public override long MaxStack(Item consumable) => consumable.IsACoin ? 100 : consumable.maxStack;

    public override Item ToConsumable(Item item) => item;
    public override Item ToItem(Item consumable) => consumable;
    public override int GetType(Item consumable) => consumable.type;

    public override string CountToString(Item consumable, long count, CountStyle style, bool rawValue = false) {
        if (rawValue) return count.ToString();
        return style switch {
            CountStyle.Sprite => $"{count}[i:{consumable.type}]",
            _ or CountStyle.Name => Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", count),
        };
    }

    public override Item FromType(int type) => new(type);

}
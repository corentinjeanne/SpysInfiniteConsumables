using Terraria;
using Terraria.Localization;
using SpikysLib.Extensions;
namespace SPIC.Default.Infinities;

public sealed class Items : Group<Items, Item> {
    public override void SetStaticDefaults() {
        Displays.Tooltip.Instance.RegisterCountStr(this, CountToString);
    }

    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);

    public override Item ToConsumable(Item item) => item;
    public override Item ToItem(Item consumable) => consumable;
    public override int GetType(Item consumable) => consumable.type;
    public override Item FromType(int type) => new(type);

    public static string CountToString(int consumable, long count, long value) => count == 0 ?
        Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", value) :
        $"{count}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", value)}";
}
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Components;
using SPIC.Default.Displays;
using Terraria;
using Terraria.Localization;

namespace SPIC.Default.Infinities;

public class ConsumableItem : Infinity<Item> {
    public static Customs<Item> Customs = new(i => new(i.type));
    public static InfinityGroup<Item> InfinityGroup = new();
    public static ConsumableItem Instance = null!;
    public static TooltipDisplay TooltipDisplay = new(null, CountToString);


    public override Color DefaultColor => new(64, 64, 64);

    protected override Optional<long> CountConsumables(PlayerConsumable<Item> args) => args.Player.CountItem(args.Consumable.type);
    protected override Optional<int> GetId(Item consumable) => consumable.type;
    protected override Optional<Item> ToConsumable(int id) => new Item(id);
    protected override Optional<Item> ToConsumable(Item item) => item;

    // TODO check with Usable
    public static string CountToString(int consumable, long count, long value) => count == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", value) : $"{count}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", value)}";
}
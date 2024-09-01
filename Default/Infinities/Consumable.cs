using Terraria;
using Terraria.ID;

namespace SPIC.Default.Infinities;

public class Consumable : GroupInfinity<Item> {
    public static Consumable Instance = null!;

    protected override long CountConsumables(PlayerConsumable<Item> args) => args.Player.CountItem(args.Consumable.type);
    public override int GetId(Item consumable) => consumable.type;
    public override Item ToConsumable(int id) => new(id);
}
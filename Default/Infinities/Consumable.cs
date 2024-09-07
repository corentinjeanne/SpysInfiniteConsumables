using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Default.Components;
using Terraria;

namespace SPIC.Default.Infinities;

public class Consumable : GroupInfinity<Item> {
    public static Custom<Item> Custom = new(i => new(i.type));
    public static Consumable Instance = null!;


    public override Color DefaultColor => new(64, 64, 64);

    protected override Optional<long> CountConsumables(PlayerConsumable<Item> args) => args.Player.CountItem(args.Consumable.type);
    public override int GetId(Item consumable) => consumable.type;
    public override Item ToConsumable(int id) => new Item(id);
}
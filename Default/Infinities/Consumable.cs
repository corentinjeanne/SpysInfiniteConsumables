using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Default.Components;
using Terraria;

namespace SPIC.Default.Infinities;

public class Consumable : Infinity<Item> {
    public static Customs<Item> Customs = new(i => new(i.type));
    public static InfinityGroup<Item> InfinityGroup = new();
    public static Consumable Instance = null!;


    public override Color DefaultColor => new(64, 64, 64);

    protected override Optional<long> CountConsumables(PlayerConsumable<Item> args) => args.Player.CountItem(args.Consumable.type);
    protected override Optional<int> GetId(Item consumable) => consumable.type;
    protected override Optional<Item> ToConsumable(int id) => new Item(id);

    public static implicit operator InfinityGroup<Item>(Consumable consumable) => InfinityGroup;
}
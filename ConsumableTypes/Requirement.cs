using Terraria;

namespace SPIC.ConsumableTypes;

public interface IRequirement {
    ItemCount NextRequirement(ItemCount count);
    Infinity Infinity(Item item, ItemCount itemCount);
}

public sealed class NoRequirement : IRequirement {
    public Infinity Infinity(Item item, ItemCount itemCount) => new(ItemCount.None, 0);
    public ItemCount NextRequirement(ItemCount count) => ItemCount.None;
}

public abstract class RecursiveRequirement : IRequirement {
    protected RecursiveRequirement(ItemCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }
    public ItemCount Root { get; init; }

    public ItemCount EffectiveRequirement(ItemCount itemCount){
        ItemCount effective = ItemCount.None;
        ItemCount next = Root;
        while(itemCount >= next){
            effective = next;
            next = NextValue(next);
        }
        return effective;
    }
    public Infinity Infinity(Item item, ItemCount itemCount) => new(EffectiveRequirement(itemCount), Multiplier);
    protected abstract ItemCount NextValue(ItemCount value);
    public ItemCount NextRequirement(ItemCount count) => count.IsNone ? Root : NextValue(count);
}

public abstract class FixedRequirement : IRequirement {
    public FixedRequirement(ItemCount root, float multiplier) {
        Multiplier = multiplier;
        Root = root;
    }

    public float Multiplier { get; init; }
    public ItemCount Root { get; init; }

    public Infinity Infinity(Item item, ItemCount itemCount) => new(EffectiveRequirement(item, itemCount), Multiplier);
    public abstract ItemCount EffectiveRequirement(Item item, ItemCount itemCount);
    public ItemCount NextRequirement(ItemCount value) => value < Root ? Root : ItemCount.None;
}

public sealed class ItemCountRequirement : FixedRequirement {
    public ItemCountRequirement(ItemCount root, float multiplier = 1) : base(root, multiplier) { }

    public override ItemCount EffectiveRequirement(Item item, ItemCount itemCount) => itemCount < Root ? ItemCount.None : Root.UseItems ? new(itemCount.Items, Root.MaxStack) : new(itemCount.CapMaxStack(Root.MaxStack).Stacks, Root.MaxStack);
}

public sealed class DisableAboveRequirement : FixedRequirement {
    public DisableAboveRequirement(ItemCount root, float multiplier = 1) : base(root, multiplier) { }

    public override ItemCount EffectiveRequirement(Item item, ItemCount itemCount) => itemCount == Root ? Root : ItemCount.None;
}

public sealed class PowerRequirement : RecursiveRequirement {
    public PowerRequirement(ItemCount root, int power, float multiplier = 1) : base(root, multiplier) {
        Power = power;
    }
    
    public int Power { get; init; }

    protected override ItemCount NextValue(ItemCount value) => value * Power;
}

public sealed class MultipleRequirement : RecursiveRequirement {
    public MultipleRequirement(ItemCount root, float multiplier = 1) : base(root, multiplier) { }

    protected override ItemCount NextValue(ItemCount value) => value + Root;
}
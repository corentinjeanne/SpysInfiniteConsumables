using System;
using Terraria;

namespace SPIC;
public interface IInfinityBridge {
    bool Enabled { get; }
    IConsumableBridge Consumable { get; }

    long GetRequirement(int consumable);
    long GetInfinity(Player player, int consumable);
}

public interface IConsumableBridge : IInfinityBridge {
    long CountConsumables(Player player, int consumable);
}

public interface IInfinityBridge<TCategory> : IInfinityBridge where TCategory: struct, Enum {
    TCategory GetCategory(int consumable);
}
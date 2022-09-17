using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableTypes;

public enum MixedCategory{
    AllNone = ConsumableType.NoCategory,
    Mixed
}

internal class Mixed : ConsumableType<Mixed>, IPartialInfinity {

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    
    public override string LocalizedCategoryName(byte category) => "";
    public override object CreateRequirements() => null;
    public override Microsoft.Xna.Framework.Color DefaultColor() => default;
    public override TooltipLine TooltipLine => null;
    public override InfinityDisplayFlag GetInfinityDisplayLevel(Item item, bool isACopy) {
        InfinityDisplayFlag flags = InfinityDisplayFlag.All & ~InfinityDisplayFlag.Category;
        foreach (int usedID in item.UsedConsumableTypes()) {
            ConsumableType type = InfinityManager.ConsumableType(usedID);
            flags &= type.GetInfinityDisplayLevel(item, isACopy);
        }
        return flags;
    }

    public override int MaxStack(byte category) => 999;
    public override int Requirement(byte category) => NoRequirement;

    public override byte GetCategory(Item item) {
        byte mixed = NoCategory;
        foreach (int usedID in item.UsedConsumableTypes()) {
            byte cat = item.GetCategory(usedID);
            if (mixed == NoCategory || cat > mixed) mixed = cat;
        }
        return mixed;
    }

    public override int GetRequirement(Item item) {
        int mixed = NoRequirement;
        foreach (int usedID in item.UsedConsumableTypes()) {
            int req = item.GetRequirement(usedID);
            if (mixed == NoInfinity || req > mixed) mixed = req;
        }
        return mixed;
    }

    public override long GetInfinity(Player player, Item item) {
        long mixed = NoInfinity;
        foreach (int usedID in item.UsedConsumableTypes()) {
            long inf = player.GetInfinity(item, usedID);
            if (mixed == NoInfinity || inf < mixed) mixed = inf;
        }
        return mixed;
    }

    public long GetFullInfinity(Player player, Item item) {
        long mixed = NoInfinity;
        foreach (int usedID in item.UsedConsumableTypes()) {
            long inf = player.GetFullInfinity(item, usedID);
            if (mixed == NoInfinity || inf > mixed) mixed = inf;
        }
        return mixed;
    }
}
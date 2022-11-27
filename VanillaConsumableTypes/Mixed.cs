using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

using SPIC.ConsumableGroup;
namespace SPIC.VanillaConsumableTypes;
public enum MixedCategory : byte {
    AllNone = Category.None,
    Mixed
}

public class MixedRequirement : IRequirement {

    public Item Item { get; init; }

    public MixedRequirement(Item item) {
        Item = item;
    }


    public Infinity Infinity(ICount count) {
        Infinity min = new(count.None, 0);
        foreach(IStandardGroup<Item> type in InfinityManager.ConsumableGroups<IStandardGroup<Item>>()){
            if(type.UID == Mixed.ID) continue;
            IRequirement requirement = Item.GetRequirement(type.UID);
            if(requirement is NoRequirement) continue;
            Infinity inf = requirement.Infinity(count);
            if(min.Value.IsNone || inf.Value.CompareTo(min.Value) > 0) min = inf;
        }
        return min;
    }

    public ICount NextRequirement(ICount count){
        ICount min = count.None;
        foreach(IConsumableGroup type in InfinityManager.ConsumableGroups()){
            if(type.UID == Mixed.ID) continue;
            IRequirement requirement = Item.GetRequirement(type.UID);
            if(requirement is NoRequirement) continue;
            ICount next = requirement.NextRequirement(requirement.Infinity(count).EffectiveRequirement);
            if(min.IsNone || next.CompareTo(min) > 0) min = next;
        }
        return min;
    }
}

// ? amunition
internal class Mixed : ConsumableGroup<Mixed, Item> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => Terraria.ID.ItemID.LunarHook;


    public override Item ToConsumable(Item item) => item;
    public override int CacheID(Item consumable) => consumable.type;

    public override ICount LongToCount(Item consumable, long count) => new ItemCount(consumable, count);

    public static Color InfinityColor => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
    public static Color PartialInfinityColor => new(255, (byte)(Main.masterColor * 200f), 0);

    // public MixedCategory GetCategory(Item item) {
    //     foreach (IConsumableGroup type in item.UsedConsumableTypes()) {
    //         if (!item.GetCategory(type.UID).IsNone) return MixedCategory.Mixed;
    //     }
    //     return MixedCategory.AllNone;
    // }

    public override IRequirement GetRequirement(Item item) {
        return new MixedRequirement(item);
    }

    public override long CountConsumables(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IConsumableGroup type in item.UsedConsumableTypes()) {
            long c = type.CountConsumables(player, item);
            if (c < count) count = c;
        }
        return count;
    }

    public static long GetMaxInfinity(Player player, Item item) {
        long mixed = 0;
        foreach (IStandardGroup type in item.UsedConsumableTypes()) {
            long inf = type.GetMaxInfinity(player, item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public static bool OwnsItem(Player player, Item item, bool isACopy){
        foreach (IStandardGroup type in item.UsedConsumableTypes()){
            if(!type.OwnsItem(player, item, isACopy)) return false;
        }
        return true;
    }

    public override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {}
    public override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}

}
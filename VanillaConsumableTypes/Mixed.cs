using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

using SPIC.ConsumableGroup;

namespace SPIC.VanillaConsumableTypes;

public class MixedRequirement : IRequirement {

    public Item Item { get; init; }

    public MixedRequirement(Item item) {
        Item = item;
    }


    public Infinity Infinity(ICount count) {
        Infinity max = new(count.None, 0);
        foreach(IStandardGroup<Item> group in InfinityManager.UsedConsumableGroups(Item)){
            IRequirement requirement = Item.GetRequirement(group.UID);
            if(requirement is NoRequirement) continue;
            Infinity inf = requirement.Infinity(count);
            if(max.Value.IsNone || inf.Value.CompareTo(max.Value) > 0) max = inf;
        }
        return max;
    }

    public ICount NextRequirement(ICount count){
        ICount max = count.None;
        foreach(IStandardGroup<Item> group in InfinityManager.UsedConsumableGroups(Item)){
            IRequirement requirement = Item.GetRequirement(group.UID);
            if(requirement is NoRequirement) continue;
            ICount next = requirement.NextRequirement(requirement.Infinity(count).EffectiveRequirement);
            if(max.IsNone || next.CompareTo(max) > 0) max = next;
        }
        return max;
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

    public override IRequirement GetRequirement(Item item) => new MixedRequirement(item);

    public override long CountConsumables(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IStandardGroup<Item> group in item.UsedConsumableGroups()) {
            long c = group.CountConsumables(player, item);
            if (c < count) count = c;
        }
        return count;
    }

    public static long GetMaxInfinity(Player player, Item item) {
        long mixed = 0;
        foreach (IStandardGroup type in item.UsedConsumableGroups()) {
            long inf = type.GetMaxInfinity(player, item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public static bool OwnsItem(Player player, Item item, bool isACopy){
        foreach (IStandardGroup type in item.UsedConsumableGroups()){
            if(!type.OwnsItem(player, item, isACopy)) return false;
        }
        return true;
    }

    public override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {}
    public override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}

}
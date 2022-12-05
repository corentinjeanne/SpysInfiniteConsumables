using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

using SPIC.ConsumableGroup;

namespace SPIC.VanillaGroups;

public class MixedRequirement : Requirement<ItemCount> {

    public override bool IsNone => false;

    public override Infinity<ItemCount> Infinity(ItemCount count) {
        Item item = new(count.Type);
        Infinity<ItemCount> max = new(count.None, 0);
        foreach(IStandardGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item)){
            Requirement<ItemCount> requirement = item.GetRequirement(group);
            if(requirement.IsNone) continue;
            Infinity<ItemCount> inf = requirement.Infinity(count);
            if(max.Value.IsNone || inf.Value.CompareTo(max.Value) > 0) max = inf;
        }
        return max;
    }

    public override ItemCount NextRequirement(ItemCount count){
        Item item = new(count.Type);
        ItemCount max = count.None;
        foreach(IStandardGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item)){
            Requirement<ItemCount> requirement = item.GetRequirement(group);
            if(requirement.IsNone) continue;
            ItemCount next = requirement.NextRequirement(requirement.Infinity(count).EffectiveRequirement);
            if(max.IsNone || next.CompareTo(max) > 0) max = next;
        }
        return max;
    }
}

// ? amunition
internal class Mixed : ConsumableGroup<Mixed, Item, ItemCount> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => Terraria.ID.ItemID.LunarHook;


    public override Item ToConsumable(Item item) => item;
    public override int ReqCacheID(Item consumable) => 0;
    public override int CacheID(Item consumable) => consumable.type;

    public override ItemCount LongToCount(Item consumable, long count) => new(consumable){Items=count};

    public static Color InfinityColor => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
    public static Color PartialInfinityColor => new(255, (byte)(Main.masterColor * 200f), 0);

    public override Requirement<ItemCount> GetRequirement(Item item) => new MixedRequirement();

    public override long CountConsumables(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IStandardGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item)) {
            long c = group.CountConsumables(player, item);
            if (c < count) count = c;
        }
        return count;
    }

    public static long GetMaxInfinity(Player player, Item item) {
        long mixed = 0;
        foreach (IStandardGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item)) {
            long inf = group.GetMaxInfinity(player, item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public static bool OwnsItem(Player player, Item item, bool isACopy){
        foreach (IStandardGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item)){
            if(!group.OwnsItem(player, item, isACopy)) return false;
        }
        return true;
    }


    public override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {}
    public override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}

    public override string Key(Item consumable) => new Item(consumable.type).ToString();
}
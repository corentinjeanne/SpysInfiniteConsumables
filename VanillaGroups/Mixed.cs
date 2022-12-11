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
        Infinity<ItemCount> min = new(count.None, 0);
        foreach(IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)){
            Requirement<ItemCount> requirement = item.GetRequirement(group);
            if(requirement.IsNone) continue;
            Infinity<ItemCount> inf = requirement.Infinity(count);
            if(min.Value.IsNone || inf.Value.CompareTo(min.Value) < 0) min = inf;
        }
        return min;
    }

    public override ItemCount NextRequirement(ItemCount count){
        Item item = new(count.Type);
        ItemCount min = count.None;
        foreach(IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)){
            Requirement<ItemCount> requirement = item.GetRequirement(group);
            if(requirement.IsNone) continue;
            ItemCount next = requirement.NextRequirement(requirement.Infinity(count).EffectiveRequirement);
            if(!next.IsNone && (min.IsNone || next.CompareTo(min) < 0)) min = next;
        }
        return min;
    }
}


// TODO ammo
public class Mixed : ConsumableGroup<Mixed, Item, ItemCount> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => Terraria.ID.ItemID.LunarHook;

    public override Item ToConsumable(Item item) => item;
    public override int ReqCacheID(Item consumable) => 0;
    public override int CacheID(Item consumable) => consumable.type;
    public override string Key(Item consumable) => new Item(consumable.type).ToString();

    public override ItemCount LongToCount(Item consumable, long count) => new(consumable){Items=count};

    public static Color InfinityColor => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
    public static Color PartialInfinityColor => new(255, (byte)(Main.masterColor * 200f), 0);

    public override Requirement<ItemCount> GetRequirement(Item item) => new MixedRequirement();

    public override long CountConsumables(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)) {
            long c = group.CountConsumables(player, item);
            if (c < count) count = c;
        }
        return count;
    }

    public override long GetMaxInfinity(Item item) {
        long mixed = 0;
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)) {
            long inf = group.GetMaxInfinity(item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public override bool OwnsItem(Player player, Item item, bool isACopy){
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)){
            if(!group.OwnsItem(player, item, isACopy)) return false;
        }
        return true;
    }


    public override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        if(!Config.InfinityDisplay.Instance.tooltip_ShowMixed || Config.RequirementSettings.Instance.MaxConsumableTypes < 2) return;
        InfinityManager.UsedConsumableGroups(item, out bool hidden);
        if(!hidden) return;

        Globals.DisplayInfo<ItemCount> info = this.GetDisplayInfo(item, true, out _);
        info.DisplayFlags &= Globals.DisplayFlags.Infinity;
        if ((info.DisplayFlags & Globals.InfinityDisplayItem.LineDisplayFlags) != 0) {
            TooltipLine line = tooltips.AddLine(TooltipHelper.AddedLine("MixedInfinity", ""), TooltipLineID.Modded);
            Color color = info.Next.IsNone ? new(Main.DiscoR, Main.DiscoG, Main.DiscoB) : new(255, (int)(Main.masterColor * 200f), 0);
            Globals.InfinityDisplayItem.DisplayOnLine(ref line.Text, ref line.OverrideColor, color, info);
            line.Text = line.Text.Replace("  ", " ");
            line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
        }
    }
    public override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}}
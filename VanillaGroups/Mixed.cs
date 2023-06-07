using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using SPIC.ConsumableGroup;
using Terraria.Localization;
using System.Collections.ObjectModel;

namespace SPIC.VanillaGroups;

public class MixedRequirement : Requirement<ItemCount> {

    public ItemCount? Custom { get; private set; }

    public Item Item { get; }

    public override bool IsNone => InfinityManager.UsedConsumableGroups(Item, out bool _).Count == 0;

    public MixedRequirement(Item item){
        Custom = null;
        Item = item;
    }

    public override Infinity<ItemCount> Infinity(ItemCount count) {
        if(Custom.HasValue) return new(Custom.Value.IsNone || Custom.Value.CompareTo(count) > 0 ? count.None : count, 1);
        Item item = new(count.Type);
        Infinity<ItemCount> min = new(count.None, 0);
        foreach(IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)){
            Infinity<ItemCount> inf = InfinityManager.GetInfinity(item, count, group);
            if(min.Value.IsNone || inf.Value.CompareTo(min.Value) < 0) min = inf;
        }
        return min;
    }

    public override ItemCount NextRequirement(ItemCount count){
        if(Custom.HasValue) return Custom.Value.IsNone || Custom.Value.CompareTo(count) > 0 ? Custom.Value.AdaptTo(count) : count.None;
        Item item = new(count.Type);
        ItemCount min = count.None;
        foreach(IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)){
            Requirement<ItemCount> requirement = item.GetRequirement(group);
            ItemCount next = requirement.NextRequirement(requirement.Infinity(count).EffectiveRequirement);
            if(!next.IsNone && (min.IsNone || next.CompareTo(min) < 0)) min = next;
        }
        return min;
    }

    public override void Customize(ItemCount custom) => Custom = custom;
}


public class Mixed : ConsumableGroup<Mixed, Item, ItemCount> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.Mixed.Name");
    public override int IconType => Terraria.ID.ItemID.LunarHook;

    public override Item ToConsumable(Item item) => item;
    public override int CacheID(Item consumable) => consumable.type;
    public override string Key(Item consumable) => new Item(consumable.type).ToString();

    public override ItemCount LongToCount(Item consumable, long count) => new(consumable) { Items = count };

    public static Color InfinityColor => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
    public static Color PartialInfinityColor => new(255, (byte)(Main.masterColor * 200f), 0);

    public override Requirement<ItemCount> GetRequirement(Item item) => new MixedRequirement(item);

    public override long CountConsumables(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)) {
            count = System.Math.Min(count, group.CountConsumables(player, item));
        }
        return count;
    }

    public override bool OwnsItem(Player player, Item item, bool isACopy) {
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.UsedConsumableGroups(item, out _)) {
            if (!group.OwnsItem(player, item, isACopy)) return false;
        }
        return true;
    }


    public override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        if (!Configs.InfinityDisplay.Instance.tooltip_ShowMixed || Configs.GroupSettings.Instance.MaxConsumableTypes <= 1) return;
        InfinityManager.UsedConsumableGroups(item, out bool hidden);
        if (!hidden) return;

        Globals.DisplayInfo<ItemCount> info = this.GetDisplayInfo(item, true, out _);

        info.DisplayFlags &= Globals.DisplayFlags.Infinity;

        if ((info.DisplayFlags & Globals.InfinityDisplayItem.LineDisplayFlags) == 0) return;
        TooltipLine line = tooltips.AddLine(new(Mod, InternalName, ""), TooltipLineID.Modded);
        Color color = info.Next.IsNone ? new(Main.DiscoR, Main.DiscoG, Main.DiscoB) : new(255, (int)(Main.masterColor * 200f), 0);
        Globals.InfinityDisplayItem.DisplayOnLine(line, color, info);
        line.Text = line.Text.Trim().Replace("  ", " ");
        line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
    }

    public override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
}
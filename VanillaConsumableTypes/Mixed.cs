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

    public Item Item;

    public MixedRequirement(Item item) {
        Item = item;
    }


    public Infinity Infinity(Item item, ItemCount itemCount) {
        Infinity min = ConsumableGroup.Infinity.None;
        foreach(IConsumableGroup type in InfinityManager.ConsumableGroups()){
            if(type.UID == Mixed.ID) continue;
            IRequirement requirement = item.GetRequirement(type.UID);
            if(requirement is NoRequirement) continue;
            Infinity inf = requirement.Infinity(item, itemCount);
            if(min.Value.IsNone || inf.Value > min.Value) min = inf;
        }
        return min;
    }

    public ItemCount NextRequirement(ItemCount count){
        ItemCount min = ItemCount.None;
        foreach(IConsumableGroup type in InfinityManager.ConsumableGroups()){
            if(type.UID == Mixed.ID) continue;
            IRequirement requirement = Item.GetRequirement(type.UID);
            if(requirement is NoRequirement) continue;
            ItemCount next = requirement.NextRequirement(requirement.Infinity(Item, count).EffectiveRequirement);
            if(min.IsNone || next > min) min = next;
        }
        return min;
    }
}

// ? amunition
internal class Mixed : ConsumableGroup<Mixed, Item> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => Terraria.ID.ItemID.LunarHook;

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

    static Globals.DisplayFlags DisplayFlags => Globals.DisplayFlags.Infinity | Globals.DisplayFlags.Requirement;

    public override long CountConsumables(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IConsumableGroup type in item.UsedConsumableTypes()) {
            long c = type.CountConsumables(player, item);
            if (c < count) count = c;
        }
        return count;
    }

    public static long GetMaxInfinity(Player player, Item item) {
        long mixed = Infinity.None.EffectiveRequirement.Items;
        foreach (IStandardGroup type in item.UsedConsumableTypes()) {
            long inf = type.GetMaxInfinity(player, item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public static bool OwnsItem(Player player, Item item, bool isACopy){
        foreach (IStandardGroup type in item.UsedConsumableTypes()){
            if(!type.OwnsConsumable(player, item, isACopy)) return false;
        }
        return true;
    }

    // TODO >>> test requiremeent display
    public override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        if(Configs.RequirementSettings.Instance.MaxConsumableTypes == 0) return;

        Player player = Main.LocalPlayer;
        Category category = 0;
        IRequirement root = item.GetRequirement(UID);
        // ItemCount effective;
        Infinity infinity;
        ItemCount itemCount;

        if (OwnsItem(player, item, true) && OwnsItem(player, item, true)) {
            // effective = player.GetEffectiveRequirement(item, UID);
            infinity = player.GetInfinity(item, UID);
            itemCount = new(CountConsumables(player, item), item.maxStack);
        } else {
            // effective = ItemCount.None;
            infinity = Infinity.None;
            itemCount = ItemCount.None;
        }

        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < GetMaxInfinity(player, item) ?
            root.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(category, infinity, next) & DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.DisplayFlags.Infinity) == 0) return;

        TooltipLine line = tooltips.AddLine("Material", TooltipHelper.AddedLine("Mixed", ""));
        Globals.InfinityDisplayItem.DisplayOnLine(ref line.Text, ref line.OverrideColor, next.IsNone ? InfinityColor : PartialInfinityColor, displayFlags, category, infinity, next, itemCount);
        line.Text = line.Text.Replace("  ", " ");
    }

    public override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}

    public override Item ToConsumable(Item item) => item;
    public override int CacheID(Item consumable) => consumable.type;

    public override Item ToItem(Item consumable) => consumable;
}
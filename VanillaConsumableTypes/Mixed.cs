using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

using SPIC.ConsumableTypes;
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
        Infinity min = ConsumableTypes.Infinity.None;
        foreach(IConsumableType type in InfinityManager.ConsumableTypes()){
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
        foreach(IConsumableType type in InfinityManager.ConsumableTypes()){
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
internal class Mixed : ConsumableType<Mixed>, IConsumableType<MixedCategory> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => Terraria.ID.ItemID.LunarHook;

    public Color Color => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
    public static Color PartialInfinityColor => new(255, (byte)(Main.masterColor * 200f), 0);

    public MixedCategory GetCategory(Item item) {
        foreach (IConsumableType type in item.UsedConsumableTypes()) {
            if (!item.GetCategory(type.UID).IsNone) return MixedCategory.Mixed;
        }
        return MixedCategory.AllNone;
    }

    public IRequirement GetRequirement(Item item) {
        return new MixedRequirement(item);
    }

    static DisplayFlags DisplayFlags => DisplayFlags.Infinity | DisplayFlags.Requirement;

    public long CountItems(Player player, Item item) {
        long count = long.MaxValue;
        foreach (IConsumableType type in item.UsedConsumableTypes()) {
            long c = type.CountItems(player, item);
            if (c < count) count = c;
        }
        return count;
    }

    public static long GetMaxInfinity(Player player, Item item) {
        long mixed = Infinity.None.EffectiveRequirement.Items;
        foreach (IDefaultDisplay type in item.UsedConsumableTypes()) {
            long inf = type.GetMaxInfinity(player, item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public static bool OwnsItem(Player player, Item item, bool isACopy){
        foreach (IDefaultDisplay type in item.UsedConsumableTypes()){
            if(!type.OwnsItem(player, item, isACopy)) return false;
        }
        return true;
    }

    // TODO >>> test requiremeent display
    public void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        if(Configs.RequirementSettings.Instance.MaxConsumableTypes == 0) return;

        Player player = Main.LocalPlayer;
        Category category = item.GetCategory(UID);
        IRequirement root = item.GetRequirement(UID);
        // ItemCount effective;
        Infinity infinity;
        ItemCount itemCount;

        if (OwnsItem(player, item, true) && OwnsItem(player, item, true)) {
            // effective = player.GetEffectiveRequirement(item, UID);
            infinity = InfinityManager.GetInfinity(player, item, UID);
            itemCount = new(CountItems(player, item), item.maxStack);
        } else {
            // effective = ItemCount.None;
            infinity = Infinity.None;
            itemCount = ItemCount.None;
        }

        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < GetMaxInfinity(player, item) ?
            root.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        DisplayFlags displayFlags = DefaultImplementation.GetDisplayFlags(category, infinity, next) & DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & DisplayFlags.Infinity) == 0) return;

        TooltipLine line = tooltips.AddLine("Material", TooltipHelper.AddedLine("Mixed", ""));
        DefaultImplementation.DisplayOnLine(ref line.Text, ref line.OverrideColor, next.IsNone ? Color : PartialInfinityColor, displayFlags, category, infinity, next, itemCount);
        line.Text = line.Text.Replace("  ", " ");
    }

    public void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
}
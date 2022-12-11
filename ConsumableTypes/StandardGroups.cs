using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public abstract class StandardGroup<TImplementation, TConsumable, TCount> : ConsumableGroup<TImplementation, TConsumable, TCount>, IStandardGroup<TConsumable, TCount>
where TImplementation : StandardGroup<TImplementation, TConsumable, TCount> where TConsumable : notnull where TCount : ICount<TCount> {
    public abstract Color DefaultColor { get; }
    public virtual bool DefaultsToOn => true;

    public override bool OwnsItem(Player player, Item item, bool isACopy) {
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;

        if (item.playerIndexTheItemIsReservedFor != Main.myPlayer) return false;
        if (System.Array.Find(player.armor, i => AreSameItems(i, item)) is not null
                || System.Array.Find(player.miscEquips, i => AreSameItems(i, item)) is not null)
            return false;

        if (AreSameItems(Main.mouseItem, item)
                || System.Array.Find(player.inventory, i => AreSameItems(i, item)) is not null
                || (player.InChest(out var chest) && System.Array.Find(chest, i => AreSameItems(i, item)) is not null)
                || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item)))
            return true;

        return false;
    }

    public virtual TooltipLineID LinePosition => System.Enum.TryParse(TooltipLine.Name, out TooltipLineID index) ? index : TooltipLineID.Modded;
    public abstract TooltipLine TooltipLine { get; }

    public sealed override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        Globals.DisplayInfo<TCount> info = this.GetDisplayInfo(item, true, out TConsumable values);

        if ((info.DisplayFlags & Globals.InfinityDisplayItem.LineDisplayFlags) != 0) {
            TooltipLine line = ToConsumable(item).Equals(values) ? tooltips.FindorAddLine(TooltipLine, LinePosition, out bool addedLine) : tooltips.FindorAddLine(((IStandardAmmunition<TConsumable>)this).WeaponLine(ToConsumable(item), values), TooltipLineID.WandConsumes, out addedLine);
            Globals.InfinityDisplayItem.DisplayOnLine(ref line.Text, ref line.OverrideColor, this.Color(), info);
            if (addedLine) line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
        }
    }

    public sealed override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if ((this.GetDisplayInfo(item, false, out _).DisplayFlags & Globals.InfinityDisplayItem.DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayDot.Add(this);
    }
    public void ActualDrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position) {
        Globals.DisplayInfo<TCount> info = this.GetDisplayInfo(item, false, out _);
        Globals.InfinityDisplayItem.DisplayDot(spriteBatch, position, this.Color(), info);
    }

    public sealed override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if ((this.GetDisplayInfo(item, false, out _).DisplayFlags & Globals.InfinityDisplayItem.DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayGlow.Add(this);
    }
    public void ActualDrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale) {
        Globals.DisplayInfo<TCount> info = this.GetDisplayInfo(item, false, out _);
        Globals.InfinityDisplayItem.DisplayGlow(spriteBatch, item, position, origin, frame, scale, this.Color(), info);
    }
}
public abstract class StandardGroup<TImplementation, TConsumable, TCount, TCategory> : StandardGroup<TImplementation, TConsumable, TCount>, ICategory<TConsumable, TCategory>
where TCategory : System.Enum where TConsumable : notnull where TCount : ICount<TCount>
where TImplementation : StandardGroup<TImplementation, TConsumable, TCount, TCategory> {
    internal override ConsumableCache<TCount, TCategory> CreateCache() => new();
    public sealed override int ReqCacheID(TConsumable consumable) => System.Convert.ToInt32(InfinityManager.GetCategory(consumable, this));

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement<TCount> Requirement(TCategory category);
    public sealed override Requirement<TCount> GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

public abstract class ItemGroup<TImplementation> : StandardGroup<TImplementation, Item, ItemCount>
where TImplementation : ItemGroup<TImplementation> {
    public static void Register() => InfinityManager.Register(Instance);

    public sealed override string Key(Item consumable) => new Terraria.ModLoader.Config.ItemDefinition(consumable.type).ToString();
    public sealed override int CacheID(Item consumable) => consumable.type;
    public sealed override Item ToConsumable(Item item) => item;

    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);
    public sealed override ItemCount LongToCount(Item item, long count) => new(item) { Items = count };
}
public abstract class ItemGroup<TImplementation, TCategory> : ItemGroup<TImplementation>, ICategory<Item, TCategory>
where TCategory : System.Enum
where TImplementation : ItemGroup<TImplementation, TCategory> {
    internal override ConsumableCache<ItemCount, TCategory> CreateCache() => new();
    public sealed override int ReqCacheID(Item consumable) => System.Convert.ToInt32(InfinityManager.GetCategory(consumable, this));
    public abstract TCategory GetCategory(Item consumable);
    public abstract Requirement<ItemCount> Requirement(TCategory category);
    public sealed override Requirement<ItemCount> GetRequirement(Item consumable) => Requirement(consumable.GetCategory(this));
}

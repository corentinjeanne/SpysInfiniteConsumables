using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public abstract class StandardGroup<TImplementation, TConsumable> : ConsumableGroup<TImplementation, TConsumable>, IStandardGroup<TConsumable>
where TImplementation : StandardGroup<TImplementation, TConsumable> where TConsumable : notnull {
    public abstract Color DefaultColor { get; }
    public virtual bool DefaultsToOn => true;

    public sealed override void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        Player player = Main.LocalPlayer;
        TConsumable consumable = ToConsumable(item);

        bool addedLine;
        TConsumable values;
        TooltipLine line;
        if(this is IAlternateDisplay<TConsumable> altDisplay && altDisplay.HasAlternate(player, consumable, out TConsumable? alt)){
            values = alt;
            line = tooltips.FindorAddLine(altDisplay!.AlternateTooltipLine(consumable, values), "WandConsumes", out addedLine);
        }else{
            values = consumable;
            line = tooltips.FindorAddLine(TooltipLine, LinePosition, out addedLine);
        }

        Category category = GetCategory_Fix(values);
        Requirement requirement = InfinityManager.GetRequirement(values, this);
        Infinity infinity;
        ICount consumableCount;
        if (OwnsItem(player, item, true)) {
            consumableCount = LongToCount(values, CountConsumables(player, values));
            infinity = InfinityManager.GetInfinity(player, values, this);
        }
        else {
            consumableCount = LongToCount(values, 0).None;
            infinity = new(consumableCount, 0);
        }

        ICount next = infinity.Value.IsNone || infinity.Value.CompareTo(LongToCount(values, GetMaxInfinity(player, values))) < 0 ?
            requirement.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(category, infinity, next) & Config.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.LineDisplayFlags) == 0) return;

        Globals.InfinityDisplayItem.DisplayOnLine(ref line.Text, ref line.OverrideColor, ((IColorable)this).Color, displayFlags, category, infinity, next, consumableCount);
        if (addedLine) line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
    }
    internal virtual Category GetCategory_Fix(TConsumable values) => Category.None; // TODO naming

    public sealed override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Player player = Main.LocalPlayer;
        
        TConsumable consumable = ToConsumable(item);
        TConsumable values = this is IAlternateDisplay<TConsumable> altDisplay && altDisplay.HasAlternate(player, consumable, out TConsumable? alt)? alt : consumable;
        Requirement requirement = InfinityManager.GetRequirement(values, this);

        Infinity infinity;
        ICount consumableCount;
        if (OwnsItem(player, item, true)) {
            consumableCount = LongToCount(values, CountConsumables(player, values));
            infinity = InfinityManager.GetInfinity(player, values, this);
        } else {
            consumableCount = LongToCount(values, 0).None;
            infinity = new(consumableCount, 0);
        }

        ICount next = infinity.EffectiveRequirement.IsNone || infinity.Value.CompareTo(LongToCount(values, GetMaxInfinity(player, values))) < 0 ?
            requirement.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(Category.None, infinity, next) & Config.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayDot.Add(new(((IColorable)this).Color, displayFlags, infinity, next, consumableCount));

    }
    public sealed override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Player player = Main.LocalPlayer;
        TConsumable consumable = ToConsumable(item);
        TConsumable values = this is IAlternateDisplay<TConsumable> altDisplay && altDisplay.HasAlternate(player, consumable, out TConsumable? alt) ? alt : consumable;

        Requirement root = InfinityManager.GetRequirement(values, this);
        Infinity infinity;
        ICount consumableCount;

        if (OwnsItem(player, item, true)) {
            consumableCount = LongToCount(values, CountConsumables(player, values));
            infinity = InfinityManager.GetInfinity(player, values, this);
        } else {
            consumableCount = LongToCount(values, 0).None;
            infinity = new(consumableCount, 0);
        }

        ICount next = infinity.Value.IsNone || infinity.Value.CompareTo(LongToCount(values, GetMaxInfinity(player, values))) < 0 ?
            root.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(Category.None, infinity, next) & Config.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.GlowDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayGlow.Add(new(((IColorable)this).Color, displayFlags, infinity, next, consumableCount));
    }

    public virtual bool OwnsItem(Player player, Item item, bool isACopy) {
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

    public virtual long GetMaxInfinity(Player player, TConsumable consumable) => 0;

    public virtual string LinePosition => TooltipLine.Name;
    public abstract TooltipLine TooltipLine { get; }
}
public abstract class StandardGroup<TImplementation, TConsumable, TCategory> : StandardGroup<TImplementation, TConsumable>, ICategory<TConsumable, TCategory>
where TCategory : System.Enum where TConsumable : notnull
where TImplementation : StandardGroup<TImplementation, TConsumable, TCategory> {
    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement Requirement(TCategory category);
    public sealed override Requirement GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
    internal override Category GetCategory_Fix(TConsumable values) => InfinityManager.GetCategory(values, this);
}

public abstract class ItemGroup<TImplementation> : StandardGroup<TImplementation, Item>
where TImplementation : ItemGroup<TImplementation> {
    public static void Register() => InfinityManager.Register(Instance);

    public sealed override string Key(Item consumable) => new Terraria.ModLoader.Config.ItemDefinition(consumable.type).ToString();
    public sealed override int CacheID(Item consumable) => consumable.type;
    public sealed override Item ToConsumable(Item item) => item;

    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);
    public sealed override ICount LongToCount(Item item, long count) => new ItemCount(item, count);
}
public abstract class ItemGroup<TImplementation, TCategory> : ItemGroup<TImplementation>, ICategory<Item, TCategory>
where TCategory : System.Enum
where TImplementation : ItemGroup<TImplementation, TCategory> {
    public abstract TCategory GetCategory(Item consumable);
    public abstract Requirement Requirement(TCategory category);
    public sealed override Requirement GetRequirement(Item consumable) => Requirement(consumable.GetCategory(this));
}

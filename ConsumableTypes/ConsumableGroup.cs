using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public abstract class ConsumableGroup<TImplementation, TConsumable> : IConsumableGroup<TConsumable>
where TImplementation : ConsumableGroup<TImplementation, TConsumable> where TConsumable : notnull {
    public static TImplementation Instance => _instance ??= System.Activator.CreateInstance<TImplementation>();
    private static TImplementation? _instance;
    public static int ID => Instance.UID;

    public static void RegisterAsGlobal() => InfinityManager.RegisterAsGlobal(Instance);

    public abstract Mod Mod { get; }
    public virtual string Name => GetType().Name;
    public int UID { get; internal set; }
    public abstract int IconType { get; }

    public abstract TConsumable ToConsumable(Item item);
    public abstract int CacheID(TConsumable consumable);

    public abstract IRequirement GetRequirement(TConsumable consumable);

    public abstract long CountConsumables(Player player, TConsumable consumable);
    public abstract ICount LongToCount(TConsumable consumable, long count);

    public abstract void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    public abstract void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public abstract void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
}
public abstract class ConsumableGroup<TImplementation, TConsumable, TCategory> : ConsumableGroup<TImplementation, TConsumable>, ICategory<TConsumable, TCategory>
where TCategory : System.Enum where TConsumable : notnull
where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCategory> {
    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract IRequirement Requirement(TCategory category);
    public sealed override IRequirement GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

public abstract class StandardGroup<TImplementation, TConsumable> : ConsumableGroup<TImplementation, TConsumable>, IStandardGroup<TConsumable>
where TImplementation : StandardGroup<TImplementation, TConsumable> where TConsumable : notnull {
    public abstract Color DefaultColor { get; }
    public virtual bool DefaultsToOn => true;

    public sealed override void ModifyTooltip(Item item, List<TooltipLine> tooltips){
        TConsumable consumable = ToConsumable(item);
        TConsumable values = consumable; // TODO alternate (x3)
        Player player = Main.LocalPlayer;

        Category category = this is ICategory ? InfinityManager.GetCategory(values, UID) : Category.None;
        IRequirement requirement = InfinityManager.GetRequirement(values, UID);

        Infinity infinity;
        ICount consumableCount;

        if (OwnsItem(player, item, true)) {
            consumableCount = LongToCount(values, CountConsumables(player, values));
            infinity = InfinityManager.GetInfinity(player, values, UID);
        } else {
            consumableCount = LongToCount(values, 0).None;
            infinity = new(consumableCount,0);
        }

        ICount next = infinity.Value.IsNone || infinity.Value.CompareTo(LongToCount(values, GetMaxInfinity(player, values))) < 0 ?
            requirement.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(category, infinity, next) & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.OnLineDisplayFlags) == 0) return;

        TooltipLine line = tooltips.FindorAddLine(TooltipLine, LinePosition, out bool addedLine);
        Globals.InfinityDisplayItem.DisplayOnLine(ref line.Text, ref line.OverrideColor, ((IColorable)this).Color, displayFlags, category, infinity, next, consumableCount);
        if (addedLine) line.OverrideColor = (line.OverrideColor ?? Color.White) * 0.75f;
    }
    public sealed override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
        TConsumable consumable = ToConsumable(item);
        TConsumable values = consumable;
        Player player = Main.LocalPlayer;
        IRequirement root = InfinityManager.GetRequirement(values, UID);

        Infinity infinity;

        if (OwnsItem(player, item, true)) {
            infinity = InfinityManager.GetInfinity(player, values, UID);
        } else {
            infinity = new(LongToCount(values, 0).None, 0);
        }
        ICount next = infinity.EffectiveRequirement.IsNone || infinity.Value.CompareTo(LongToCount(values, GetMaxInfinity(player, values))) < 0 ?
            root.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(Category.None, infinity, next) & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayDot.Add(new(((IColorable)this).Color, displayFlags, infinity, next));

    }
    public sealed override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        TConsumable consumable = ToConsumable(item);
        TConsumable values = consumable;
        Player player = Main.LocalPlayer;

        IRequirement root = InfinityManager.GetRequirement(values, UID);
        Infinity infinity;
        if (OwnsItem(player, item, true)) {
            infinity = InfinityManager.GetInfinity(player, values, UID);
        } else {
            infinity = new(LongToCount(values, 0).None, 0);
        }

        ICount next = infinity.Value.IsNone || infinity.Value.CompareTo(LongToCount(values, GetMaxInfinity(player, values))) < 0 ?
            root.NextRequirement(infinity.EffectiveRequirement) : infinity.Value.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(Category.None, infinity, next) & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.GlowDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayGlow.Add(new(((IColorable)this).Color, displayFlags, infinity, next));
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
    public abstract IRequirement Requirement(TCategory category);
    public sealed override IRequirement GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

public abstract class ItemGroup<TImplementation> : StandardGroup<TImplementation, Item>
where TImplementation : ItemGroup<TImplementation> {
    public static void Register() => InfinityManager.Register(Instance);


    public sealed override int CacheID(Item consumable) => consumable.type;
    public sealed override Item ToConsumable(Item item) => item;

    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);
    public sealed override ICount LongToCount(Item item, long count) => new ItemCount(item, count);
}
public abstract class ItemGroup<TImplementation, TCategory> : ItemGroup<TImplementation>, ICategory<Item, TCategory>
where TCategory : System.Enum
where TImplementation : ItemGroup<TImplementation, TCategory> {
    public abstract TCategory GetCategory(Item consumable);
    public abstract IRequirement Requirement(TCategory category);
    public sealed override IRequirement GetRequirement(Item consumable) => Requirement(GetCategory(consumable));
}

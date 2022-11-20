using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public abstract class ConsumableGroup<TImplementation, TConsumable> : IConsumableGroup<TConsumable>
where TImplementation : ConsumableGroup<TImplementation, TConsumable>, IConsumableGroup, new() {
    public static TImplementation Instance => _instance ??= new TImplementation();
    private static TImplementation _instance;
    public static int ID => Instance.UID;

    public static void RegisterAsGlobal() => InfinityManager.RegisterAsGlobal(Instance);

    public abstract Mod Mod { get; }
    public virtual string Name => GetType().Name;
    public int UID { get; internal set; }
    public abstract int IconType { get; }

    public abstract Item ToItem(TConsumable consumable);
    public abstract TConsumable ToConsumable(Item item);
    public abstract int CacheID(TConsumable consumable);
    public abstract long CountConsumables(Player player, TConsumable consumable);
    public abstract IRequirement GetRequirement(TConsumable consumable);

    public abstract void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    public abstract void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public abstract void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
}
public abstract class ConsumableGroup<TImplementation, TConsumable, TCategory> : ConsumableGroup<TImplementation, TConsumable>, ICategory<TConsumable, TCategory>
where TCategory : System.Enum
where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCategory>, IConsumableGroup, new () {
    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract IRequirement Requirement(TCategory category);
    public sealed override IRequirement GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

public abstract class StandardGroup<TImplementation, TConsumable> : ConsumableGroup<TImplementation, TConsumable>, IStandardGroup<TConsumable>
where TImplementation : StandardGroup<TImplementation, TConsumable>, IConsumableGroup, new() {
    public abstract Color DefaultColor { get; }
    public virtual bool DefaultsToOn => true;

    public sealed override void ModifyTooltip(Item item, List<TooltipLine> tooltips){
        TConsumable consumable = ToConsumable(item);
        TConsumable values = consumable; // TODO alternate
        Player player = Main.LocalPlayer;

        Category category = this is ICategory ? InfinityManager.GetCategory(values, UID) : Category.None;
        IRequirement requirement = InfinityManager.GetRequirement(values, UID);

        Infinity infinity;
        ItemCount itemCount;

        if (OwnsConsumable(player, consumable, true)) {
            infinity = InfinityManager.GetInfinity(player, values, UID);
            itemCount = new(CountConsumables(player, values), item.maxStack);
        } else {
            infinity = Infinity.None;
            itemCount = ItemCount.None;
        }

        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < GetMaxInfinity(player, consumable) ?
            requirement.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;


        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(category, infinity, next) & DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.OnLineDisplayFlags) == 0) return;

        TooltipLine line = tooltips.FindorAddLine(TooltipLine, LinePosition, out bool addedLine);
        Globals.InfinityDisplayItem.DisplayOnLine(ref line.Text, ref line.OverrideColor, ((IColorable)this).Color, displayFlags, category, infinity, next, itemCount);
        if (addedLine) line.OverrideColor *= 0.75f;
    }
    public sealed override void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
        TConsumable consumable = ToConsumable(item);
        TConsumable values = consumable; // TODO alternate
        Player player = Main.LocalPlayer;
        IRequirement root = InfinityManager.GetRequirement(values, UID);

        Infinity infinity;
        // ItemCount itemCount;

        if (OwnsConsumable(player, consumable, true)) {
            infinity = InfinityManager.GetInfinity(player, values, UID);
            // itemCount = new(CountConsumables(player, values), 999);
        } else {
            infinity = Infinity.None;
            // itemCount = ItemCount.None;
        }
        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < GetMaxInfinity(player, values) ?
            root.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(Category.None, infinity, next) & DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayDot.Add(new(((IColorable)this).Color, displayFlags, infinity, next));

    }
    public sealed override void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        TConsumable consumable = ToConsumable(item);
        TConsumable values = consumable; // TODO alternate
        Player player = Main.LocalPlayer;

        IRequirement root = InfinityManager.GetRequirement(values, UID);

        Infinity infinity;
        // ItemCount itemCount;

        if (OwnsConsumable(player, consumable, true)) {
            infinity = InfinityManager.GetInfinity(player, values, UID);
            // itemCount = new(CountConsumables(player, values), item.maxStack);
        } else {
            infinity = Infinity.None;
            // itemCount = ItemCount.None;
        }
        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < GetMaxInfinity(player, values) ?
            root.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        Globals.DisplayFlags displayFlags = Globals.InfinityDisplayItem.GetDisplayFlags(Category.None, infinity, next) & DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & Globals.InfinityDisplayItem.GlowDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayGlow.Add(new(((IColorable)this).Color, displayFlags, infinity, next));

    }

    public virtual Globals.DisplayFlags DisplayFlags => Globals.DisplayFlags.All;
    public abstract bool OwnsConsumable(Player player, TConsumable consumable, bool isACopy);
    
    public virtual long GetMaxInfinity(Player player, TConsumable consumable) => Infinity.None.Value.Items;
    
    public virtual string LinePosition => TooltipLine.Name;
    public abstract TooltipLine TooltipLine { get; }
}
public abstract class StandardGroup<TImplementation, TConsumable, TCategory> : StandardGroup<TImplementation, TConsumable>, ICategory<TConsumable, TCategory>
where TCategory : System.Enum
where TImplementation : StandardGroup<TImplementation, TConsumable, TCategory>, IConsumableGroup, new() {
    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract IRequirement Requirement(TCategory category);
    public sealed override IRequirement GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

public abstract class ItemGroup<TImplementation> : StandardGroup<TImplementation, Item>
where TImplementation : ItemGroup<TImplementation>, IConsumableGroup, new() {
    public static void Register() => InfinityManager.Register(Instance);

    public sealed override Item ToItem(Item consumable) => consumable;

    public sealed override int CacheID(Item consumable) => consumable.type;
    public sealed override Item ToConsumable(Item item) => item;

    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);
    public override bool OwnsConsumable(Player player, Item item, bool isACopy) {
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
}
public abstract class ItemGroup<TImplementation, TCategory> : ItemGroup<TImplementation>, ICategory<Item, TCategory>
where TCategory : System.Enum
where TImplementation : ItemGroup<TImplementation, TCategory>, IConsumableGroup, new() {
    public abstract TCategory GetCategory(Item consumable);
    public abstract IRequirement Requirement(TCategory category);
    public sealed override IRequirement GetRequirement(Item consumable) => Requirement(GetCategory(consumable));
}

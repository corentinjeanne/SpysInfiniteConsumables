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
    public abstract string Key(TConsumable consumable);
    public abstract int CacheID(TConsumable consumable);

    public abstract Requirement GetRequirement(TConsumable consumable);

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
    public abstract Requirement Requirement(TCategory category);
    public sealed override Requirement GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

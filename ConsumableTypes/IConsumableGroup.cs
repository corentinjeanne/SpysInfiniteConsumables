using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public interface IConsumableGroup {
    Mod Mod { get; }
    string Name { get; }
    int UID { get; }
    int IconType { get; }

    object ToConsumable(Item item);
    int CacheID(object consumable);
    IRequirement GetRequirement(object consumable);
    long CountConsumables(Player player, object consumable);
    ICount LongToCount(object consumable, long count);

    void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);

}
public interface IConsumableGroup<TConsumable> : IConsumableGroup where TConsumable : notnull {
    object IConsumableGroup.ToConsumable(Item item) => ToConsumable(item);
    int IConsumableGroup.CacheID(object consumable) => CacheID((TConsumable)consumable);
    IRequirement IConsumableGroup.GetRequirement(object consumable) => GetRequirement((TConsumable)consumable);
    long IConsumableGroup.CountConsumables(Player player, object consumable) => CountConsumables(player, (TConsumable)consumable);
    ICount IConsumableGroup.LongToCount(object consumable, long count) => LongToCount((TConsumable)consumable, count);

    new TConsumable ToConsumable(Item item);
    int CacheID(TConsumable consumable);
    IRequirement GetRequirement(TConsumable consumable);
    long CountConsumables(Player player, TConsumable consumable);
    ICount LongToCount(TConsumable consumable, long count);
}

public interface IStandardGroup : IConsumableGroup, IToggleable, IColorable {
    string LinePosition { get; }
    TooltipLine TooltipLine { get; }
    
    long GetMaxInfinity(Player player, object consumable);
    bool OwnsItem(Player player, Item item, bool isACopy);
}
public interface IStandardGroup<TConsumable> : IConsumableGroup<TConsumable>, IStandardGroup where TConsumable : notnull {
    long IStandardGroup.GetMaxInfinity(Player player, object consumable) => GetMaxInfinity(player, (TConsumable)consumable);

    long GetMaxInfinity(Player player, TConsumable consumable);
}

public interface ICategory : IConsumableGroup {
    Category GetCategory(object consumable);
    IRequirement Requirement(Category category);
}
public interface ICategory<TConsumable> : IConsumableGroup<TConsumable>, ICategory where TConsumable : notnull {
    Category ICategory.GetCategory(object consumable) => GetCategory((TConsumable)consumable);
    IRequirement ICategory.Requirement(Category category) => Requirement(category);

    new IRequirement Requirement(Category category);
    Category GetCategory(TConsumable consumable);
}
public interface ICategory<TConsumable, TCategory> : ICategory<TConsumable> where TConsumable : notnull where TCategory : System.Enum {
    Category ICategory<TConsumable>.GetCategory(TConsumable consumable) => GetCategory(consumable);
    IRequirement ICategory<TConsumable>.Requirement(Category category) => Requirement((TCategory)category);

    new TCategory GetCategory(TConsumable consumable);
    IRequirement Requirement(TCategory category);
}
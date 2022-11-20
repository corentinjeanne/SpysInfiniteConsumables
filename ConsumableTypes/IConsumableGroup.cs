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

    int CacheID(Item item);
    IRequirement GetRequirement(Item item);
    long CountConsumables(Player player, Item item);

    void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);

}
public interface IConsumableGroup<TConsumable> : IConsumableGroup {
    IRequirement IConsumableGroup.GetRequirement(Item item) => GetRequirement(ToConsumable(item));
    long IConsumableGroup.CountConsumables(Player player, Item item) => CountConsumables(player, ToConsumable(item));
    int IConsumableGroup.CacheID(Item item) => CacheID(ToConsumable(item));

    Item ToItem(TConsumable consumable); // temporary
    TConsumable ToConsumable(Item item);
    IRequirement GetRequirement(TConsumable consumable);
    int CacheID(TConsumable consumable);
    long CountConsumables(Player player, TConsumable consumable);
}

public interface ICategory : IConsumableGroup {
    Category GetCategory(Item item);
    IRequirement Requirement(Category category);
}
public interface ICategory<TConsumable> : IConsumableGroup<TConsumable>, ICategory{
    Category ICategory.GetCategory(Item item) => GetCategory(ToConsumable(item));
    Category GetCategory(TConsumable consumable);
}
public interface ICategory<TConsumable, TCategory> : ICategory<TConsumable> where TCategory : System.Enum{
    Category ICategory<TConsumable>.GetCategory(TConsumable consumable) => GetCategory(consumable);
    IRequirement ICategory.Requirement(Category category) => Requirement((TCategory)category);

    new TCategory GetCategory(TConsumable consumable);
    IRequirement Requirement(TCategory category);
}

public interface IStandardGroup : IConsumableGroup, IToggleable, IColorable {
    long GetMaxInfinity(Player player, Item item);
    
    string LinePosition { get; }
    TooltipLine TooltipLine { get; }
    
    Globals.DisplayFlags DisplayFlags { get; }
    bool OwnsConsumable(Player player, Item item, bool isACopy);
}
public interface IStandardGroup<TConsumable> : IConsumableGroup<TConsumable>, IStandardGroup {
    long IStandardGroup.GetMaxInfinity(Player player, Item item) => GetMaxInfinity(player, ToConsumable(item));
    bool IStandardGroup.OwnsConsumable(Player player, Item item, bool isACopy) => OwnsConsumable(player, ToConsumable(item), isACopy);
    
    bool OwnsConsumable(Player player, TConsumable consumable, bool isACopy);
    long GetMaxInfinity(Player player, TConsumable consumable) => Infinity.None.Value.Items;
}

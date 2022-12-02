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
    string Key(object consumable);
    int CacheID(object consumable);
    Requirement GetRequirement(object consumable);
    long CountConsumables(Player player, object consumable);
    ICount LongToCount(object consumable, long count);

    void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);

}
public interface IConsumableGroup<TConsumable> : IConsumableGroup where TConsumable : notnull {
    object IConsumableGroup.ToConsumable(Item item) => ToConsumable(item);
    string IConsumableGroup.Key(object consumable) => Key((TConsumable) consumable);
    int IConsumableGroup.CacheID(object consumable) => CacheID((TConsumable)consumable);
    Requirement IConsumableGroup.GetRequirement(object consumable) => GetRequirement((TConsumable)consumable);
    long IConsumableGroup.CountConsumables(Player player, object consumable) => CountConsumables(player, (TConsumable)consumable);
    ICount IConsumableGroup.LongToCount(object consumable, long count) => LongToCount((TConsumable)consumable, count);

    new TConsumable ToConsumable(Item item);
    string Key(TConsumable consumable);
    int CacheID(TConsumable consumable);
    Requirement GetRequirement(TConsumable consumable);
    long CountConsumables(Player player, TConsumable consumable);
    ICount LongToCount(TConsumable consumable, long count);
}

public interface IStandardGroup<TConsumable> : IConsumableGroup<TConsumable>, IToggleable, IColorable where TConsumable : notnull {
    string LinePosition { get; }
    TooltipLine TooltipLine { get; }
    bool OwnsItem(Player player, Item item, bool isACopy);
    long GetMaxInfinity(Player player, TConsumable consumable);
}
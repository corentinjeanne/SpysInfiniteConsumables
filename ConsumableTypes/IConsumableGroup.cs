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


    bool CanDisplay(Item item);
    void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
}
public interface IConsumableGroup<TConsumable> : IConsumableGroup where TConsumable:  notnull {
    TConsumable ToConsumable(Item item);
    string Key(TConsumable consumable);
    int ReqCacheID(TConsumable consumable);
    int CacheID(TConsumable consumable);
}
public interface IConsumableGroup<TConsumable, TCount> : IConsumableGroup<TConsumable> where TConsumable : notnull where TCount : ICount<TCount> {
    Requirement<TCount> GetRequirement(TConsumable consumable);
    TCount LongToCount(TConsumable consumable, long count);
    long CountConsumables(Player player, TConsumable consumable);
}


public interface IStandardGroup : IConsumableGroup, IToggleable, IColorable {
    TooltipLineID LinePosition { get; }
    TooltipLine TooltipLine { get; }
    bool OwnsItem(Player player, Item item, bool isACopy);
    void ActualDrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position);
    void ActualDrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale);
}
public interface IStandardGroup<TConsumable, TCount> : IStandardGroup, IConsumableGroup<TConsumable, TCount> where TConsumable : notnull where TCount : ICount<TCount> {
    long GetMaxInfinity(Player player, TConsumable consumable);
}
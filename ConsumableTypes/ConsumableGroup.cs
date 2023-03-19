using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public abstract class ConsumableGroup<TImplementation, TConsumable, TCount> : IConsumableGroup<TConsumable, TCount>
where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCount> where TConsumable : notnull where TCount : ICount<TCount> {
    public static TImplementation Instance => _instance ??= System.Activator.CreateInstance<TImplementation>();
    private static TImplementation? _instance;
    public static int ID => Instance.UID;

    public static void RegisterAsGlobal() => InfinityManager.RegisterAsGlobal(Instance);
    internal virtual ConsumableCache<TCount> CreateCache() => new();

    public abstract Mod Mod { get; }
    public string InternalName => GetType().Name;
    // Should be localized
    public virtual string Name => InternalName;
    public int UID { get; internal set; }
    public abstract int IconType { get; }

    public virtual bool CanDisplay(Item item){
        TConsumable consumable = ToConsumable(item);
        if(InfinityManager.IsBlacklisted(consumable, this)) return false;
        if(InfinityManager.IsUsed(consumable, this)) return true;
        if(this is IAmmunition<TConsumable> iAmmo && iAmmo.HasAmmo(Main.LocalPlayer, consumable, out TConsumable? ammo)
                && !InfinityManager.IsBlacklisted(ammo, this) && (UID > 0 || InfinityManager.IsUsed(ammo, this)))
            return true;

        return false;
    }


    public abstract TConsumable ToConsumable(Item item);

    public abstract string Key(TConsumable consumable);
    public abstract int CacheID(TConsumable consumable);

    public abstract bool Includes(TConsumable consumable);
    
    public abstract Requirement<TCount> GetRequirement(TConsumable consumable);
    public virtual long GetMaxInfinity(TConsumable consumable) => 0;

    public abstract long CountConsumables(Player player, TConsumable consumable);
    public abstract TCount LongToCount(TConsumable consumable, long count);


    public abstract bool OwnsItem(Player player, Item item, bool isACopy);
    public abstract void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    public abstract void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public abstract void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
}

public abstract class ConsumableGroup<TImplementation, TConsumable, TCount, TCategory> : ConsumableGroup<TImplementation, TConsumable, TCount>, ICategory<TConsumable, TCategory>
where TCategory : System.Enum where TConsumable : notnull where TCount : ICount<TCount>
where TImplementation : ConsumableGroup<TImplementation, TConsumable, TCount, TCategory> {
    public override bool Includes(TConsumable consumable) => ICategory<TConsumable, TCategory>.Includes(this, consumable);
    internal override ConsumableCache<TCount, TCategory> CreateCache() => new();
    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement<TCount> Requirement(TCategory category);
    public sealed override Requirement<TCount> GetRequirement(TConsumable consumable) => Requirement(GetCategory(consumable));
}

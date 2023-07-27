using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

// ItemID.Sets.ShimmerTransformToItem; // ? shimmer + chloro extra group (conversions) 

public interface IModGroup : ILocalizedModType, ILoadable {
    IModConsumable ModConsumable { get; }
    int IconType { get; }
    bool DefaultsToOn { get; }
    Color DefaultColor { get; }
    LocalizedText DisplayName { get; }

    long GetConsumedFromContext(Player player, Item item, out bool exclusive);

    (TooltipLine, TooltipLineID?) GetTooltipLine(Item item);
}

public abstract class ModGroup<TModConsumable, TConsumable> : ModType, IModGroup where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
    public string LocalizationCategory => "Groups";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);

    public abstract int IconType { get; }
    public virtual bool DefaultsToOn => true;
    public abstract Color DefaultColor { get; }

    protected sealed override void Register() {
        ModTypeLookup<ModGroup<TModConsumable, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
    public abstract Requirement GetRequirement(TConsumable consumable);
    public virtual IFullRequirement GetFullRequirement(TConsumable consumable) => new FullRequirement(ModConsumable.GetRequirement(consumable, this));
    public virtual (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, Name, DisplayName.Value), null);


    public virtual TConsumable DisplayedValue(TConsumable consumable) => consumable;
    public virtual long GetConsumedFromContext(Player player, Item item, out bool exclusive) {
        exclusive = false;
        return player.IsFromVisibleInventory(item) ? -1 : 0;
    }

    IModConsumable IModGroup.ModConsumable => ModConsumable;
    public TModConsumable ModConsumable { get; internal set; } = null!;
}

public abstract class ModGroup<TModConsumable, TConsumable, TCategory> : ModGroup<TModConsumable, TConsumable> where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull where TCategory : Enum {

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement GetRequirement(TCategory category);

    public sealed override Requirement GetRequirement(TConsumable consumable) => GetRequirement(ModConsumable.GetCategory(consumable, this));
    public override IFullRequirement GetFullRequirement(TConsumable consumable) {
        return new FullRequirement<TCategory>(ModConsumable.GetCategory(consumable, this), ModConsumable.GetRequirement(consumable, this));
    }
}


public abstract class ModGroupStatic<TGroup, TModConsumable, TConsumable> : ModGroup<TModConsumable, TConsumable> where TGroup : ModGroupStatic<TGroup, TModConsumable, TConsumable> where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
   
    public override void SetStaticDefaults() => Instance = (TGroup)this;
    public override void Unload() => Instance = null!;

    public static TGroup Instance = null!;
}
public abstract class ModGroupStatic<TGroup, TModConsumable, TConsumable, TCategory> : ModGroup<TModConsumable, TConsumable, TCategory> where TGroup : ModGroupStatic<TGroup, TModConsumable, TConsumable, TCategory> where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull where TCategory : Enum {
    public override void SetStaticDefaults() => Instance = (TGroup)this;
    public override void Unload() => Instance = null!;

    public static TGroup Instance = null!;
}
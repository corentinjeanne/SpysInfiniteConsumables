using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

// ItemID.Sets.ShimmerTransformToItem; // ? shimmer + chloro extra Infinity (conversions) 

public interface IInfinity : ILocalizedModType, ILoadable {
    IGroup Group { get; }
    bool Enabled { get; }
    Color Color { get; }
    int IconType { get; }
    bool DefaultsToOn { get; }
    Color DefaultColor { get; }
    LocalizedText DisplayName { get; }

    long GetConsumedFromContext(Player player, Item item, out bool exclusive);

    (TooltipLine, TooltipLineID?) GetTooltipLine(Item item);
}

public abstract class Infinity<TGroup, TConsumable> : ModType, IInfinity where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);

    public abstract int IconType { get; }
    public virtual bool DefaultsToOn => true;
    public abstract Color DefaultColor { get; }

    protected sealed override void Register() {
        ModTypeLookup<Infinity<TGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
    public abstract Requirement GetRequirement(TConsumable consumable);
    public virtual IFullRequirement GetFullRequirement(TConsumable consumable) => new FullRequirement(Group.GetRequirement(consumable, this));
    public virtual (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, Name, DisplayName.Value), null);


    public virtual TConsumable DisplayedValue(TConsumable consumable) => consumable;
    public virtual long GetConsumedFromContext(Player player, Item item, out bool exclusive) {
        exclusive = false;
        return player.IsFromVisibleInventory(item) ? -1 : 0;
    }

    public TGroup Group { get; internal set; } = null!;
    public bool Enabled { get; internal set; }
    public Color Color { get; internal set; }

    IGroup IInfinity.Group => Group;
}

public abstract class Infinity<TGroup, TConsumable, TCategory> : Infinity<TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, Enum {

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement GetRequirement(TCategory category);

    public sealed override Requirement GetRequirement(TConsumable consumable) => GetRequirement(Group.GetCategory(consumable, this));
    public override IFullRequirement GetFullRequirement(TConsumable consumable) {
        return new FullRequirement<TCategory>(Group.GetCategory(consumable, this), Group.GetRequirement(consumable, this));
    }
}


public abstract class InfinityStatic<TInfinity, TGroup, TConsumable> : Infinity<TGroup, TConsumable> where TInfinity : InfinityStatic<TInfinity, TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
   
    public override void SetStaticDefaults() => Instance = (TInfinity)this;
    public override void Unload() => Instance = null!;

    public static TInfinity Instance = null!;
}
public abstract class InfinityStatic<TInfinity, TGroup, TConsumable, TCategory> : Infinity<TGroup, TConsumable, TCategory> where TInfinity : InfinityStatic<TInfinity, TGroup, TConsumable, TCategory> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, Enum {
    public override void SetStaticDefaults() => Instance = (TInfinity)this;
    public override void Unload() => Instance = null!;

    public static TInfinity Instance = null!;
}
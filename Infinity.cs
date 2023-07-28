using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public interface IInfinity : ILocalizedModType, ILoadable {
    IGroup Group { get; }
    bool Enabled { get; }
    bool DefaultsToOn { get; }
    
    Color Color { get; }
    Color DefaultColor { get; }
    int IconType { get; }
    LocalizedText DisplayName { get; }

    long GetConsumedFromContext(Player player, Item item, out bool exclusive);
    (TooltipLine, TooltipLineID?) GetTooltipLine(Item item);
}

public abstract class InfinityRoot<TGroup, TConsumable> : ModType, IInfinity where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
    protected sealed override void Register() {
        ModTypeLookup<InfinityRoot<TGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() => SetStaticDefaults();

    public abstract Requirement GetRequirement(TConsumable consumable, FullInfinity steps);
    
    public virtual (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, Name, DisplayName.Value), null);

    public virtual TConsumable DisplayedValue(TConsumable consumable) => consumable;
    
    public virtual long GetConsumedFromContext(Player player, Item item, out bool exclusive) {
        exclusive = false;
        return player.IsFromVisibleInventory(item) ? -1 : 0;
    }

    public TGroup Group { get; internal set; } = null!;
    public bool Enabled { get; internal set; }
    public virtual bool DefaultsToOn => true;

    public abstract int IconType { get; }
    public Color Color { get; internal set; }
    public abstract Color DefaultColor { get; }
    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);
    
    IGroup IInfinity.Group => Group;
}

public abstract class Infinity<TGroup, TConsumable> : InfinityRoot<TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
    public override Requirement GetRequirement(TConsumable consumable, FullInfinity steps) => GetRequirement(consumable);

    public abstract Requirement GetRequirement(TConsumable consumable);
}

public abstract class Infinity<TGroup, TConsumable, TCategory> : InfinityRoot<TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, Enum {
    public override Requirement GetRequirement(TConsumable consumable, FullInfinity steps) {
        if(!Group.Config.HasCustomCategory(consumable, this, out TCategory category)) category = GetCategory(consumable);
        return GetRequirement(steps.Add(category));
    }

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement GetRequirement(TCategory category);
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
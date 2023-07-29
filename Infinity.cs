using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    (TooltipLine, TooltipLineID?) GetTooltipLine(Item item);
}

public abstract class InfinityRoot<TGroup, TConsumable> : ModType, IInfinity where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
    protected sealed override void Register() {
        ModTypeLookup<InfinityRoot<TGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() => SetStaticDefaults();

    public override void Unload() {
        DisplayOverrides = null;
        InfinityOverrides = null;
    }

    public abstract Requirement GetRequirement(TConsumable consumable, List<object> extras);
    
    public virtual (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, Name, DisplayName.Value), null);

    public virtual TConsumable DisplayedValue(TConsumable consumable) => consumable;

    public TGroup Group { get; internal set; } = null!;
    public bool Enabled { get; internal set; }
    public virtual bool DefaultsToOn => true;

    public abstract int IconType { get; }
    public Color Color { get; internal set; }
    public abstract Color DefaultColor { get; }
    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);
    
    IGroup IInfinity.Group => Group;

    public event OverrideInfinityFn? InfinityOverrides;
    public void OverrideInfinity(Player player, TConsumable consumable, Requirement requirement, long count, ref long infinity, List<object> extras) => InfinityOverrides?.Invoke(player, consumable, requirement,  count, ref infinity, extras);
    public delegate void OverrideInfinityFn(Player player, TConsumable consumable, Requirement requirement, long count, ref long infinity, List<object> extras);
    
    public event OverrideDisplayFn? DisplayOverrides;
    public void OverrideDisplay(Player player, Item item, TConsumable consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) => DisplayOverrides?.Invoke(player, item, consumable, ref requirement, ref count, extras, ref visibility);
    public delegate void OverrideDisplayFn(Player player, Item item, TConsumable consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility);
}

public abstract class Infinity<TGroup, TConsumable> : InfinityRoot<TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
    public override Requirement GetRequirement(TConsumable consumable, List<object> extras) => GetRequirement(consumable);

    public abstract Requirement GetRequirement(TConsumable consumable);
}

public abstract class Infinity<TGroup, TConsumable, TCategory> : InfinityRoot<TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, Enum {
    public override Requirement GetRequirement(TConsumable consumable, List<object> extras) {
        if(!Group.Config.HasCustomCategory(consumable, this, out TCategory category)) category = GetCategory(consumable);
        extras.Add(category);
        return GetRequirement(category);
    }

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement GetRequirement(TCategory category);
}

public abstract class InfinityStatic<TInfinity, TGroup, TConsumable> : Infinity<TGroup, TConsumable> where TInfinity : InfinityStatic<TInfinity, TGroup, TConsumable> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
    public override void SetStaticDefaults() => Instance = (TInfinity)this;
    public override void Unload() {
        Instance = null!;
        base.Unload();
    }

    public static TInfinity Instance = null!;
}
public abstract class InfinityStatic<TInfinity, TGroup, TConsumable, TCategory> : Infinity<TGroup, TConsumable, TCategory> where TInfinity : InfinityStatic<TInfinity, TGroup, TConsumable, TCategory> where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : struct, Enum {
    public override void SetStaticDefaults() => Instance = (TInfinity)this;
    public override void Unload() {
        Instance = null!;
        base.Unload();
    }

    public static TInfinity Instance = null!;
}
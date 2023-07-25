using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

// ItemID.Sets.ShimmerTransformToItem; // ? shimmer + chloro extra group (conversions) 

public interface IModGroup : ILocalizedModType, ILoadable {
    IMetaGroup MetaGroup { get; }
    int IconType { get; }
    bool DefaultsToOn { get; }
    Color DefaultColor { get; }
    LocalizedText DisplayName { get; }

    long GetConsumedFromContext(Player player, Item item, out bool exclusive);

    (TooltipLine, TooltipLineID?) GetTooltipLine(Item item);
}

public abstract class ModGroup<TMetaGroup, TConsumable> : ModType, IModGroup where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
    public string LocalizationCategory => "Groups";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);

    public abstract int IconType { get; }
    public virtual bool DefaultsToOn => true;
    public abstract Color DefaultColor { get; }

    protected sealed override void Register() {
        ModTypeLookup<ModGroup<TMetaGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
    public abstract Requirement GetRequirement(TConsumable consumable);
    public virtual IFullRequirement GetFullRequirement(TConsumable consumable) => new FullRequirement(MetaGroup.GetRequirement(consumable, this));
    public virtual (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, Name, DisplayName.Value), null);


    public virtual TConsumable DisplayedValue(TConsumable consumable) => consumable;
    public virtual long GetConsumedFromContext(Player player, Item item, out bool exclusive) {
        exclusive = false;
        return player.IsFromVisibleInventory(item) ? -1 : 0;
    }

    IMetaGroup IModGroup.MetaGroup => MetaGroup;
    public TMetaGroup MetaGroup { get; internal set; } = null!;
}

public abstract class ModGroup<TMetaGroup, TConsumable, TCategory> : ModGroup<TMetaGroup, TConsumable> where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> where TCategory : Enum {

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement GetRequirement(TCategory category);

    public sealed override Requirement GetRequirement(TConsumable consumable) => GetRequirement(MetaGroup.GetCategory(consumable, this));
    public override IFullRequirement GetFullRequirement(TConsumable consumable) {
        return new FullRequirement<TCategory>(MetaGroup.GetCategory(consumable, this), MetaGroup.GetRequirement(consumable, this));
    }
}


public abstract class ModGroupStatic<TGroup, TMetaGroup, TConsumable> : ModGroup<TMetaGroup, TConsumable> where TGroup : ModGroupStatic<TGroup, TMetaGroup, TConsumable> where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
   
    public override void SetStaticDefaults() => Instance = (TGroup)this;
    public override void Unload() => Instance = null!;

    public static TGroup Instance = null!;
}
public abstract class ModGroupStatic<TGroup, TMetaGroup, TConsumable, TCategory> : ModGroup<TMetaGroup, TConsumable, TCategory> where TGroup : ModGroupStatic<TGroup, TMetaGroup, TConsumable, TCategory> where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> where TCategory : Enum {
    public override void SetStaticDefaults() => Instance = (TGroup)this;
    public override void Unload() => Instance = null!;

    public static TGroup Instance = null!;
}
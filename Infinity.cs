using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public interface IInfinity : ILocalizedModType, ILoadable {
    IGroup Group { get; }
    bool Enabled { get; }

    Color Color { get; }
    int IconType { get; }
    LocalizedText DisplayName { get; }
}

public abstract class Infinity<TConsumable> : ModType, IInfinity where TConsumable : notnull {
    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityManager.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }
    public sealed override void SetupContent() => SetStaticDefaults();

    public abstract Requirement GetRequirement(TConsumable consumable, List<object> extras);
    
    public abstract Group<TConsumable> Group { get; }
    public virtual bool Enabled { get; set; } = true;

    public abstract int IconType { get; }
    public abstract Color Color { get; set; }
    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);
    
    IGroup IInfinity.Group => Group;

    public virtual void ModifyRequirement(TConsumable consumable, ref Requirement requirement, List<object> extras) {}

    public virtual void ModifyInfinity(Player player, TConsumable consumable, Requirement requirement, long count, ref long infinity, List<object> extras) {}

    public virtual void ModifyDisplay(Player player, Item item, TConsumable consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {}

    public virtual void ModifyDisplayedConsumables(TConsumable consumable, List<TConsumable> displayed) {}
}

public abstract class Infinity<TConsumable, TCategory> : Infinity<TConsumable> where TConsumable : notnull where TCategory : struct, Enum {

    public override Requirement GetRequirement(TConsumable consumable, List<object> extras) {
        TCategory category = GetCategory(consumable);
        extras.Add(category);
        HashSet<TCategory> categories = new();
        Requirement req = GetRequirement(category);
        while (req.Count < 0 && categories.Add(category)) {
            category = Enum.Parse<TCategory>((-req.Count).ToString());
            extras.Add(category);
            req = GetRequirement(category);
        }
        return req;
    }

    public override void ModifyRequirement(TConsumable consumable, ref Requirement requirement, List<object> extras) {
        if (!Group.Config.HasCustomCategory(consumable, this, out TCategory category)) return;
        extras.Clear();
        extras.Add(category);
        requirement = GetRequirement(category);
    }

    public abstract TCategory GetCategory(TConsumable consumable);
    public abstract Requirement GetRequirement(TCategory category);
}
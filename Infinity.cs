using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using SpikysLib;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace SPIC;

public interface IComponent {
    void Load(IInfinity infinity);
    void Unload();
    void Validate();
}


public interface IInfinity : ILocalizedModType, ILoadable {
    ReadOnlyCollection<IComponent> Components { get; }
    LocalizedText Label { get; }
    LocalizedText DisplayName { get; }
    LocalizedText Tooltip { get; }
}

public abstract class Infinity<TConsumable> : ModType, IInfinity {
    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityManager.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), () => $$"""{{{this.GetLocalizationKey("Tooltip")}}}""");
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }
    public sealed override void SetupContent() => SetStaticDefaults();

    public override void Load() {
        _components = GetComponents();
        foreach (IComponent component in Components) component.Load(this);
        InfinityManager.CountConsumablesEndpoint(this).Register(CountConsumables);
        InfinityManager.GetRequirementEndpoint(this).Register(GetRequirement);
    }

    public override void Unload() {
        foreach (IComponent component in Components) component.Unload();
        _components = null!;
    }

    public virtual GroupInfinity<TConsumable>? Group => null;
    public virtual int GetId(TConsumable consumable) => Group!.GetId(consumable);
    public virtual TConsumable ToConsumable(int id) => Group!.ToConsumable(id);

    protected virtual long CountConsumables(PlayerConsumable<TConsumable> args) => Group!.CountConsumables(args);
    protected virtual Requirement GetRequirement(TConsumable consumable) => throw new NotImplementedException();

    protected virtual IList<IComponent> GetComponents()
        => GetType().GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(f => f.FieldType.ImplementsInterface(typeof(IComponent), out _))
            .Select(f => (IComponent)f.GetValue(this)!)
            .ToArray();

    public ReadOnlyCollection<IComponent> Components => _components.AsReadOnly();
    private IList<IComponent> _components = null!;

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText Label => this.GetLocalization("Label");
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName");
    public virtual LocalizedText Tooltip => this.GetLocalization("Tooltip");
}

public abstract class Infinity<TConsumable, TCategory> : Infinity<TConsumable> where TCategory : struct, Enum {
    public override void Load() {
        base.Load();
        if (LoaderUtils.HasOverride(this, i => i.GetCategory)) InfinityManager.GetCategoryEndpoint(this).Register(GetCategory);
    }

    protected virtual TCategory GetCategory(TConsumable consumable) => throw new NotImplementedException();
    protected abstract Requirement GetRequirement(TCategory category);

    protected sealed override Requirement GetRequirement(TConsumable consumable) => GetRequirement(InfinityManager.GetCategory(consumable, this));
}

public abstract class GroupInfinity<TConsumable> : Infinity<TConsumable> {
    protected sealed override Requirement GetRequirement(TConsumable consumable) {
        long count = long.MaxValue;
        float multiplier = 1;
        return new(count, multiplier);
    }
}
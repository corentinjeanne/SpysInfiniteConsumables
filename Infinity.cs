using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using SpikysLib;
using SpikysLib.Configs;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace SPIC;

public interface IComponent {
    void Load(IInfinity infinity);
    void Unload();

    IInfinity Infinity { get; }
}


public interface IInfinity : ILocalizedModType, ILoadable {
    IEnumerable<IComponent> Components { get; }
    LocalizedText Label { get; }
    LocalizedText DisplayName { get; }
    LocalizedText Tooltip { get; }
}

public abstract class Infinity<TConsumable> : ModType, IInfinity, IComponent {
    public override void Load() {
        ConfigHelper.SetInstance(this);
        Components = GetComponents();
        foreach (IComponent component in Components) component.Load(this);
    }

    void IComponent.Load(IInfinity infinity) {
        InfinityManager.CountConsumablesEndpoint(this).Register(CountConsumables);
        InfinityManager.GetRequirementEndpoint(this).Register(GetRequirement);
    }

    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityManager.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), () => $$"""{{{this.GetLocalizationKey("Tooltip")}}}""");
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public sealed override void SetupContent() {
        Group?.RegisterChild(this);
        SetStaticDefaults();
    }

    public override void Unload() {
        foreach (IComponent component in Components) component.Unload();
        Components = null!;
        ConfigHelper.SetInstance(this, true);
    }

    public virtual GroupInfinity<TConsumable>? Group => null;
    public virtual int GetId(TConsumable consumable) => Group!.GetId(consumable);
    public virtual TConsumable ToConsumable(int id) => Group!.ToConsumable(id);

    protected virtual long CountConsumables(PlayerConsumable<TConsumable> args) => Group!.CountConsumables(args);
    protected virtual Requirement GetRequirement(TConsumable consumable) => throw new NotImplementedException();

    protected virtual IEnumerable<IComponent> GetComponents()
        => GetType().GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(f => f.FieldType.ImplementsInterface(typeof(IComponent), out _))
            .Select(f => (IComponent)f.GetValue(null)!);

    public IEnumerable<IComponent> Components { get; private set; } = null!;

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText Label => this.GetLocalization("Label");
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName");
    public virtual LocalizedText Tooltip => this.GetLocalization("Tooltip");

    IInfinity IComponent.Infinity => this;
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

public abstract class GroupInfinity<TConsumable> : Infinity<TConsumable>{
    protected sealed override Requirement GetRequirement(TConsumable consumable) {
        long count = 0;
        float multiplier = float.MaxValue;
        foreach (Infinity<TConsumable> infinity in _orderedInfinities) {
            Requirement requirement = InfinityManager.GetRequirement(consumable, infinity);
            if (requirement.IsNone) continue;
            count = Math.Max(count, requirement.Count);
            multiplier = Math.Min(multiplier, requirement.Multiplier);
        }
        return new(count, multiplier);
    }

    internal void RegisterChild(Infinity<TConsumable> infinity) {
        _infinities.Add(infinity);
        _orderedInfinities.Add(infinity);
    }

    public ReadOnlyCollection<Infinity<TConsumable>> Infinities => _orderedInfinities.AsReadOnly();
    private readonly List<Infinity<TConsumable>> _orderedInfinities = [];
    private readonly HashSet<Infinity<TConsumable>> _infinities = [];
}
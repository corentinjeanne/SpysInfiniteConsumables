using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using SPIC.Configs;
using SpikysLib;
using SpikysLib.Collections;
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
    protected override void InitTemplateInstance() => ConfigHelper.SetInstance(this);

    public override void Load() {
        foreach (IComponent component in GetComponents()) RegisterComponent(component);
        if (!Components.Contains(this)) RegisterComponent(this);
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
        if (Group is null) InfinityManager.RegisterRootInfinity(this);
        else Group.RegisterChild(this);
        SetStaticDefaults();
    }

    public void RegisterComponent(IComponent component) {
        _components.Add(component);
        component.Load(this);
    }

    public override void Unload() {
        foreach (IComponent component in _components) component.Unload();
        _components.Clear();
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

    public IEnumerable<IComponent> Components => _components.AsReadOnly();
    public readonly List<IComponent> _components = [];

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

public abstract class GroupInfinity<TConsumable> : Infinity<TConsumable>, IConfigurableComponents<GroupConfig> {
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

    void IConfigurableComponents<GroupConfig>.OnLoaded(GroupConfig config) {
        _orderedInfinities.Clear();
        List<IInfinity> toRemove = [];
        foreach (Infinity<TConsumable> infinity in _infinities) config.Infinities.GetOrAdd(new(infinity), new Toggle<Dictionary<string, object>>(true));
        foreach ((InfinityDefinition key, Toggle<Dictionary<string, object>> value) in config.Infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !_infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            Configs.Infinities.Instance.LoadInfinityConfig(infinity, value);
            _orderedInfinities.Add(infinity);
        }
        foreach (var infinity in toRemove) config.Infinities.Remove(new(infinity));
    }

    public ReadOnlyCollection<Infinity<TConsumable>> Infinities => _orderedInfinities.AsReadOnly();
    private readonly List<Infinity<TConsumable>> _orderedInfinities = [];
    private readonly HashSet<Infinity<TConsumable>> _infinities = [];
}

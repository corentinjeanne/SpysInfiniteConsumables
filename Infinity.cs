using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Configs;
using SpikysLib;
using SpikysLib.Collections;
using SpikysLib.Configs;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace SPIC;

public interface IInfinity : ILocalizedModType, ILoadable, IComponent {
    void RegisterComponent(IComponent component);
    ReadOnlyCollection<IComponent> Components { get; }

    bool DefaultEnabled { get; }
    Color DefaultColor { get; }

    LocalizedText Label { get; }
    LocalizedText DisplayName { get; }
    LocalizedText Tooltip { get; }
}

public abstract class Infinity<TConsumable> : ModType, IInfinity {
    protected override void InitTemplateInstance() => ConfigHelper.SetInstance(this);

    public override void Load() {
        foreach (IComponent component in GetComponents()) RegisterComponent(component);
        if (!Components.Contains(this)) RegisterComponent(this);
    }

    void IComponent.Load(IInfinity infinity) {
        if (LoaderUtils.HasOverride(this, i => i.GetId)) Endpoints.GetId(this).Register(GetId);
        if (LoaderUtils.HasOverride(this, i => i.ToConsumable)) Endpoints.ToConsumable(this).Register(ToConsumable);
        if (LoaderUtils.HasOverride(this, i => i.CountConsumables)) Endpoints.CountConsumables(this).Register(CountConsumables);
        if (LoaderUtils.HasOverride(this, i => i.GetRequirement)) Endpoints.GetRequirement(this).Register(GetRequirement);
    }

    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityManager.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), () => $$"""{${{this.GetLocalizationKey("DisplayName")}}}""");
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public sealed override void SetupContent() {
        foreach (IComponent component in _components) component.SetStaticDefaults();
        SetStaticDefaults();
    }

    public override void Unload() {
        foreach (IComponent component in _components) component.Unload();
        _components.Clear();
        ConfigHelper.SetInstance(this, true);
    }
    
    void IComponent.Unload() { }

    protected virtual Optional<int> GetId(TConsumable consumable) => throw new NotImplementedException();
    protected virtual Optional<TConsumable> ToConsumable(int id) => throw new NotImplementedException();
    protected virtual Optional<long> CountConsumables(PlayerConsumable<TConsumable> args) => throw new NotImplementedException();
    protected virtual Optional<Requirement> GetRequirement(TConsumable consumable) => throw new NotImplementedException();
    
    protected virtual IEnumerable<IComponent> GetComponents()
        => GetType().GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(f => f.FieldType.ImplementsInterface(typeof(IComponent), out _))
            .Select(f => (IComponent)f.GetValue(null)!);
    public void RegisterComponent(IComponent component) {
        _components.Add(component);
        component.Load(this);
    }
    public ReadOnlyCollection<IComponent> Components => _components.AsReadOnly();
    private readonly List<IComponent> _components = [];

    public virtual bool DefaultEnabled => true;
    public virtual Color DefaultColor => Color.White;

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText Label => this.GetLocalization("Label");
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName");
    public virtual LocalizedText Tooltip => this.GetLocalization("Tooltip");

    IInfinity IComponent.Infinity => this;
}

public abstract class Infinity<TConsumable, TCategory> : Infinity<TConsumable> where TCategory : struct, Enum {
    public override void Load() {
        base.Load();
        if (LoaderUtils.HasOverride(this, i => i.GetCategory)) Endpoints.GetCategory(this).Register(GetCategory);
    }

    protected virtual Optional<TCategory> GetCategory(TConsumable consumable) => throw new NotImplementedException();
    protected abstract Optional<Requirement> GetRequirement(TCategory category);

    protected sealed override Optional<Requirement> GetRequirement(TConsumable consumable) => GetRequirement(InfinityManager.GetCategory(consumable, this));
}

public abstract class GroupInfinity<TConsumable> : Infinity<TConsumable>, IConfigurableComponents<GroupConfig>, IClientConfigurableComponents<ClientGroupConfig> {
    protected sealed override Optional<Requirement> GetRequirement(TConsumable consumable) {
        long count = 0;
        float multiplier = float.MaxValue;
        foreach (Infinity<TConsumable> infinity in _orderedInfinities) {
            Requirement requirement = InfinityManager.GetRequirement(consumable, infinity);
            if (requirement.IsNone) continue;
            count = Math.Max(count, requirement.Count);
            multiplier = Math.Min(multiplier, requirement.Multiplier);
        }
        return new Requirement(count, multiplier);
    }

    internal void RegisterChild(Infinity<TConsumable> infinity) {
        _infinities.Add(infinity);
        _orderedInfinities.Add(infinity);
    }

    void IConfigurableComponents<GroupConfig>.OnLoaded(GroupConfig config) {
        _orderedInfinities.Clear();
        List<IInfinity> toRemove = [];
        foreach (Infinity<TConsumable> infinity in _infinities) config.Infinities.GetOrAdd(new(infinity), InfinitySettings.DefaultConfig(infinity));
        foreach ((InfinityDefinition key, Toggle<Dictionary<string, object>> value) in config.Infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !_infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            InfinitySettings.Instance.LoadInfinityConfig(infinity, value);
            _orderedInfinities.Add(infinity);
        }
        foreach (var infinity in toRemove) config.Infinities.Remove(new(infinity));
    }
    void IClientConfigurableComponents<ClientGroupConfig>.OnLoaded(ClientGroupConfig config) {
        List<IInfinity> toRemove = [];
        foreach (Infinity<TConsumable> infinity in _infinities) config.Infinities.GetOrAdd(new(infinity), InfinityDisplays.DefaultConfig(infinity));
        foreach ((InfinityDefinition key, NestedValue<Color, Dictionary<string, object>> value) in config.Infinities) {
            IInfinity? i = key.Entity;
            if (i is null) continue;
            if (i is not Infinity<TConsumable> infinity || !_infinities.Contains(infinity)) {
                toRemove.Add(i);
                continue;
            }
            InfinityDisplays.Instance.LoadInfinityConfig(infinity, value);
        }
        foreach (var infinity in toRemove) config.Infinities.Remove(new(infinity));
    }

    public ReadOnlyCollection<Infinity<TConsumable>> Infinities => _orderedInfinities.AsReadOnly();
    private readonly List<Infinity<TConsumable>> _orderedInfinities = [];
    private readonly HashSet<Infinity<TConsumable>> _infinities = [];
}

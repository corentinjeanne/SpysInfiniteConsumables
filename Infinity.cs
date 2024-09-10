using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using SpikysLib;
using SpikysLib.Configs;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace SPIC;

public interface IInfinity : ILocalizedModType, ILoadable, IComponent {
    void BindComponent(IComponent component);
    ReadOnlyCollection<IComponent> Components { get; }
    TComponent GetComponent<TComponent>() where TComponent : IComponent;
    bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : IComponent;

    bool DefaultEnabled { get; }
    Color DefaultColor { get; }

    LocalizedText Label { get; }
    LocalizedText DisplayName { get; }
    LocalizedText Tooltip { get; }
}

public abstract class Infinity<TConsumable> : ModType, IInfinity {
    protected override void InitTemplateInstance() => ConfigHelper.SetInstance(this);

    public override void Load() {
        foreach (IComponent component in GetComponents()) BindComponent(component);
        if (!Components.Contains(this)) BindComponent(this);
    }

    void IComponent.Bind(IInfinity infinity) {
        if (LoaderUtils.HasOverride(this, i => i.GetId)) Endpoints.GetId(this).AddProvider(GetId);
        if (LoaderUtils.HasOverride(this, i => i.ToConsumable)) Endpoints.ToConsumable(this).AddProvider(ToConsumable);
        if (LoaderUtils.HasOverride(this, i => i.CountConsumables)) Endpoints.CountConsumables(this).AddProvider(CountConsumables);
        if (LoaderUtils.HasOverride(this, i => i.GetRequirement)) Endpoints.GetRequirement(this).AddProvider(GetRequirement);
        Endpoints.IdInfinity(this).AddProvider(_ => this);
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
        foreach (IComponent component in _components) component.Unbind();
        _components.Clear();
        ConfigHelper.SetInstance(this, true);
    }
    
    void IComponent.Unbind() { }

    protected virtual Optional<int> GetId(TConsumable consumable) => throw new NotImplementedException();
    protected virtual Optional<TConsumable> ToConsumable(int id) => throw new NotImplementedException();
    protected virtual Optional<long> CountConsumables(PlayerConsumable<TConsumable> args) => throw new NotImplementedException();
    protected virtual Optional<Requirement> GetRequirement(TConsumable consumable) => throw new NotImplementedException();
    
    protected virtual IEnumerable<IComponent> GetComponents()
        => GetType().GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(f => f.FieldType.ImplementsInterface(typeof(IComponent), out _))
            .Select(f => (IComponent)f.GetValue(null)!);
    public void BindComponent(IComponent component) {
        _components.Add(component);
        component.Bind(this);
    }

    public TComponent GetComponent<TComponent>() where TComponent: IComponent => TryGetComponent(out TComponent? component) ? component : throw new NullReferenceException("Component not found");
    public bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : IComponent {
        foreach(IComponent c in _components) {
            if (c is not TComponent comp) continue;
            component = comp;
            return true;
        }
        component = default;
        return false;
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
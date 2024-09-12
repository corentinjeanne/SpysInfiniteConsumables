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
using SpikysLib.Localization;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace SPIC;

public interface IInfinity : ILocalizedModType, ILoadable, IComponent {
    void BindComponent(IComponent component);
    ReadOnlyCollection<IComponent> Components { get; }
    TComponent GetComponent<TComponent>() where TComponent : IComponent;
    bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : IComponent;

    IEnumerable<InfinityValue> GetDisplayedInfinities(Item item);

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

    void IComponent.Bind(IInfinity infinity) => Bind();
    public virtual void Bind() {
        if (LoaderUtils.HasOverride(this, i => i.GetId)) Endpoints.GetId(this).Add(GetId);
        if (LoaderUtils.HasOverride<Infinity<TConsumable>, ToConsumableFn>(this, i => i.ToConsumable)) Endpoints.ToConsumable(this).Add(ToConsumable);
        if (LoaderUtils.HasOverride<Infinity<TConsumable>, ItemToConsumableFn>(this, i => i.ToConsumable)) Endpoints.ItemToConsumable(this).Add(ToConsumable);
        if (LoaderUtils.HasOverride(this, i => i.CountConsumables)) Endpoints.CountConsumables(this).Providers.Add(CountConsumables);
        if (LoaderUtils.HasOverride(this, i => i.GetRequirement)) Endpoints.GetRequirement(this).Providers.Add(GetRequirement);
        if (LoaderUtils.HasOverride(this, i => i.ModifyRequirement)) Endpoints.GetRequirement(this).Modifiers.Add(ModifyRequirement);
        if (LoaderUtils.HasOverride(this, i => i.GetVisibility)) Endpoints.GetVisibility(this).Add(GetVisibility);
        if (LoaderUtils.HasOverride(this, i => i.ModifyDisplayedConsumables)) Endpoints.ModifyDisplayedConsumables(this).Add(ModifyDisplayedConsumables);
        if (LoaderUtils.HasOverride(this, i => i.ModifyDisplayedInfinity)) Endpoints.ModifyDisplayedInfinity(this).Add(ModifyDisplayedInfinity);
    }

    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityManager.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), () => $$"""{${{this.GetLocalizationKey("DisplayName")}}}""");
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
        foreach (IComponent component in _components) {
            if (component is IConfigurableComponents configurable) LanguageHelper.RegisterLocalizationKeysForMembers(configurable.ConfigType);
            if (component is IClientConfigurableComponents clientConfigurable) LanguageHelper.RegisterLocalizationKeysForMembers(clientConfigurable.ConfigType);
        }
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
    
    public virtual void Unbind() { }

    protected virtual Optional<int> GetId(TConsumable consumable) => default;
    protected virtual Optional<TConsumable> ToConsumable(int id) => default;
    protected virtual Optional<TConsumable> ToConsumable(Item item) => default;
    protected virtual Optional<long> CountConsumables(PlayerConsumable<TConsumable> args) => default;
    protected virtual Optional<Requirement> GetRequirement(TConsumable consumable) => default;
    protected virtual void ModifyRequirement(TConsumable consumable, ref Requirement requirement) { }
    protected virtual Optional<InfinityVisibility> GetVisibility(Item item) => default;
    protected virtual void ModifyDisplayedConsumables(Item item, ref List<TConsumable> consumables) { }
    protected virtual void ModifyDisplayedInfinity(ItemConsumable<TConsumable> args, ref InfinityValue value) { }

    private delegate Optional<TConsumable> ToConsumableFn(int consumable);
    private delegate Optional<TConsumable> ItemToConsumableFn(Item item);

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

    public IEnumerable<InfinityValue> GetDisplayedInfinities(Item item) {
        foreach(TConsumable consumable in InfinityManager.GetDisplayedConsumables(item, this)) {
            InfinityValue infinity = new(InfinityManager.GetId(consumable, this), Main.LocalPlayer.CountConsumables(consumable, this), InfinityManager.GetRequirement(consumable, this), Main.LocalPlayer.GetInfinity(consumable, this));
            InfinityManager.ModifyDisplayedInfinity(item, consumable, ref infinity, this);
            yield return infinity;
        }
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
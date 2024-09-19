using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using SPIC.Configs;
using SpikysLib.Configs;
using SpikysLib.Localization;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC;

public interface IInfinity : ILocalizedModType, ILoadable {
    bool Enabled { get; set; }
    IConsumableInfinity Consumable { get; }
    bool IsConsumable { get; }

    IEnumerable<(InfinityVisibility visibility, InfinityValue value)> GetDisplayedInfinities(Item item);

    Color Color { get; set; }
    LocalizedText Label { get; }
    LocalizedText DisplayName { get; }
    LocalizedText Tooltip { get; }

    bool DefaultEnabled { get; }
    Color DefaultColor { get; }
}

public abstract class Infinity<TConsumable> : ModType, IInfinity {
    public bool Enabled { get => _enabled && (IsConsumable || Consumable.Enabled); set => _enabled = value; }
    private bool _enabled;
    public virtual bool DefaultEnabled => true;

    public abstract ConsumableInfinity<TConsumable> Consumable { get; }
    IConsumableInfinity IInfinity.Consumable => Consumable;
    public bool IsConsumable => Consumable == this;

    public Requirement GetRequirement(TConsumable consumable) {
        Optional<Requirement> custom = Customs.GetRequirement(consumable);
        Requirement requirement = custom.HasValue ? custom.Value : GetRequirementInner(consumable);
        ModifyRequirement(consumable, ref requirement);
        return requirement;
    }
    protected abstract Requirement GetRequirementInner(TConsumable consumable);
    protected virtual void ModifyRequirement(TConsumable consumable, ref Requirement requirement) { }

    public IEnumerable<(InfinityVisibility visibility, InfinityValue value)> GetDisplayedInfinities(Item item) {
        List<TConsumable> consumables = [Consumable.ToConsumable(item)];
        ModifyDisplayedConsumables(item, ref consumables);
        foreach(TConsumable consumable in consumables) {
            InfinityVisibility visibility = IsConsumable || InfinityManager.UsedInfinities(consumable, Consumable).Contains(this) ? InfinityVisibility.Visible : InfinityVisibility.Hidden;
            InfinityValue infinity = new(Consumable.GetId(consumable), Main.LocalPlayer.CountConsumables(consumable, Consumable), InfinityManager.GetRequirement(consumable, this), Main.LocalPlayer.GetInfinity(consumable, this));
            ModifyDisplayedInfinity(item, consumable, ref visibility, ref infinity);
            if (infinity.Requirement.IsNone) continue;
            yield return (visibility, infinity);
        }
    }
    protected virtual void ModifyDisplayedConsumables(Item item, ref List<TConsumable> consumables) { }
    protected virtual void ModifyDisplayedInfinity(Item item, TConsumable consumable, ref InfinityVisibility visibility, ref InfinityValue value) { }

    public ICustoms<TConsumable> Customs { get; private set; } = null!;

    public Color Color { get; set; }
    public virtual Color DefaultColor => Color.White;

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText Label => this.GetLocalization("Label");
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName");
    public virtual LocalizedText Tooltip => this.GetLocalization("Tooltip");


    protected override void InitTemplateInstance() => ConfigHelper.SetInstance(this);

    public override void Load() {
        AddConfig(this, "config");
        Customs = CreateCustoms();
        AddConfig(Customs, "customs");
    }

    protected virtual ICustoms<TConsumable> CreateCustoms() => new Customs<TConsumable>(this);

    protected void AddConfig(object obj, string key) {
        if (obj is IConfigProvider config) {
            InfinitySettings.AddConfig(this, key, config);
            LanguageHelper.RegisterLocalizationKeysForMembers(config.ConfigType);
        }
        if (obj is IClientConfigProvider clientConfig) {
            InfinityDisplays.AddConfig(this, key, clientConfig);
            LanguageHelper.RegisterLocalizationKeysForMembers(clientConfig.ConfigType);
        }
    }

    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityManager.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), () => $$"""{${{this.GetLocalizationKey("DisplayName")}}}""");
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public override void SetStaticDefaults() {
        if (!IsConsumable) Consumable.AddInfinity(this);
    }

    public sealed override void SetupContent() => SetStaticDefaults();

    public override void Unload() => ConfigHelper.SetInstance(this, true);
}

public abstract class Infinity<TConsumable, TCategory> : Infinity<TConsumable> where TCategory: struct, Enum {
    public TCategory GetCategory(TConsumable consumable) {
        TCategory category;
        Optional<TCategory> custom = Customs.GetCategory(consumable);
        category = custom.HasValue ? custom.Value : GetCategoryInner(consumable);
        return category;
    }
    protected abstract TCategory GetCategoryInner(TConsumable consumable);

    public abstract Requirement GetRequirement(TCategory category);
    protected override Requirement GetRequirementInner(TConsumable consumable) {
        HashSet<long> categories = [];
        Requirement requirement = GetRequirement(InfinityManager.GetCategory(consumable, this));
        while (requirement.Count < 0 && categories.Add(-requirement.Count)) {
            var category = -requirement.Count;
            requirement = GetRequirement(Unsafe.As<long, TCategory>(ref category));
        }
        return requirement;
    }

    protected sealed override ICustoms<TConsumable> CreateCustoms() => new Customs<TConsumable, TCategory>(this);
    public new Customs<TConsumable, TCategory> Customs => (Customs<TConsumable, TCategory>)base.Customs;
    public bool SaveDetectedCategory(TConsumable consumable, TCategory category) {
        Dictionary<ItemDefinition, Count<TCategory>> customs = Customs.Config.customs;
        if (!InfinitySettings.Instance.DetectMissingCategories || !customs.TryAdd(Consumable.ToDefinition(consumable), new Count<TCategory>(category)))
            return false;

        InfinityManager.ClearCache();
        return true;
    }
}
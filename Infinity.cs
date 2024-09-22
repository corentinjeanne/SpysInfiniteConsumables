using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using SPIC.Configs;
using SpikysLib.Configs;
using SpikysLib.Localization;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC;

public readonly struct InfinityDefaults() {
    public readonly bool Enabled { get; init; } = true;
    public readonly Color Color { get; init; } = Color.White;
}

public interface IInfinity : ILocalizedModType, ILoadable {
    bool Enabled { get; set; }
    IConsumableInfinity Consumable { get; }
    bool IsConsumable { get; }

    IEnumerable<(InfinityVisibility visibility, InfinityValue value)> GetDisplayedInfinities(Item item, bool visible);

    Color Color { get; set; }
    LocalizedText Label { get; }
    LocalizedText DisplayName { get; }
    LocalizedText Tooltip { get; }

    InfinityDefaults Defaults { get; }

    long GetRequirement(int consumable);
    long GetInfinity(int consumable, long count);
}

public abstract class Infinity<TConsumable> : ModType, IInfinity {
    public bool Enabled { get => _enabled && (IsConsumable || Consumable.Enabled); set => _enabled = value; }
    private bool _enabled;

    public abstract ConsumableInfinity<TConsumable> Consumable { get; }
    IConsumableInfinity IInfinity.Consumable => Consumable;
    public bool IsConsumable => Consumable == this;

    public long GetRequirement(TConsumable consumable) {
        InfinityManager.GetDebugInfo(Consumable.GetId(consumable), this).Clear();
        var custom = Customs.GetRequirement(consumable);
        if (custom.HasValue) InfinityManager.AddDebugInfo(Consumable.GetId(consumable), Language.GetText($"{Localization.Keys.CommonItemTooltips}.Custom"), this);
        long requirement = custom ?? GetRequirementInner(consumable);
        ModifyRequirement(consumable, ref requirement);
        return requirement;
    }
    protected abstract long GetRequirementInner(TConsumable consumable);
    protected virtual void ModifyRequirement(TConsumable consumable, ref long requirement) { }

    public long GetInfinity(TConsumable consumable, long count) {
        long infinity = GetInfinityInner(consumable, count);
        ModifyInfinity(consumable, ref infinity);
        return infinity;
    }
    protected virtual long GetInfinityInner(TConsumable consumable, long count) {
        long requirement = InfinityManager.GetRequirement(consumable, this);
        return requirement > 0 && count >= requirement ? count : 0;
    }
    protected virtual void ModifyInfinity(TConsumable consumable, ref long infinity) { }

    public IEnumerable<(InfinityVisibility visibility, InfinityValue value)> GetDisplayedInfinities(Item item, bool visible) {
        List<TConsumable> consumables = [Consumable.ToConsumable(item)];
        if (Configs.InfinityDisplay.Instance.alternateDisplays) ModifyDisplayedConsumables(item, ref consumables);
        for (int i = 0; i < consumables.Count; i++) {
            TConsumable consumable = consumables[i];
            InfinityVisibility visibility = i != 0 || InfinityManager.IsUsed(consumable, this) ? InfinityVisibility.Visible : InfinityVisibility.Hidden;
            long count = Main.LocalPlayer.CountConsumables(consumable, Consumable);
            InfinityValue infinity = new(
                Consumable.GetId(consumable),
                InfinityManager.GetRequirement(consumable, this),
                visible ? count : 0,
                visible ? InfinityManager.GetInfinity(consumable, count, this) : 0
            );
            ModifyDisplayedInfinity(item, consumable, ref visibility, ref infinity);
            if (infinity.Requirement > 0) yield return (visibility, infinity);
        }
    }
    protected virtual void ModifyDisplayedConsumables(Item item, ref List<TConsumable> consumables) { }
    protected virtual void ModifyDisplayedInfinity(Item item, TConsumable consumable, ref InfinityVisibility visibility, ref InfinityValue value) { }

    public ICustoms<TConsumable> Customs { get; private set; } = null!;

    public virtual Color Color { get; set; }

    public virtual InfinityDefaults Defaults => new();

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
            Configs.InfinityDisplay.AddConfig(this, key, clientConfig);
            LanguageHelper.RegisterLocalizationKeysForMembers(clientConfig.ConfigType);
        }
    }

    protected sealed override void Register() {
        ModTypeLookup<Infinity<TConsumable>>.Register(this);
        InfinityLoader.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), () => $$"""{${{this.GetLocalizationKey("DisplayName")}}}""");
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public sealed override void SetupContent() {
        if (!IsConsumable) Consumable.AddInfinity(this);
        SetStaticDefaults();
    }

    public override void Unload() => ConfigHelper.SetInstance(this, true);

    long IInfinity.GetRequirement(int consumable) => InfinityManager.GetRequirement(Consumable.ToConsumable(consumable), this);
    long IInfinity.GetInfinity(int consumable, long count) => InfinityManager.GetInfinity(Consumable.ToConsumable(consumable), count, this);
}

public interface IInfinity<TCategory> : IInfinity where TCategory : struct, Enum {
    TCategory GetCategory(int consumable);
}

public abstract class Infinity<TConsumable, TCategory> : Infinity<TConsumable>, IInfinity<TCategory> where TCategory: struct, Enum {
    public TCategory GetCategory(TConsumable consumable)  => Customs.GetCategory(consumable) ?? GetCategoryInner(consumable);
    protected abstract TCategory GetCategoryInner(TConsumable consumable);

    public abstract long GetRequirement(TCategory category);
    protected override long GetRequirementInner(TConsumable consumable) {
        HashSet<TCategory> categories = [];
        TCategory category = InfinityManager.GetCategory(consumable, this);
        while (categories.Add(category)) {
            InfinityManager.AddDebugInfo(Consumable.GetId(consumable), Mod.GetLocalization($"Configs.{typeof(TCategory).Name}.{category}.Label"), this);
            long requirement = GetRequirement(category);
            if (requirement >= 0) return requirement;
            int c = (int)-requirement;
            category = Unsafe.As<int, TCategory>(ref c);
        }
        return 0;
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

    TCategory IInfinity<TCategory>.GetCategory(int consumable) => InfinityManager.GetCategory(Consumable.ToConsumable(consumable), this);
}
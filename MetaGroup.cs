using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using SPIC.Configs;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public enum GroupState {
    Disabled,
    Used,
    Unused
}

public interface IMetaGroup : ILocalizedModType, ILoadable {
    void ClearInfinities();
    void ClearInfinity(Item item);

    string CountToString(Item item, long owned, long infinity, InfinityDisplay.CountStyle style);

    FullInfinity GetEffectiveInfinity(Player player, int type, IModGroup group);

    IEnumerable<(IModGroup group, int type, bool used)> GetDisplayedGroups(Item item);

    IEnumerable<IModGroup> Groups { get; }
    
    LocalizedText DisplayName { get; }
    MetaConfig Config { get; internal set; }


    internal void SortGroups();
    internal void LogCacheStats();
}

public abstract class MetaGroup<TMetaGroup, TConsumable> : ModType, IMetaGroup where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {

    public MetaGroup() {
        _infinities = new(GetType, consumable => GetMetaInfinity(Main.LocalPlayer, consumable));
    }

    internal void Add(ModGroup<TMetaGroup, TConsumable> group) {
        _groups.Add(group, group.DefaultsToOn);
        group.MetaGroup = (TMetaGroup)this;
        group.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, group);
    }

    public override void Unload() {
        foreach (ModGroup<TMetaGroup, TConsumable> group in _groups.Keys) {
            group.MetaGroup = null!;
            group.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, null);
        }
        _groups.Clear();
    }
    protected sealed override void Register() {
        ModTypeLookup<MetaGroup<TMetaGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() {
        SetStaticDefaults();
    }

    public TCategory GetCategory<TCategory>(TConsumable consumable, ModGroup<TMetaGroup, TConsumable, TCategory> group) where TCategory : Enum {
        if(_infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return ((FullRequirement<TCategory>)metaInfinity[group].FullRequirement).Category;
        if (CategoryDetection.Instance.HasDetectedCategory(consumable, group, out TCategory? category)) return category;
        return group.GetCategory(consumable);
    }
    public Requirement GetRequirement(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity[group].Requirement;
        Requirement requirement = group.GetRequirement(consumable);
        if (GroupSettings.Instance.HasCustomCount(consumable, group, out Count? custom)) requirement = new(custom, requirement.Multiplier);
        long maxStack = MaxStack(consumable);
        if(maxStack != 0 && requirement.Count > maxStack) requirement = new(maxStack, requirement.Multiplier);
        return requirement;
    }
    public IFullRequirement GetFullRequirement(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity[group].FullRequirement;
        if (GroupSettings.Instance.HasCustomCount(consumable, group, out _)) return new CustomRequirement(GetRequirement(consumable, group));
        return group.GetFullRequirement(consumable);
    }

    public long GetInfinity(Player player, TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity[group].Infinity;
        return GetRequirement(consumable, group).Infinity(CountConsumables(player, consumable));
    }

    public FullInfinity GetFullInfinity(Player player, int type, IModGroup group) => _infinities.TryGet(type, out MetaInfinity? metaInfinity) ?
        metaInfinity[group] : GetFullInfinity(player, FromType(type), (ModGroup<TMetaGroup, TConsumable>)group);
    public FullInfinity GetFullInfinity(Player player, TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity[group];
        IFullRequirement fullRequirement = GetFullRequirement(consumable, group);
        long count = CountConsumables(player, consumable);
        long infinity = fullRequirement.Requirement.Infinity(count);
        return new(fullRequirement, count, infinity);
    }

    public Requirement GetMixedRequirement(TConsumable consumable) => _infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity) ?
        metaInfinity.Mixed.Requirement : GetMixedFullInfinity(Main.LocalPlayer, consumable).Requirement;
    public long GetMixedInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity.Mixed.Infinity;
        return GetMixedFullInfinity(player, consumable).Infinity;
    }

    public FullInfinity GetMixedFullInfinity(Player player, int type) => _infinities.TryGet(type, out MetaInfinity? metaInfinity) ?
        metaInfinity.Mixed : GetMixedFullInfinity(player, FromType(type));
    public FullInfinity GetMixedFullInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity.Mixed;
        return GetMetaInfinity(player, consumable).Mixed;
    }

    public FullInfinity GetEffectiveInfinity(Player player, int type, IModGroup group) {
        if (!IsEnabled(group)) return default;
        return IsUsed(type, group) ? GetFullInfinity(player, type, group) : GetMixedFullInfinity(player, type);
    }
    public FullInfinity GetEffectiveInfinity(Player player, TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (!IsEnabled(group)) return default;
        return IsUsed(consumable, group) ? GetFullInfinity(player, consumable, group) : GetMixedFullInfinity(player, consumable);
    }

    public MetaInfinity GetMetaInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity)) return metaInfinity;
        metaInfinity = new();
        foreach ((ModGroup<TMetaGroup, TConsumable> group, bool enabled) in _groups.Items<ModGroup<TMetaGroup, TConsumable>, bool>()) {
            IFullRequirement fullRequirement = GetFullRequirement(consumable, group);
            long count = CountConsumables(player, consumable);
            long infinity = fullRequirement.Requirement.Infinity(count);

            bool used = enabled && !fullRequirement.Requirement.IsNone && (Config.MaxConsumableTypes == 0 || metaInfinity.UsedGroups.Count < Config.MaxConsumableTypes);
            metaInfinity.AddGroup(group, new(fullRequirement, count, infinity), used);
        }
        bool hasCustom = GroupSettings.Instance.HasCustomGlobalRequirement(consumable, this, out Requirement custom);
        metaInfinity.AddMixed(hasCustom ? custom : null);
        return metaInfinity;
    }

    public IEnumerable<(IModGroup group, int type, bool used)> GetDisplayedGroups(Item item) {
        TConsumable consumable = ToConsumable(item);
        bool hasCustom = GroupSettings.Instance.HasCustomGlobalRequirement(consumable, this, out _);

        foreach ((ModGroup<TMetaGroup, TConsumable> group, bool enabled) in _groups.Items<ModGroup<TMetaGroup, TConsumable>, bool>()) {
            if (!enabled) continue;
            TConsumable displayed = group.DisplayedValue(consumable); // TODO Ammos with non used groups for weapons
            if (!hasCustom || GetRequirement(consumable, group).IsNone) {
                yield return (group, GetType(displayed), !consumable.Equals(displayed) || IsUsed(displayed, group));
                continue;
            }
            yield return (group, GetType(displayed), true);
            hasCustom = false;
        }
    }
   
    public void ClearInfinities() => _infinities.Clear();
    public void ClearInfinity(Item item) => _infinities.Clear(ToConsumable(item));

    public bool IsEnabled(IModGroup group) => (bool)_groups[group]!;

    public bool IsUsed(int type, IModGroup group) => _infinities.TryGet(type, out MetaInfinity? metaInfinity) ?
        metaInfinity.UsedGroups.Contains(group) : IsUsed(FromType(type), group);
    public bool IsUsed(TConsumable consumable, IModGroup group) => (_infinities.TryGetOrCache(consumable, out MetaInfinity? metaInfinity) ?
        metaInfinity.UsedGroups : GetMetaInfinity(Main.LocalPlayer, consumable).UsedGroups).Contains(group);

    public abstract TConsumable ToConsumable(Item item);
    public abstract Item ToItem(TConsumable consumable);


    public abstract int GetType(TConsumable consumable);
    public abstract TConsumable FromType(int type);

    public abstract long CountConsumables(Player player, TConsumable consumable);
    public virtual long MaxStack(TConsumable consumable) => 0;


    public abstract string CountToString(TConsumable consumable, long count, InfinityDisplay.CountStyle style, bool rawValue = false);
    public virtual string CountToString(Item item, long owned, long infinity, InfinityDisplay.CountStyle style) {
        TConsumable consumable = ToConsumable(item);
        return owned == 0 ? CountToString(consumable, infinity, style) : $"{CountToString(consumable, owned, style, true)}/{CountToString(consumable, infinity, style)}";
    }

    void IMetaGroup.SortGroups() {
        List<ModGroup<TMetaGroup, TConsumable>> groups = new(Groups);
        _groups.Clear();
        foreach ((var def, var enabled) in Config.EnabledGroups.Items<ModGroupDefinition, bool>()) {
            int i = groups.FindIndex(g => g.Mod.Name == def.Mod && g.Name == def.Name);
            _groups.Add(groups[i], enabled);
        }
    }

    void IMetaGroup.LogCacheStats(){
        Mod.Logger.Debug($"{Name}:{_infinities}");
        _infinities.ResetStats();
    }

    public string LocalizationCategory => "Groups";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);


    IEnumerable<IModGroup> IMetaGroup.Groups => Groups;
    public IEnumerable<ModGroup<TMetaGroup, TConsumable>> Groups { get {
        foreach (ModGroup<TMetaGroup, TConsumable> group in _groups.Keys) yield return group;
    } }
    public MetaConfig Config { get; set; } = null!;


    private readonly OrderedDictionary _groups = new();

    private readonly Cache<TConsumable, int, MetaInfinity> _infinities;
}
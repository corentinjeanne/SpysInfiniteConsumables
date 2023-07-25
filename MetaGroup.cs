using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using SPIC.Configs;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public interface IMetaGroup : ILocalizedModType, ILoadable {
    void ClearInfinities();
    void ClearInfinity(Item item);
    string CountToString(Item item, long owned, long infinity, InfinityDisplay.CountStyle style);
    internal void SortGroups();

    bool IsUsed(int type, IModGroup group);
    FullInfinity GetFullInfinity(Player player, int type, IModGroup group);
    FullInfinity GetMixedFullInfinity(Player player, int type);

    IEnumerable<(IModGroup group, int type, bool used)> GetDisplayedGroups(Item item);

    LocalizedText DisplayName { get; }
    MetaConfig Config { get; internal set; }

    IEnumerable<IModGroup> Groups { get; }

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
        if(_infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return ((FullRequirement<TCategory>)metaInfinity[group].FullRequirement).Category;
        if (CategoryDetection.Instance.HasDetectedCategory(consumable, group, out TCategory? category)) return category;
        return group.GetCategory(consumable);
    }
    public Requirement GetRequirement(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity[group].Requirement;
        Requirement requirement = group.GetRequirement(consumable);
        if (GroupSettings.Instance.HasCustomCount(consumable, group, out Count? custom)) requirement = new(custom, requirement.Multiplier);
        long maxStack = MaxStack(consumable);
        if(maxStack != 0 && requirement.Count > maxStack) requirement = new(maxStack, requirement.Multiplier);
        return requirement;
    }

    public IFullRequirement GetFullRequirement(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity[group].FullRequirement;
        if (GroupSettings.Instance.HasCustomCount(consumable, group, out _)) return new CustomRequirement(GetRequirement(consumable, group));
        return group.GetFullRequirement(consumable);
    }

    public long GetInfinity(TConsumable consumable, long count, ModGroup<TMetaGroup, TConsumable> group) => GetRequirement(consumable, group).Infinity(count);

    public long GetInfinity(Player player, TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity[group].Infinity;
        return  GetInfinity(consumable, CountConsumables(player, consumable), group);
    }

    public long GetMixedInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity.Mixed.Infinity;
        return GetMetaInfinity(player, consumable).Mixed.Infinity;
    }

    public FullInfinity GetFullInfinity(Player player, int consumable, IModGroup group) {
        ModGroup<TMetaGroup, TConsumable> realGroup = (ModGroup<TMetaGroup, TConsumable>)group;
        if (_infinities.TryGet(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity[realGroup];
        return GetFullInfinity(player, FromType(consumable), realGroup);
    }
    public FullInfinity GetFullInfinity(Player player, TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity[group];
        IFullRequirement fullRequirement = GetFullRequirement(consumable, group);
        long count = CountConsumables(player, consumable);
        long infinity = fullRequirement.Requirement.Infinity(count);
        return new(fullRequirement, count, infinity);
    }

    public FullInfinity GetMixedFullInfinity(Player player, int consumable) {
        if (_infinities.TryGet(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity.Mixed;
        return GetMixedFullInfinity(player, FromType(consumable));
    }
    public FullInfinity GetMixedFullInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity.Mixed;
        return GetMetaInfinity(player, consumable).Mixed;
    }

    public MetaInfinity<TMetaGroup, TConsumable> GetMetaInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity;
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
                yield return (group, GetType(displayed), !consumable!.Equals(displayed) || IsUsed(displayed, group));
                continue;
            }
            yield return (group, GetType(displayed), true);
            hasCustom = false;
        }
    }
   
    public void ClearInfinities() => _infinities.Clear();
    public void ClearInfinity(Item item) => _infinities.Clear(ToConsumable(item)); 

    public bool IsEnabled(ModGroup<TMetaGroup, TConsumable> group) => (bool)_groups[group]!;

    public bool IsUsed(int type, IModGroup group) => UsedGroups(type).Contains((ModGroup<TMetaGroup, TConsumable>)group);
    public bool IsUsed(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group) => UsedGroups(consumable).Contains(group);
    public HashSet<ModGroup<TMetaGroup, TConsumable>> UsedGroups(int type) {
        if (_infinities.TryGet(type, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity.UsedGroups;
        return UsedGroups(FromType(type));
    }
    public HashSet<ModGroup<TMetaGroup, TConsumable>> UsedGroups(TConsumable consumable) {
        if (_infinities.TryGetOrCache(consumable, out MetaInfinity<TMetaGroup, TConsumable>? metaInfinity)) return metaInfinity.UsedGroups;
        return GetMetaInfinity(Main.LocalPlayer, consumable).UsedGroups;
    }

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

    public void SortGroups() {
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

    private readonly Cache<TConsumable, int, MetaInfinity<TMetaGroup, TConsumable>> _infinities;
}
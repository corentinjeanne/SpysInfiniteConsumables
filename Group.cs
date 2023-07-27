using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using SPIC.Configs;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public enum InfinityState {
    Disabled,
    Used,
    Unused
}

public interface IGroup : ILocalizedModType, ILoadable {
    void ClearInfinities();
    void ClearInfinity(Item item);

    string CountToString(Item item, long owned, long infinity, InfinityDisplay.CountStyle style);

    FullInfinity GetEffectiveInfinity(Player player, int type, IInfinity infinity);

    IEnumerable<(IInfinity infinity, int type, bool used)> GetDisplayedInfinities(Item item);

    IEnumerable<IInfinity> Infinities { get; }
    
    LocalizedText DisplayName { get; }
    GroupConfig Config { get; internal set; }


    internal void SortInfinities();
    internal void LogCacheStats();
}

public abstract class Group<TGroup, TConsumable> : ModType, IGroup where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {

    public Group() {
        _cachedInfinities = new(GetType, consumable => GetConsumableInfinity(Main.LocalPlayer, consumable));
    }

    internal void Add(Infinity<TGroup, TConsumable> infinity) {
        _infinities.Add(infinity, infinity.DefaultsToOn);
        infinity.Group = (TGroup)this;
        infinity.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, infinity);
    }

    public override void Unload() {
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities.Keys) {
            infinity.Group = null!;
            infinity.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, null);
        }
        _infinities.Clear();
    }
    protected sealed override void Register() {
        ModTypeLookup<Group<TGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() {
        SetStaticDefaults();
    }

    public TCategory GetCategory<TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity) where TCategory : Enum {
        if(_cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return ((FullRequirement<TCategory>)groupInfinity[infinity].FullRequirement).Category;
        if (CategoryDetection.Instance.HasDetectedCategory(consumable, infinity, out TCategory? category)) return category;
        return infinity.GetCategory(consumable);
    }
    public Requirement GetRequirement(TConsumable consumable, Infinity<TGroup, TConsumable> infinity) {
        if (_cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity[infinity].Requirement;
        Requirement requirement = infinity.GetRequirement(consumable);
        if (InfinitySettings.Instance.HasCustomCount(consumable, infinity, out Count? custom)) requirement = new(custom, requirement.Multiplier);
        long maxStack = MaxStack(consumable);
        if(maxStack != 0 && requirement.Count > maxStack) requirement = new(maxStack, requirement.Multiplier);
        return requirement;
    }
    public IFullRequirement GetFullRequirement(TConsumable consumable, Infinity<TGroup, TConsumable> infinity) {
        if (_cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity[infinity].FullRequirement;
        if (InfinitySettings.Instance.HasCustomCount(consumable, infinity, out _)) return new CustomRequirement(GetRequirement(consumable, infinity));
        return infinity.GetFullRequirement(consumable);
    }

    public long GetInfinity(Player player, TConsumable consumable, Infinity<TGroup, TConsumable> infinity) {
        if (_cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity[infinity].Infinity;
        return GetRequirement(consumable, infinity).Infinity(CountConsumables(player, consumable));
    }

    public FullInfinity GetFullInfinity(Player player, int type, IInfinity infinity) => _cachedInfinities.TryGet(type, out GroupInfinity? groupInfinity) ?
        groupInfinity[infinity] : GetFullInfinity(player, FromType(type), (Infinity<TGroup, TConsumable>)infinity);
    public FullInfinity GetFullInfinity(Player player, TConsumable consumable, Infinity<TGroup, TConsumable> infinity) {
        if (player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity[infinity];
        IFullRequirement fullRequirement = GetFullRequirement(consumable, infinity);
        long count = CountConsumables(player, consumable);
        long infinityValue = fullRequirement.Requirement.Infinity(count);
        return new(fullRequirement, count, infinityValue);
    }

    public Requirement GetMixedRequirement(TConsumable consumable) => _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ?
        groupInfinity.Mixed.Requirement : GetMixedFullInfinity(Main.LocalPlayer, consumable).Requirement;
    public long GetMixedInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity.Mixed.Infinity;
        return GetMixedFullInfinity(player, consumable).Infinity;
    }

    public FullInfinity GetMixedFullInfinity(Player player, int type) => _cachedInfinities.TryGet(type, out GroupInfinity? groupInfinity) ?
        groupInfinity.Mixed : GetMixedFullInfinity(player, FromType(type));
    public FullInfinity GetMixedFullInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity.Mixed;
        return GetConsumableInfinity(player, consumable).Mixed;
    }

    public FullInfinity GetEffectiveInfinity(Player player, int type, IInfinity group) {
        if (!IsEnabled(group)) return default;
        return IsUsed(type, group) ? GetFullInfinity(player, type, group) : GetMixedFullInfinity(player, type);
    }
    public FullInfinity GetEffectiveInfinity(Player player, TConsumable consumable, Infinity<TGroup, TConsumable> group) {
        if (!IsEnabled(group)) return default;
        return IsUsed(consumable, group) ? GetFullInfinity(player, consumable, group) : GetMixedFullInfinity(player, consumable);
    }

    public GroupInfinity GetConsumableInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity;
        groupInfinity = new();
        foreach ((Infinity<TGroup, TConsumable> group, bool enabled) in _infinities.Items<Infinity<TGroup, TConsumable>, bool>()) {
            IFullRequirement fullRequirement = GetFullRequirement(consumable, group);
            long count = CountConsumables(player, consumable);
            long infinity = fullRequirement.Requirement.Infinity(count);

            bool used = enabled && !fullRequirement.Requirement.IsNone && (Config.MaxConsumableTypes == 0 || groupInfinity.UsedInfinities.Count < Config.MaxConsumableTypes);
            groupInfinity.Add(group, new(fullRequirement, count, infinity), used);
        }
        bool hasCustom = InfinitySettings.Instance.HasCustomGlobalRequirement(consumable, this, out Requirement custom);
        groupInfinity.AddMixed(hasCustom ? custom : null);
        return groupInfinity;
    }

    public IEnumerable<(IInfinity infinity, int type, bool used)> GetDisplayedInfinities(Item item) {
        TConsumable consumable = ToConsumable(item);
        bool hasCustom = InfinitySettings.Instance.HasCustomGlobalRequirement(consumable, this, out _);

        foreach ((Infinity<TGroup, TConsumable> infinity, bool enabled) in _infinities.Items<Infinity<TGroup, TConsumable>, bool>()) {
            if (!enabled) continue;
            TConsumable displayed = infinity.DisplayedValue(consumable); // TODO Ammos with non used groups for weapons
            if (!hasCustom || GetRequirement(consumable, infinity).IsNone) {
                yield return (infinity, GetType(displayed), !consumable.Equals(displayed) || IsUsed(displayed, infinity));
                continue;
            }
            yield return (infinity, GetType(displayed), true);
            hasCustom = false;
        }
    }
   
    public void ClearInfinities() => _cachedInfinities.Clear();
    public void ClearInfinity(Item item) => _cachedInfinities.Clear(ToConsumable(item));

    public bool IsEnabled(IInfinity infinity) => (bool)_infinities[infinity]!;

    public bool IsUsed(int type, IInfinity infinity) => _cachedInfinities.TryGet(type, out GroupInfinity? groupInfinity) ?
        groupInfinity.UsedInfinities.Contains(infinity) : IsUsed(FromType(type), infinity);
    public bool IsUsed(TConsumable consumable, IInfinity infinity) => (_cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ?
        groupInfinity.UsedInfinities : GetConsumableInfinity(Main.LocalPlayer, consumable).UsedInfinities).Contains(infinity);

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

    void IGroup.SortInfinities() {
        List<Infinity<TGroup, TConsumable>> infinities = new(Infinities);
        _infinities.Clear();
        foreach ((var def, var enabled) in Config.EnabledInfinities.Items<InfinityDefinition, bool>()) {
            int i = infinities.FindIndex(g => g.Mod.Name == def.Mod && g.Name == def.Name);
            _infinities.Add(infinities[i], enabled);
        }
    }

    void IGroup.LogCacheStats(){
        Mod.Logger.Debug($"{Name}:{_cachedInfinities}");
        _cachedInfinities.ResetStats();
    }

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);


    IEnumerable<IInfinity> IGroup.Infinities => Infinities;
    public IEnumerable<Infinity<TGroup, TConsumable>> Infinities { get {
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities.Keys) yield return infinity;
    } }
    public GroupConfig Config { get; set; } = null!;


    private readonly OrderedDictionary _infinities = new();

    private readonly Cache<TConsumable, int, GroupInfinity> _cachedInfinities;
}
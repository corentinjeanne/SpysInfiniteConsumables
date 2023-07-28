using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    GroupColors Colors { get; internal set; }

    IDictionary<IInfinity, IWrapper> InfinityConfigs { get; }


    internal void UpdateInfinities();
    internal void LogCacheStats();
}

public abstract class Group<TGroup, TConsumable> : ModType, IGroup where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {

    public Group() {
        _cachedInfinities = new(GetType, consumable => GetGroupInfinity(Main.LocalPlayer, consumable));
    }

    internal void Add(InfinityRoot<TGroup, TConsumable> infinity) {
        _infinities.Add(infinity);
        infinity.Group = (TGroup)this;
        infinity.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, infinity);
    }

    public Wrapper<T> AddConfig<T>(IInfinity infinity) where T : new() {
        Wrapper<T> wrapper = new();
        _infinityConfigs[infinity] = wrapper;
        return wrapper;
    }

    public override void Load() => Instance = (TGroup)this;

    public override void Unload() {
        foreach (InfinityRoot<TGroup, TConsumable> infinity in _infinities) {
            infinity.Group = null!;
            infinity.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, null);
        }
        _infinities.Clear();
        _infinityConfigs.Clear();
        Instance = null!;
    }
    protected sealed override void Register() {
        ModTypeLookup<Group<TGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() {
        SetStaticDefaults();
    }

    public Requirement GetRequirement(TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) => _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ? groupInfinity[infinity].Requirement : FullInfinity.WithRequirement(consumable, infinity).Requirement;
    public long GetInfinity(Player player, TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity) => GetFullInfinity(player, consumable, infinity).Infinity;
    public FullInfinity GetFullInfinity(Player player, int type, IInfinity infinity) => _cachedInfinities.TryGet(type, out GroupInfinity? groupInfinity) ? groupInfinity[infinity] : GetFullInfinity(player, FromType(type), (Infinity<TGroup, TConsumable>)infinity);
    public FullInfinity GetFullInfinity(Player player, TConsumable consumable, InfinityRoot<TGroup, TConsumable> infinity)
        => player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ? groupInfinity[infinity] : FullInfinity.WithInfinity(player, consumable, infinity);

    public Requirement GetMixedRequirement(TConsumable consumable) => _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ?
        groupInfinity.Mixed.Requirement : GetMixedFullInfinity(Main.LocalPlayer, consumable).Requirement;
    public long GetMixedInfinity(Player player, TConsumable consumable)
        => player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ? groupInfinity.Mixed.Infinity : GetMixedFullInfinity(player, consumable).Infinity;
    public FullInfinity GetMixedFullInfinity(Player player, int type) => _cachedInfinities.TryGet(type, out GroupInfinity? groupInfinity) ? groupInfinity.Mixed : GetMixedFullInfinity(player, FromType(type));
    public FullInfinity GetMixedFullInfinity(Player player, TConsumable consumable)
        => player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ? groupInfinity.Mixed : GetGroupInfinity(player, consumable).Mixed;

    public GroupInfinity GetGroupInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity;
        groupInfinity = new();
        foreach (InfinityRoot<TGroup, TConsumable> infinity in _infinities) {
            FullInfinity fullInfinity = GetFullInfinity(player, consumable, infinity);
            bool used = infinity.Enabled && !fullInfinity.Requirement.IsNone && (Config.MaxUsedInfinities == 0 || groupInfinity.UsedInfinities.Count < Config.MaxUsedInfinities);
            groupInfinity.Add(infinity, fullInfinity, used);
        }
        groupInfinity.AddMixed(Config.HasCustomGlobal(consumable, this, out Count? custom) ? new(custom!) : null);
        return groupInfinity;
    }

    public bool IsUsed(int type, IInfinity infinity) => _cachedInfinities.TryGet(type, out GroupInfinity? groupInfinity) ? groupInfinity.UsedInfinities.Contains(infinity) : IsUsed(FromType(type), infinity);
    public bool IsUsed(TConsumable consumable, IInfinity infinity) => (_cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity) ? groupInfinity.UsedInfinities : GetGroupInfinity(Main.LocalPlayer, consumable).UsedInfinities).Contains(infinity);
    public FullInfinity GetEffectiveInfinity(Player player, int type, IInfinity group) => !group.Enabled ? FullInfinity.None : IsUsed(type, group) ? GetFullInfinity(player, type, group) : GetMixedFullInfinity(player, type);
    public FullInfinity GetEffectiveInfinity(Player player, TConsumable consumable, InfinityRoot<TGroup, TConsumable> group) => !group.Enabled ? FullInfinity.None : IsUsed(consumable, group) ? GetFullInfinity(player, consumable, group) : GetMixedFullInfinity(player, consumable);

    public IEnumerable<(IInfinity infinity, int type, bool used)> GetDisplayedInfinities(Item item) {
        TConsumable consumable = ToConsumable(item);
        bool hasCustom = Config.HasCustomGlobal(consumable, this, out _);

        foreach (InfinityRoot<TGroup, TConsumable> infinity in _infinities) {
            if (!infinity.Enabled) continue;
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

    void IGroup.UpdateInfinities() {
        List<InfinityRoot<TGroup, TConsumable>> infinities = new(Infinities);
        _infinities.Clear();
        foreach ((var def, var enabled) in Config.Infinities.Items<InfinityDefinition, bool>()) {
            InfinityRoot<TGroup, TConsumable> infinity = infinities.Find(i => i.Mod.Name == def.Mod && i.Name == def.Name)!;
            infinity.Enabled = enabled;
            infinity.Color = Colors.Colors[def];
            _infinities.Add(infinity);
        }
    }

    void IGroup.LogCacheStats(){
        Mod.Logger.Debug($"{Name}:{_cachedInfinities}");
        _cachedInfinities.ResetStats();
    }

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);

    public IEnumerable<InfinityRoot<TGroup, TConsumable>> Infinities => _infinities;
    public GroupConfig Config { get; private set; } = null!;
    public GroupColors Colors { get; private set; } = null!;
    public ReadOnlyDictionary<IInfinity, IWrapper> InfinityConfigs => new(_infinityConfigs);
    
    IEnumerable<IInfinity> IGroup.Infinities => Infinities;
    IDictionary<IInfinity, IWrapper> IGroup.InfinityConfigs => InfinityConfigs;
    GroupConfig IGroup.Config { get => Config; set => Config = value; }
    GroupColors IGroup.Colors { get => Colors; set => Colors = value; }

    private readonly List<InfinityRoot<TGroup, TConsumable>> _infinities = new();
    private readonly Dictionary<IInfinity, IWrapper> _infinityConfigs = new();
    private readonly Cache<TConsumable, int, GroupInfinity> _cachedInfinities;
    
    public static TGroup Instance { get; private set; } = null!;
}
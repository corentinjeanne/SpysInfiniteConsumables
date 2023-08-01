using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using SPIC.Configs;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public enum InfinityVisibility { Hidden, Normal, Exclusive }

public interface IGroup : ILocalizedModType, ILoadable {
    void ClearInfinities();
    void ClearInfinity(Item item);

    string CountToString(Item item, IInfinity infinity, long owned, long value, CountStyle style);

    IEnumerable<(IInfinity infinity, FullInfinity display, InfinityVisibility visibility)> GetDisplayedInfinities(Player player, Item item);

    IEnumerable<IInfinity> Infinities { get; }
    
    LocalizedText DisplayName { get; }
    GroupConfig Config { get; }
    GroupColors Colors { get; }

    IDictionary<IInfinity, Wrapper> InfinityConfigs { get; }


    internal void LoadConfig(GroupConfig config);
    internal void LoadConfig(GroupColors colors);

    internal string CacheStats();
}

public abstract class Group<TGroup, TConsumable> : ModType, IGroup where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {

    public Group() {
        _cachedInfinities = new(GetType, consumable => GetGroupInfinity(Main.LocalPlayer, consumable)) {
            ValueSizeEstimate = (GroupInfinity value) => (value._infinities.Count + 1) * FullInfinity.EstimatedSize
        };
    }

    internal void Add(Infinity<TGroup, TConsumable> infinity) {
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
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
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

    public GroupInfinity GetGroupInfinity(Player player, TConsumable consumable) {
        if (player == Main.LocalPlayer && _cachedInfinities.TryGetOrCache(consumable, out GroupInfinity? groupInfinity)) return groupInfinity;
        groupInfinity = new();
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            FullInfinity fullInfinity = FullInfinity.WithInfinity(player, consumable, infinity);
            if(!infinity.Enabled || fullInfinity.Requirement.IsNone) continue;
            bool used = Config.UsedInfinities == 0 || groupInfinity.UsedInfinities.Count < Config.UsedInfinities;
            groupInfinity.Add(infinity, fullInfinity, used);
        }
        groupInfinity.AddMixed(Config.HasCustomGlobal(consumable, this, out Count? custom) ? new(custom) : null);
        return groupInfinity;
    }
    public GroupInfinity GetGroupInfinity(Player player, int consumable) => player == Main.LocalPlayer && _cachedInfinities.TryGet(consumable, out GroupInfinity? groupInfinity) ? groupInfinity : GetGroupInfinity(player, FromType(consumable));

    public bool IsUsed(TConsumable consumable, IInfinity infinity) => GetGroupInfinity(Main.LocalPlayer, consumable).UsedInfinities.ContainsKey(infinity);
    public FullInfinity GetEffectiveInfinity(Player player, TConsumable consumable, Infinity<TGroup, TConsumable> group) => GetGroupInfinity(player, consumable).EffectiveInfinity(group);
    public FullInfinity GetEffectiveInfinity(Player player, int consumable, Infinity<TGroup, TConsumable> group) => GetGroupInfinity(player, consumable).EffectiveInfinity(group);

    public FullInfinity GetMixedFullInfinity(Player player, TConsumable consumable) => GetGroupInfinity(player, consumable).Mixed;
    public Requirement GetMixedRequirement(TConsumable consumable) => GetMixedFullInfinity(Main.LocalPlayer, consumable).Requirement;
    public long GetMixedInfinity(Player player, TConsumable consumable) => GetMixedFullInfinity(player, consumable).Infinity;


    public IEnumerable<(IInfinity infinity, FullInfinity display, InfinityVisibility visibility)> GetDisplayedInfinities(Player player, Item item) {
        TConsumable consumable = ToConsumable(item);

        bool forcedByCustom = Config.HasCustomGlobal(consumable, this, out _);

        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            TConsumable displayed = infinity.DisplayedValue(consumable);
            FullInfinity effective = GetEffectiveInfinity(player, displayed, infinity);

            if (effective.Requirement.IsNone) continue;
            List<object> extras = new(effective.Extras);
            Requirement requirement = effective.Requirement;
            long count = effective.Count;
            bool weapon = !consumable.Equals(displayed);
            InfinityVisibility visibility = IsUsed(displayed, infinity) || weapon ? InfinityVisibility.Normal : InfinityVisibility.Hidden;
            infinity.OverrideDisplay(player, item, displayed, ref requirement, ref count, extras, ref visibility);
            if (visibility == InfinityVisibility.Normal && !Main.LocalPlayer.IsFromVisibleInventory(item)) {
                if (weapon) visibility = InfinityVisibility.Hidden;
                count = 0;
            }
            if (forcedByCustom) {
                if (visibility == InfinityVisibility.Hidden) visibility = InfinityVisibility.Normal;
                forcedByCustom = false;
            }
            if (!InfinityDisplay.Instance.general_ExclusiveDisplay && visibility == InfinityVisibility.Exclusive) visibility = InfinityVisibility.Normal;
            if(visibility == InfinityVisibility.Hidden) continue;
            yield return (infinity, FullInfinity.With(requirement, count, requirement.Infinity(count), extras.ToArray()), visibility);
            
        }
    }

    public abstract TConsumable ToConsumable(Item item);
    public abstract Item ToItem(TConsumable consumable);

    public abstract int GetType(TConsumable consumable);
    public abstract TConsumable FromType(int type);

    public abstract long CountConsumables(Player player, TConsumable consumable);
    public virtual long MaxStack(TConsumable consumable) => 0;

    public abstract string CountToString(TConsumable consumable, long count, CountStyle style, bool rawValue = false);
    public virtual string CountToString(Item item, IInfinity infinity, long owned, long value, CountStyle style) {
        TConsumable consumable = ((Infinity<TGroup, TConsumable>)infinity).DisplayedValue(ToConsumable(item));
        return owned == 0 ? CountToString(consumable, value, style) : $"{CountToString(consumable, owned, style, true)}/{CountToString(consumable, value, style)}";
    }


    public void ClearInfinities() => _cachedInfinities.Clear();
    public void ClearInfinity(Item item) => _cachedInfinities.Clear(ToConsumable(item));

    void IGroup.LoadConfig(GroupConfig config) {
        OrderedDictionary /* <InfinityDefinition, bool> */ infinitiesBool = new();
        foreach ((string key, bool enabled) in config.Infinities.Items<string, bool>()) infinitiesBool[new InfinityDefinition(key)] = enabled;
        config.Infinities = infinitiesBool;

        List<Infinity<TGroup, TConsumable>> infinities = new(_infinities);
        _infinities.Clear();
        foreach ((InfinityDefinition def, bool enabled) in config.Infinities.Items<InfinityDefinition, bool>()) {
            int i = infinities.FindIndex(i => i.Mod.Name == def.Mod && i.Name == def.Name);
            if (i == -1) continue;
            infinities[i].Enabled = (bool)config.Infinities[def]!;
            _infinities.Add(infinities[i]);
            infinities.RemoveAt(i);
        }
        foreach (Infinity<TGroup, TConsumable> infinity in infinities) {
            InfinityDefinition def = new(infinity);
            config.Infinities.TryAdd(def, infinity.DefaultsToOn);
            infinity.Enabled = (bool)config.Infinities[def]!;
            _infinities.Add(infinity);
        }

        foreach ((IInfinity infinity, Wrapper wrapper) in _infinityConfigs) {
            InfinityDefinition def = new(infinity);
            config.Configs[def] = config.Configs.TryGetValue(def, out var c) ? c.ChangeType(wrapper.Member.Type) : Wrapper.From(wrapper.Member.Type);
            wrapper.Value = config.Configs[def].Value;
        }

        foreach (Custom custom in config.Customs.Values) {
            foreach (InfinityDefinition def in custom.Individual.Keys) {
                def.Filter = this;
                if ((InfinityManager.GetInfinity(def.Mod, def.Name)?.GetType().IsSubclassOfGeneric(typeof(Infinity<,,>), out System.Type? infinity3)) != true) custom.Individual[def] = new Count(custom.Individual[def].Value);
                else custom.Individual[def] = (Count)System.Activator.CreateInstance(typeof(Count<>).MakeGenericType(infinity3!.GenericTypeArguments[2]), custom.Individual[def].Value)!;
            }
        }
        Config = config;
    }
    void IGroup.LoadConfig(GroupColors colors) {
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            InfinityDefinition def = new(infinity);
            infinity.Color = colors.Colors[def] = colors.Colors.GetValueOrDefault(def, infinity.DefaultColor);
        }
        Colors = colors;
    }

    string IGroup.CacheStats() {
        string s = $"{Name}: {_cachedInfinities.Stats()}";
        _cachedInfinities.ClearStats();
        return s;
    }

    public string LocalizationCategory => "Infinities";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", PrettyPrintName);

    public ReadOnlyCollection<Infinity<TGroup, TConsumable>> Infinities => _infinities.AsReadOnly();
    public GroupConfig Config { get; private set; } = null!;
    public GroupColors Colors { get; private set; } = null!;
    public ReadOnlyDictionary<IInfinity, Wrapper> InfinityConfigs => new(_infinityConfigs);
    
    IEnumerable<IInfinity> IGroup.Infinities => _infinities;
    IDictionary<IInfinity, Wrapper> IGroup.InfinityConfigs => InfinityConfigs;

    private readonly List<Infinity<TGroup, TConsumable>> _infinities = new();
    private readonly Dictionary<IInfinity, Wrapper> _infinityConfigs = new();
    private readonly Cache<TConsumable, int, GroupInfinity> _cachedInfinities;
    
    public static TGroup Instance { get; private set; } = null!;
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using SPIC.Configs;
using SPIC.Configs.Presets;
using SpikysLib.Extensions;
using SpikysLib.Configs.UI;
using SpikysLib.DataStructures;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using SPIC.Configs.UI;

namespace SPIC;

public enum InfinityVisibility { Hidden, Normal, Exclusive }

// ? Add transformations Infinity
// ? Add quest fish Infinity
// ? Add HP/Mana Infinity

public interface IGroup : ILocalizedModType, ILoadable {
    void ClearInfinities();

    internal void Add(Preset preset);

    IEnumerable<(IInfinity infinity, int displayed, FullInfinity display, InfinityVisibility visibility)> GetDisplayedInfinities(Player player, Item item);

    IReadOnlyList<IInfinity> Infinities { get; }
    
    LocalizedText DisplayName { get; }
    GroupConfig Config { get; }
    GroupColors Colors { get; }

    ReadOnlyCollection<Preset>? Presets { get; }

    internal void LoadConfig(GroupConfig config);
    internal void LoadConfig(GroupColors colors);

    internal string CacheStats();
}

public abstract class Group<TGroup, TConsumable> : ModType, IGroup where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {

    public Group() {
        _cachedInfinities = new(GetType, consumable => ComputeGroupInfinity(Main.LocalPlayer, consumable)) {
            EstimateValueSize = (GroupInfinity value) => (value._infinities.Count + 1) * FullInfinity.EstimatedSize
        };
    }

    internal void Add(Infinity<TGroup, TConsumable> infinity) {
        _infinities.Add(infinity);
        infinity.Group = (TGroup)this;
        infinity.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, infinity);
    }

    void IGroup.Add(Preset preset) => Add(preset);
    internal void Add(Preset preset) => _presets.Add(preset);

    public Wrapper<T> AddConfig<T>(IInfinity infinity) where T : new() {
        Wrapper<T> wrapper = new();
        _configs[infinity] = wrapper;
        return wrapper;
    }

    public override void Load() => Instance = (TGroup)this;

    public override void Unload() {
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            infinity.Group = null!;
            infinity.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, null);
        }
        _infinities.Clear();
        _configs.Clear();
        Instance = null!;
    }
    protected sealed override void Register() {
        ModTypeLookup<Group<TGroup, TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() => SetStaticDefaults();

    public GroupInfinity GetGroupInfinity(Player player, int consumable) => player == Main.LocalPlayer && _cachedInfinities.TryGetValue(consumable, out GroupInfinity? groupInfinity) ? groupInfinity : GetGroupInfinity(player, FromType(consumable));
    public GroupInfinity GetGroupInfinity(Player player, TConsumable consumable) => player == Main.LocalPlayer ? _cachedInfinities.GetOrAdd(consumable) : ComputeGroupInfinity(player, consumable);
    private GroupInfinity ComputeGroupInfinity(Player player, TConsumable consumable) {
        GroupInfinity groupInfinity = new();
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            FullInfinity fullInfinity = FullInfinity.WithInfinity(player, consumable, infinity);
            if(!infinity.Enabled || fullInfinity.Requirement.IsNone) continue;
            bool used = Config.UsedInfinities == 0 || groupInfinity.UsedInfinities.Count < Config.UsedInfinities;
            groupInfinity.Add(infinity, fullInfinity, used);
        }
        groupInfinity.AddMixed(Config.HasCustomGlobal(consumable, this, out Count? custom) ? new(custom) : null);
        return groupInfinity;
    }

    public bool IsUsed(TConsumable consumable, IInfinity infinity) => GetGroupInfinity(Main.LocalPlayer, consumable).UsedInfinities.ContainsKey(infinity);
    public FullInfinity GetEffectiveInfinity(Player player, TConsumable consumable, Infinity<TGroup, TConsumable> group) => GetGroupInfinity(player, consumable).EffectiveInfinity(group);
    public FullInfinity GetEffectiveInfinity(Player player, int consumable, Infinity<TGroup, TConsumable> group) => GetGroupInfinity(player, consumable).EffectiveInfinity(group);

    public FullInfinity GetMixedFullInfinity(Player player, TConsumable consumable) => GetGroupInfinity(player, consumable).Mixed;
    public Requirement GetMixedRequirement(TConsumable consumable) => GetMixedFullInfinity(Main.LocalPlayer, consumable).Requirement;
    public long GetMixedInfinity(Player player, TConsumable consumable) => GetMixedFullInfinity(player, consumable).Infinity;


    public IEnumerable<(IInfinity infinity, int displayed, FullInfinity display, InfinityVisibility visibility)> GetDisplayedInfinities(Player player, Item item) {
        TConsumable consumable = ToConsumable(item);

        GroupInfinity consumableInfinity = InfinityDisplay.Instance.Cache == CacheStyle.Performances ? GetGroupInfinity(player, consumable) : ComputeGroupInfinity(player, consumable); ;
        bool forcedByCustom = consumableInfinity.UsedInfinities.Count == 0 && !consumableInfinity.Mixed.Requirement.IsNone;

        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            foreach(TConsumable displayed in infinity.DisplayedValues(consumable)){
                bool weapon = !consumable.Equals(displayed);
                GroupInfinity displayedInfinity = weapon ? GetGroupInfinity(player, displayed) : consumableInfinity;
                FullInfinity effective = displayedInfinity.EffectiveInfinity(infinity);

                if (effective.Requirement.IsNone) continue;
                List<object> extras = new(effective.Extras);
                Requirement requirement = effective.Requirement;
                long count = effective.Count;
                InfinityVisibility visibility = displayedInfinity.UsedInfinities.ContainsKey(infinity) || weapon ? InfinityVisibility.Normal : InfinityVisibility.Hidden;
                infinity.OverrideDisplay(player, item, displayed, ref requirement, ref count, extras, ref visibility);
                if (visibility == InfinityVisibility.Normal && !Main.LocalPlayer.IsFromVisibleInventory(item)) {
                    if (weapon) visibility = InfinityVisibility.Hidden;
                    count = 0;
                }
                if (forcedByCustom) {
                    if (visibility == InfinityVisibility.Hidden) visibility = InfinityVisibility.Normal;
                    forcedByCustom = false;
                }
                if (!InfinityDisplay.Instance.ShowExclusiveDisplay && visibility == InfinityVisibility.Exclusive) visibility = InfinityVisibility.Normal;
                if (visibility == InfinityVisibility.Hidden) continue;
                yield return (infinity, GetType(displayed), FullInfinity.With(requirement, count, requirement.Infinity(count), extras.ToArray()), visibility);
            }
        }
    }

    public abstract TConsumable ToConsumable(Item item);
    public abstract Item ToItem(TConsumable consumable);

    public abstract int GetType(TConsumable consumable);
    public abstract TConsumable FromType(int type);

    public abstract long CountConsumables(Player player, TConsumable consumable);

    public void ClearInfinities() => _cachedInfinities.Clear();

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
            config.Infinities.TryAdd(def, infinity.DefaultState());
            infinity.Enabled = (bool)config.Infinities[def]!;
            _infinities.Add(infinity);
        }

        foreach ((IInfinity infinity, Wrapper wrapper) in _configs) {
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
        Preset? preset = null;
        foreach (Preset p in PresetLoader.Presets) if (p.MeetsCriterias(config) && (preset is null || p.CriteriasCount >= preset.CriteriasCount)) preset = p;
        config.Preset = preset is not null ? new(preset.Mod.Name, preset.Name) : new();
        config.Preset.Filter = this;
        Config = config;
    }
    void IGroup.LoadConfig(GroupColors colors) {
        foreach (Infinity<TGroup, TConsumable> infinity in _infinities) {
            InfinityDefinition def = new(infinity);
            infinity.Color = colors.Colors[def] = colors.Colors.GetValueOrDefault(def, infinity.DefaultColor());
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
    public ReadOnlyCollection<Preset> Presets => _presets.AsReadOnly();
    public GroupConfig Config { get; private set; } = null!;
    public GroupColors Colors { get; private set; } = null!;

    IReadOnlyList<IInfinity> IGroup.Infinities => _infinities;

    private readonly List<Infinity<TGroup, TConsumable>> _infinities = new();
    private readonly Dictionary<IInfinity, Wrapper> _configs = new();
    private readonly List<Preset> _presets = new();
    private readonly Cache<TConsumable, int, GroupInfinity> _cachedInfinities;
    
    public static TGroup Instance { get; private set; } = null!;
}
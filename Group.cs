using System.Collections.Generic;
using System.Collections.ObjectModel;
using SPIC.Configs;
using SPIC.Configs.Presets;
using SpikysLib.Extensions;
using SpikysLib.Configs;
using SpikysLib.DataStructures;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Reflection;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System;

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

public abstract class Group<TConsumable> : ModType, IGroup where TConsumable : notnull {
    internal void Add(Infinity<TConsumable> infinity) => _infinities.Add(infinity);

    void IGroup.Add(Preset preset) => Add(preset);
    internal void Add(Preset preset) => _presets.Add(preset);

    public override void Unload() {
        foreach (IInfinity infinity in _infinities) Utility.SetConfig(infinity, null);
        _infinities.Clear();
    }

    protected sealed override void Register() {
        ModTypeLookup<Group<TConsumable>>.Register(this);
        InfinityManager.Register(this);
    }
    public sealed override void SetupContent() => SetStaticDefaults();

    public GroupInfinity GetGroupInfinity(Player player, int consumable) => _cachedInfinities.TryGetValue((player.whoAmI, consumable), out GroupInfinity? groupInfinity) ? groupInfinity : GetGroupInfinity(player, FromType(consumable));
    public GroupInfinity GetGroupInfinity(Player player, TConsumable consumable) => InfinityDisplay.Instance.Cache != CacheStyle.None ? _cachedInfinities.GetOrAdd((player.whoAmI, GetType(consumable)), () => ComputeGroupInfinity(player, consumable)) : ComputeGroupInfinity(player, consumable);
    private GroupInfinity ComputeGroupInfinity(Player player, TConsumable consumable) {
        GroupInfinity groupInfinity = new();
        foreach (Infinity<TConsumable> infinity in _infinities) {
            FullInfinity fullInfinity = FullInfinity.WithInfinity(player, consumable, infinity);
            if(!infinity.Enabled || fullInfinity.Requirement.IsNone) continue;
            bool used = Config.UsedInfinities == 0 || groupInfinity.UsedInfinities.Count < Config.UsedInfinities;
            groupInfinity.Add(infinity, fullInfinity, used);
        }
        groupInfinity.AddMixed(Config.HasCustomGlobal(consumable, this, out Count? custom) ? new(custom) : null);
        return groupInfinity;
    }

    public bool IsUsed(TConsumable consumable, IInfinity infinity) => GetGroupInfinity(Main.LocalPlayer, consumable).UsedInfinities.ContainsKey(infinity);
    public FullInfinity GetEffectiveInfinity(Player player, TConsumable consumable, Infinity<TConsumable> group) => GetGroupInfinity(player, consumable).EffectiveInfinity(group);
    public FullInfinity GetEffectiveInfinity(Player player, int consumable, Infinity<TConsumable> group) => GetGroupInfinity(player, consumable).EffectiveInfinity(group);
    public FullInfinity GetMixedInfinity(Player player, TConsumable consumable) => GetGroupInfinity(player, consumable).Mixed;


    public IEnumerable<(IInfinity infinity, int displayed, FullInfinity display, InfinityVisibility visibility)> GetDisplayedInfinities(Player player, Item item) {
        TConsumable consumable = ToConsumable(item);

        GroupInfinity consumableInfinity = InfinityDisplay.Instance.Cache == CacheStyle.Performances ? GetGroupInfinity(player, consumable) : ComputeGroupInfinity(player, consumable); ;
        bool forcedByCustom = consumableInfinity.UsedInfinities.Count == 0 && !consumableInfinity.Mixed.Requirement.IsNone;

        foreach (Infinity<TConsumable> infinity in _infinities) {
            List<TConsumable> displayedConsumables = new() { consumable };
            infinity.ModifyDisplayedConsumables(consumable, displayedConsumables);
            foreach(TConsumable displayed in displayedConsumables){
                bool weapon = !consumable.Equals(displayed);
                GroupInfinity displayedInfinity = weapon ? (InfinityDisplay.Instance.Cache == CacheStyle.Performances ? GetGroupInfinity(player, displayed) : ComputeGroupInfinity(player, displayed)) : consumableInfinity;
                FullInfinity effective = displayedInfinity.EffectiveInfinity(infinity);

                if (effective.Requirement.IsNone) continue;
                List<object> extras = new(effective.Extras);
                Requirement requirement = effective.Requirement;
                long count = effective.Count;
                InfinityVisibility visibility = displayedInfinity.UsedInfinities.ContainsKey(infinity) || weapon ? InfinityVisibility.Normal : InfinityVisibility.Hidden;
                infinity.ModifyDisplay(player, item, displayed, ref requirement, ref count, extras, ref visibility);
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
        (OrderedDictionary oldInfs, config.Infinities) = (config.Infinities, new());
        List<Infinity<TConsumable>> infinities = new(_infinities);
        _infinities.Clear();
        foreach((string k, object v) in oldInfs.Items<string, object>()) {
            InfinityDefinition def = new(k);
            Infinity<TConsumable>? infinity = infinities.Find(i => i.Mod.Name == def.Mod && i.Name == def.Name);
            if (infinity is null) continue;
            
            JToken token;
            if (v is bool oldEn) token = JObject.FromObject(config.Configs.TryGetValue(def, out Wrapper? oldVal) ? new Toggle<JObject>(oldEn, ((JObject)oldVal.Value!) ?? new()) : new Empty()); // Compatibility version < v3.1.1
            else if (v is JToken t) token = t;
            else continue;

            _infinities.Add(infinity); 
            infinities.Remove(infinity);

            FieldInfo? configField = infinity.GetType().GetField("Config", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            INestedValue value = (INestedValue)token.ToObject(typeof(Toggle<>).MakeGenericType(configField is null ? typeof(Empty) : configField.FieldType))!;

            config.Infinities.Add(def, value);
            infinity.Enabled = (bool)value.Key;
            configField?.SetValue(infinity, value.Value);
        }
        config.Configs.Clear(); // Compatibility version < v3.1.1
        foreach (Infinity<TConsumable> infinity in infinities) {
            _infinities.Add(infinity);
            InfinityDefinition def = new(infinity);

            FieldInfo? configField = infinity.GetType().GetField("Config", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            INestedValue value = (INestedValue)Activator.CreateInstance(typeof(Toggle<>).MakeGenericType(configField is null ? typeof(Empty) : configField.FieldType), infinity.DefaultEnabled(), null)!;

            config.Infinities.Add(def, value);
            infinity.Enabled = (bool)value.Key;
            configField?.SetValue(infinity, value.Value);
        }

        foreach (Custom custom in config.Customs.Values) {
            foreach (InfinityDefinition def in custom.Individual.Keys) {
                def.Filter = this;
                if ((InfinityManager.GetInfinity(def.Mod, def.Name)?.GetType().IsSubclassOfGeneric(typeof(Infinity<,>), out System.Type? infinity2)) != true) custom.Individual[def] = new Count(custom.Individual[def].Value);
                else custom.Individual[def] = (Count)System.Activator.CreateInstance(typeof(Count<>).MakeGenericType(infinity2!.GenericTypeArguments[1]), custom.Individual[def].Value)!;
            }
        }
        
        Preset? preset = null;
        foreach (Preset p in PresetLoader.Presets) if (p.MeetsCriterias(config) && (preset is null || p.CriteriasCount >= preset.CriteriasCount)) preset = p;
        config.Preset = preset is not null ? new(preset.Mod.Name, preset.Name) : new();
        config.Preset.Filter = this;
        Config = config;
    }
    void IGroup.LoadConfig(GroupColors colors) {
        foreach (Infinity<TConsumable> infinity in _infinities) {
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

    public ReadOnlyCollection<Infinity<TConsumable>> Infinities => _infinities.AsReadOnly();
    public ReadOnlyCollection<Preset> Presets => _presets.AsReadOnly();
    public GroupConfig Config { get; private set; } = null!;
    public GroupColors Colors { get; private set; } = null!;

    IReadOnlyList<IInfinity> IGroup.Infinities => _infinities;

    private readonly List<Infinity<TConsumable>> _infinities = new();
    private readonly List<Preset> _presets = new();
    private readonly DictionaryWithStats<(int player, int consumable), GroupInfinity> _cachedInfinities = new() {
        EstimateValueSize = (GroupInfinity value) => (value._infinities.Count + 1) * FullInfinity.EstimatedSize
    };
}
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using Terraria;
using Terraria.ID;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Configs;

public class InfinitySettings : ModConfig {

    [Header($"${Localization.Keys.InfinitySettings}.General.Header")]
    [DefaultValue(true)]
    public bool PreventItemDupication { get; set; }

    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<GroupDefinition, GroupConfig> GroupConfigs {
        get => _groupConfigs;
        set {
            _groupConfigs.Clear();
            foreach (IGroup group in InfinityManager.Groups) {
                GroupDefinition def = new(group);
                group.Config = _groupConfigs[def] = value.TryGetValue(def, out GroupConfig? config) ? config : new();
                group.Config.Group = group;
            }
        }
    }

    [Header($"${Localization.Keys.InfinitySettings}.Settings.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<InfinityDefinition, WrapperBase<object>> Configs {
        get => _configs;
        set {
            _configs.Clear();
            foreach ((IInfinity infinity, IWrapper wrapper) in InfinityManager.Configs) {
                InfinityDefinition def = new(infinity);
                _configs[def] = value.TryGetValue(def, out WrapperBase<object>? config) ? config.ChangeType(wrapper.Type) : WrapperBase<object>.From(wrapper.Type);
                wrapper.Value = _configs[def].Value;
            }
        }
    }

    [Header($"${Localization.Keys.InfinitySettings}.Customs.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<GroupDefinition, Dictionary<ItemDefinition, Custom>> Customs {
        get => _customs;
        set {
            _customs.Clear();
            foreach (IGroup group in InfinityManager.Groups) {
                GroupDefinition def = new(group);
                _customs[def] = value.TryGetValue(def, out Dictionary<ItemDefinition, Custom>? customs) ? customs : new();
                foreach(Custom custom in _customs[def].Values) custom.Group = group;
            }
        }
    }

    public bool HasCustomCount<TGroup, TConsumable>(TConsumable consumable, Infinity<TGroup, TConsumable> infinity, [MaybeNullWhen(false)] out Count count) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        TGroup group = infinity.Group;
        Dictionary<ItemDefinition, Custom> customs = Customs[new(group)];
        ItemDefinition def = new(group.ToItem(consumable).type);
        if(customs.TryGetValue(def, out Custom? custom) && custom.TryGetValue(infinity, out count)) return true;
        count = default;
        return false;
    }
    public bool HasCustomGlobalRequirement<TGroup, TConsumable>(TConsumable consumable, Group<TGroup, TConsumable> group, [MaybeNullWhen(false)] out Requirement requirement) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        Dictionary<ItemDefinition, Custom> customs = Customs[new(group)];
        ItemDefinition def = new(group.ToItem(consumable).type);
        if(customs.TryGetValue(def, out Custom? custom) && custom.TryGetGlobal(out Count? count)) {
            requirement = new(count);
            return true;
        }
        requirement = new();
        return false;
    }
    public override void OnChanged() {
        if(!Main.gameMenu && Main.netMode != NetmodeID.Server) InfinityManager.ClearInfinities();
        if(Main.netMode != NetmodeID.Server) InfinityManager.SortInfinities();
    }

    private readonly Dictionary<GroupDefinition, Dictionary<ItemDefinition, Custom>> _customs = new();
    private readonly Dictionary<GroupDefinition, GroupConfig> _groupConfigs = new();
    private readonly Dictionary<InfinityDefinition, WrapperBase<object>> _configs = new();

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static InfinitySettings Instance = null!;
}
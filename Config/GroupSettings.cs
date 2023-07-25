using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using Terraria;
using Terraria.ID;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Configs;

public class GroupSettings : ModConfig {

    [Header($"${Localization.Keys.GroupSettings}.General.Header")]
    [DefaultValue(true)]
    public bool PreventItemDupication { get; set; }

    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<MetaGroupDefinition, MetaConfig> MetaConfigs {
        get => _metaConfigs;
        set {
            _metaConfigs.Clear();
            foreach (IMetaGroup metaGroup in InfinityManager.MetaGroups) {
                MetaGroupDefinition def = new(metaGroup);
                metaGroup.Config = _metaConfigs[def] = value.TryGetValue(def, out MetaConfig? config) ? config : new();
                metaGroup.Config.MetaGroup = metaGroup;
            }
        }
    }

    [Header($"${Localization.Keys.GroupSettings}.Settings.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ModGroupDefinition, object> Configs {
        get => _configs;
        set {
            _configs.Clear();
            foreach ((IModGroup group, IWrapper wrapper) in InfinityManager.Configs) {
                ModGroupDefinition def = new(group);
                if (value.TryGetValue(def, out object? config)) {
                    if (config is JObject jobj) config = jobj.ToObject(wrapper.Type)!;
                    else if (config.GetType() != wrapper.Type) throw new NotImplementedException();
                    _configs[def] = config;
                } else {
                    _configs[def] = Activator.CreateInstance(wrapper.Type)!;
                }
                wrapper.Obj = _configs[def];
            }
        }
    }

    [Header($"${Localization.Keys.GroupSettings}.Customs.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<MetaGroupDefinition, Dictionary<ItemDefinition, Custom>> Customs {
        get => _customs;
        set {
            _customs.Clear();
            foreach (IMetaGroup metaGroup in InfinityManager.MetaGroups) {
                MetaGroupDefinition def = new(metaGroup);
                _customs[def] = value.TryGetValue(def, out Dictionary<ItemDefinition, Custom>? customs) ? customs : new();
                foreach(Custom custom in _customs[def].Values) custom.MetaGroup = metaGroup;
            }
        }
    }

    public bool HasCustomCount<TMetaGroup, TConsumable>(TConsumable consumable, ModGroup<TMetaGroup, TConsumable> group, [MaybeNullWhen(false)] out Count count) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
        TMetaGroup metaGroup = group.MetaGroup;
        Dictionary<ItemDefinition, Custom> customs = Customs[new(metaGroup)];
        ItemDefinition def = new(metaGroup.ToItem(consumable).type);
        if(customs.TryGetValue(def, out Custom? custom) && custom.TryGetValue(group, out count)) return true;
        count = default;
        return false;
    }
    public bool HasCustomGlobalRequirement<TMetaGroup, TConsumable>(TConsumable consumable, MetaGroup<TMetaGroup, TConsumable> metaGroup, [MaybeNullWhen(false)] out Requirement requirement) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
        Dictionary<ItemDefinition, Custom> customs = Customs[new(metaGroup)];
        ItemDefinition def = new(metaGroup.ToItem(consumable).type);
        if(customs.TryGetValue(def, out Custom? custom) && custom.TryGetGlobal(out Count? count)) {
            requirement = new(count);
            return true;
        }
        requirement = new();
        return false;
    }
    public override void OnChanged() {
        if(!Main.gameMenu && Main.netMode != NetmodeID.Server) InfinityManager.ClearInfinities();
        if(Main.netMode != NetmodeID.Server) InfinityManager.SortGroups();
    }

    private readonly Dictionary<MetaGroupDefinition, Dictionary<ItemDefinition, Custom>> _customs = new();
    private readonly Dictionary<MetaGroupDefinition, MetaConfig> _metaConfigs = new();
    private readonly Dictionary<ModGroupDefinition, object> _configs = new();

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static GroupSettings Instance = null!;
}
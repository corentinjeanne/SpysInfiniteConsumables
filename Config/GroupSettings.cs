using System.Collections.Generic;
using System.ComponentModel;
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
    public Dictionary<ModConsumableDefinition, ConsumableConfig> ConsumableConfigs {
        get => _consumableConfigs;
        set {
            _consumableConfigs.Clear();
            foreach (IModConsumable modConsumable in InfinityManager.ModConsumables) {
                ModConsumableDefinition def = new(modConsumable);
                modConsumable.Config = _consumableConfigs[def] = value.TryGetValue(def, out ConsumableConfig? config) ? config : new();
                modConsumable.Config.ModConsumable = modConsumable;
            }
        }
    }

    [Header($"${Localization.Keys.GroupSettings}.Settings.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ModGroupDefinition, GenericWrapper<object>> Configs {
        get => _configs;
        set {
            _configs.Clear();
            foreach ((IModGroup group, IWrapper wrapper) in InfinityManager.Configs) {
                ModGroupDefinition def = new(group);
                _configs[def] = value.TryGetValue(def, out GenericWrapper<object>? config) ? config.MakeGeneric(wrapper.Type) : GenericWrapper<object>.From(wrapper.Type);
                wrapper.Obj = _configs[def].Value;
            }
        }
    }

    [Header($"${Localization.Keys.GroupSettings}.Customs.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ModConsumableDefinition, Dictionary<ItemDefinition, Custom>> Customs {
        get => _customs;
        set {
            _customs.Clear();
            foreach (IModConsumable modConsumable in InfinityManager.ModConsumables) {
                ModConsumableDefinition def = new(modConsumable);
                _customs[def] = value.TryGetValue(def, out Dictionary<ItemDefinition, Custom>? customs) ? customs : new();
                foreach(Custom custom in _customs[def].Values) custom.ModConsumable = modConsumable;
            }
        }
    }

    public bool HasCustomCount<TModConsumable, TConsumable>(TConsumable consumable, ModGroup<TModConsumable, TConsumable> group, [MaybeNullWhen(false)] out Count count) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
        TModConsumable modConsumable = group.ModConsumable;
        Dictionary<ItemDefinition, Custom> customs = Customs[new(modConsumable)];
        ItemDefinition def = new(modConsumable.ToItem(consumable).type);
        if(customs.TryGetValue(def, out Custom? custom) && custom.TryGetValue(group, out count)) return true;
        count = default;
        return false;
    }
    public bool HasCustomGlobalRequirement<TModConsumable, TConsumable>(TConsumable consumable, ModConsumable<TModConsumable, TConsumable> modConsumable, [MaybeNullWhen(false)] out Requirement requirement) where TModConsumable : ModConsumable<TModConsumable, TConsumable> where TConsumable : notnull {
        Dictionary<ItemDefinition, Custom> customs = Customs[new(modConsumable)];
        ItemDefinition def = new(modConsumable.ToItem(consumable).type);
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

    private readonly Dictionary<ModConsumableDefinition, Dictionary<ItemDefinition, Custom>> _customs = new();
    private readonly Dictionary<ModConsumableDefinition, ConsumableConfig> _consumableConfigs = new();
    private readonly Dictionary<ModGroupDefinition, GenericWrapper<object>> _configs = new();

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static GroupSettings Instance = null!;
}
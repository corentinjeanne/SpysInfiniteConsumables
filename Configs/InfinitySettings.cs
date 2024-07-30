using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria;
using Terraria.ID;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;


public class GroupValueWrapper<TValue> : ValueWrapper<GroupDefinition, TValue> {
    public override TValue Value { get; set; } = default!;
    public override void OnBind(ConfigElement element) {
        if (Key.IsUnloaded) return;
        SpikysLib.Reflection.ConfigElement.TooltipFunction.SetValue(element, () => Key.Tooltip!);
    }
}

public sealed class InfinitySettings : ModConfig {

    [Header("Features")]
    [DefaultValue(true)] public bool DetectMissingCategories;
    [DefaultValue(true)] public bool PreventItemDuplication { get; set; }

    [Header("Configs")]
    [CustomModConfigItem(typeof(DictionaryValuesElement)), ValueWrapper(typeof(GroupValueWrapper<>))] public Dictionary<GroupDefinition, GroupConfig> Configs {
        get => _configs;
        set {
            foreach (IGroup group in InfinityManager.Groups) {
                GroupDefinition def = new(group);
                value[def] = value.GetValueOrDefault(def, new());
                group.LoadConfig(value[def]);
            }
            _configs = value;
        }
    }

    public override void OnChanged() {
        if(!Main.gameMenu && Main.netMode != NetmodeID.Server) InfinityManager.ClearInfinities();
    }

    private Dictionary<GroupDefinition, GroupConfig> _configs = new();

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static InfinitySettings Instance = null!;
}
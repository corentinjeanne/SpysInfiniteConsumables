using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using Terraria;
using Terraria.ID;

namespace SPIC.Configs;

public sealed class InfinitySettings : ModConfig {

    [Header("Features")]
    [DefaultValue(true)]
    public bool DetectMissingCategories;
    [DefaultValue(true)]
    public bool PreventItemDupication { get; set; }

    [Header("Configs")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<GroupDefinition, GroupConfig> Configs {
        get => _configs;
        set {
            foreach (IGroup group in InfinityManager.Groups) {
                GroupDefinition def = new(group);
                if (value.TryGetValue(def, out GroupConfig? config)) config.SetGroup(group);
                else value[def] = config = new(group);
                group.Config = config;
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
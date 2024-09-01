using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria;
using Terraria.ID;
using SpikysLib.Configs.UI;

namespace SPIC.Configs;

public sealed class InfinitySettings : ModConfig {

    [Header("Features")]
    [DefaultValue(true)] public bool DetectMissingCategories;
    [DefaultValue(true)] public bool PreventItemDuplication { get; set; }

    public override ConfigScope Mode => ConfigScope.ServerSide;    
    public static InfinitySettings Instance = null!;
}
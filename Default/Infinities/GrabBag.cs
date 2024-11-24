using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using SPIC.Configs;
using Terraria.ModLoader;
using SpikysLib;
using SPIC.Default.Displays;
using System.Collections.Generic;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace SPIC.Default.Infinities;

public enum GrabBagCategory {
    None,
    Container,
    Extractinator,
    TreasureBag,
}

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class GrabBagRequirements {
    public Count<GrabBagCategory> Container = 10;
    public Count<GrabBagCategory> Extractinator = 499;
    public Count<GrabBagCategory> TreasureBag = 3;
}

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class GrabBagDisplay {
    [DefaultValue(true)] public bool reuseTooltip = true;
}

public sealed class GrabBag : Infinity<Item, GrabBagCategory>, IConfigProvider<GrabBagRequirements>, IClientConfigProvider<GrabBagDisplay>, ITooltipLineDisplay {
    public static GrabBag Instance = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance; 
    public GrabBagRequirements Config { get; set; } = null!;
    public GrabBagDisplay ClientConfig { get; set; } = null!;

    public sealed override InfinityDefaults Defaults => new(){
        Enabled = false,
        Color = Colors.RarityDarkPurple
    };

    public override long GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => Config.Container,
        GrabBagCategory.TreasureBag => Config.TreasureBag,
        GrabBagCategory.Extractinator => Config.Extractinator,
        _ => 0,
    };

    protected override GrabBagCategory GetCategoryInner(Item item) {
        switch (item.type) {
        case ItemID.Geode: return GrabBagCategory.Container;
        }
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        var drops = Main.ItemDropsDB.GetRulesForItemID(item.type);
        if (drops.Count != 0){
            if(drops.Count == 1 && drops[0] is CommonDrop commonDrop && Main.ItemDropsDB.GetRulesForItemID(commonDrop.itemId).Count == 1) return GrabBagCategory.None;
            return GrabBagCategory.Container;
        }
        if(ItemID.Sets.ExtractinatorMode[item.type] != -1) return GrabBagCategory.Extractinator;
        return GrabBagCategory.None;
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed)
        => ClientConfig.reuseTooltip ? (new(Instance.Mod, item.type == ItemID.LockBox && item.type != displayed ? "Tooltip1" : "Tooltip0", Instance.DisplayName.Value), TooltipLineID.Tooltip) : Displays.Tooltip.DefaultTooltipLine(this);
    
    protected override void ModifyDisplayedConsumables(Item item, int context, ref List<Item> displayed) {
        Item? key = item.type == ItemID.LockBox ? Main.LocalPlayer.FindItemRaw(ItemID.GoldenKey) : null ;
        if (key is not null) displayed.Add(key);
    }
}

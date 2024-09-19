using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using SpikysLib;
using SPIC.Default.Displays;
using System.Collections.Generic;

namespace SPIC.Default.Infinities;

public enum GrabBagCategory {
    None,
    Container,
    Extractinator,
    TreasureBag,
}

public sealed class GrabBagRequirements {
    public Count<GrabBagCategory> Container = 10;
    public Count<GrabBagCategory> Extractinator = 499;
    public Count<GrabBagCategory> TreasureBag = 3;
}

public sealed class GrabBag : Infinity<Item, GrabBagCategory>, IConfigProvider<GrabBagRequirements>, ITooltipLineDisplay {
    public static GrabBag Instance = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance; 
    public GrabBagRequirements Config { get; set; } = null!;

    public override bool DefaultEnabled => false;
    public override Color DefaultColor => Colors.RarityDarkPurple;

    public override Requirement GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => new(Config.Container),
        GrabBagCategory.TreasureBag => new(Config.TreasureBag),
        GrabBagCategory.Extractinator => new(Config.Extractinator),
        _ => default,
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
        return GrabBagCategory.None; // GrabBagCategory.Unknown;
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed)
        => (new(Instance.Mod, item.type == ItemID.LockBox && item.type != displayed ? "Tooltip1" : "Tooltip0", Instance.DisplayName.Value), TooltipLineID.Tooltip);
    
    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? key = item.type == ItemID.LockBox ? Main.LocalPlayer.FindItemRaw(ItemID.GoldenKey) : null ;
        if (key is not null) displayed.Add(key);
    }
}

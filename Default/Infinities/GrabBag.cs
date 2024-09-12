using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Components;
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

public sealed class GrabBag : Infinity<Item>, IConfigurableComponents<GrabBagRequirements> {
    public static Customs<Item, GrabBagCategory> Customs = new(i => new(i.type));
    public static Group<Item> Group = new(() => ConsumableItem.InfinityGroup);
    public static Category<Item, GrabBagCategory> Category = new(GetRequirement, GetCategory);
    public static GrabBag Instance = null!;
    public static TooltipDisplay TooltipDisplay = new(GetTooltipLine);

    public override bool DefaultEnabled => false;
    public override Color DefaultColor => Colors.RarityDarkPurple;

    private static Optional<Requirement> GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => new(InfinitySettings.Get(Instance).Container),
        GrabBagCategory.TreasureBag => new(InfinitySettings.Get(Instance).TreasureBag),
        GrabBagCategory.Extractinator => new(InfinitySettings.Get(Instance).Extractinator),
        _ => Requirement.None,
    };

    private static Optional<GrabBagCategory> GetCategory(Item item) {
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

    public static (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed)
        => (new(Instance.Mod, item.type == ItemID.LockBox && item.type != displayed ? "Tooltip1" : "Tooltip0", Instance.DisplayName.Value), TooltipLineID.Tooltip);
    
    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? key = item.type == ItemID.LockBox ? Main.LocalPlayer.FindItemRaw(ItemID.GoldenKey) : null ;
        if (key is not null) displayed.Add(key);
    }
}

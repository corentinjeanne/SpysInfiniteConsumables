﻿using Terraria;
using Terraria.ID;

using Terraria.ModLoader.Config;

using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.Localization;

namespace SPIC.Infinities; 
public enum GrabBagCategory {
    None,
    Container,
    TreasureBag,
    Convertible,
}

// BUG capricorn leggings (tranforms)

public class GrabBagRequirements {
    [LabelKey($"${Localization.Keys.Infinties}.GrabBag.Containers")]
    public Count Containers = 10;
    [LabelKey($"${Localization.Keys.Infinties}.GrabBag.Convertibles")]
    public Count Convertibles = 499;
    [LabelKey($"${Localization.Keys.Infinties}.GrabBag.Boss")]
    public Count TreasureBags = 3;
}

public class GrabBag : InfinityStatic<GrabBag, Items, Item, GrabBagCategory> {

    public override int IconType => ItemID.FairyQueenBossBag;
    public override Color DefaultColor => Colors.RarityDarkPurple;


    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "Tooltip0", Language.GetTextValue("CommonItemTooltip.RightClickToOpen")), TooltipLineID.Tooltip); // TODO detected items opened uppon use


    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<GrabBagRequirements>(this);
    }
    public override Requirement GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => new(Config.Obj.Containers),
        GrabBagCategory.TreasureBag => new(Config.Obj.TreasureBags),
        GrabBagCategory.Convertible => new(Config.Obj.Convertibles),
        GrabBagCategory.None /* or GrabBagCategory.Unknown */ or _ => new(),
    };

    public override GrabBagCategory GetCategory(Item item) {
        switch (item.type) {
        case ItemID.Geode: return GrabBagCategory.Container;
        }
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        if (Main.ItemDropsDB.GetRulesForItemID(item.type).Count != 0) return GrabBagCategory.Container;
        if(ItemID.Sets.ExtractinatorMode[item.type] != -1) return GrabBagCategory.Convertible;
        return GrabBagCategory.None; // GrabBagCategory.Unknown;
    }

    public Wrapper<GrabBagRequirements> Config = null!;
}
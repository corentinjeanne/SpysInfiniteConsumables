using Terraria;
using Terraria.ID;

using Terraria.ModLoader.Config;

using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;

namespace SPIC.Infinities; 
public enum GrabBagCategory {
    None,
    Container,
    TreasureBag,
    Convertible,
}

public sealed class GrabBagRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.GrabBag.Containers")]
    public Count Containers = 10;
    [LabelKey($"${Localization.Keys.Infinities}.GrabBag.Convertibles")]
    public Count Convertibles = 499;
    [LabelKey($"${Localization.Keys.Infinities}.GrabBag.Boss")]
    public Count TreasureBags = 3;
}

public sealed class GrabBag : InfinityStatic<GrabBag, Items, Item, GrabBagCategory> {

    public override int IconType => ItemID.FairyQueenBossBag;
    public override Color DefaultColor => Colors.RarityDarkPurple;


    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "Tooltip0", Language.GetTextValue("CommonItemTooltip.RightClickToOpen")), TooltipLineID.Tooltip); // TODO detected items opened uppon use

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<GrabBagRequirements>(this);
    }

    public override Requirement GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => new(Config.Value.Containers),
        GrabBagCategory.TreasureBag => new(Config.Value.TreasureBags),
        GrabBagCategory.Convertible => new(Config.Value.Convertibles),
        _ => new(),
    };

    public override GrabBagCategory GetCategory(Item item) {
        switch (item.type) {
        case ItemID.Geode: return GrabBagCategory.Container;
        }
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        var drops = Main.ItemDropsDB.GetRulesForItemID(item.type);
        if (drops.Count != 0){
            if(drops.Count == 1 && drops[0] is CommonDrop commonDrop && Main.ItemDropsDB.GetRulesForItemID(commonDrop.itemId).Count == 1) return GrabBagCategory.None;
            return GrabBagCategory.Container;
        }
        if(ItemID.Sets.ExtractinatorMode[item.type] != -1) return GrabBagCategory.Convertible;
        return GrabBagCategory.None; // GrabBagCategory.Unknown;
    }

    public Wrapper<GrabBagRequirements> Config = null!;
}

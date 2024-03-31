using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using SpikysLib;
using SpikysLib.Extensions;
using SpikysLib.Configs.UI;
using System.Collections.Generic;
using SPIC.Default.Displays;

namespace SPIC.Default.Infinities; 

public enum GrabBagCategory {
    None,
    Container,
    Extractinator,
    TreasureBag,
}

public sealed class GrabBagRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.GrabBag.Container")]
    public Count Container = 10;
    [LabelKey($"${Localization.Keys.Infinities}.GrabBag.Extractinator")]
    public Count Extractinator = 499;
    [LabelKey($"${Localization.Keys.Infinities}.GrabBag.TreasureBag")]
    public Count TreasureBag = 3;
}

public sealed class GrabBag : Infinity<Item, GrabBagCategory>, ITooltipLineDisplay {

    public override Group<Item> Group => Items.Instance;
    public static GrabBag Instance = null!;
    public static Wrapper<GrabBagRequirements> Config = null!;


    public override int IconType => ItemID.FairyQueenBossBag;
    public override Color Color { get; set; } = Colors.RarityDarkPurple;

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Mod, item.type == ItemID.LockBox && item.type != displayed ? "Tooltip1" : "Tooltip0", DisplayName.Value), TooltipLineID.Tooltip);

    public override void SetStaticDefaults() {
        Config = Group.AddConfig<GrabBagRequirements>(this);
    }

    public override Requirement GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => new(Config.Value.Container),
        GrabBagCategory.TreasureBag => new(Config.Value.TreasureBag),
        GrabBagCategory.Extractinator => new(Config.Value.Extractinator),
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
        if(ItemID.Sets.ExtractinatorMode[item.type] != -1) return GrabBagCategory.Extractinator;
        return GrabBagCategory.None; // GrabBagCategory.Unknown;
    }

    public override void ModifyDisplayedConsumables(Item consumable, List<Item> displayed) {
        Item? key = consumable.type == ItemID.LockBox ? Main.LocalPlayer.FindItemRaw(ItemID.GoldenKey) : null ;
        if (key is not null) displayed.Add(key);
    }
}

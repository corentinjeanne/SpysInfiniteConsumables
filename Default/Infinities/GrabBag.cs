using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Default.Components;

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

public sealed class GrabBag : Infinity<Item, GrabBagCategory>, IConfigurableComponents<GrabBagRequirements> {
    public static Customs<Item, GrabBagCategory> Customs = new(i => new(i.type));
    public override GroupInfinity<Item> Group => Consumable.Instance;
    public static GrabBag Instance = null!;

    public override bool DefaultEnabled => false;
    public override Color DefaultColor => Colors.RarityDarkPurple;

    // public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Mod, item.type == ItemID.LockBox && item.type != displayed ? "Tooltip1" : "Tooltip0", DisplayName.Value), TooltipLineID.Tooltip);

    protected override Optional<Requirement> GetRequirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Container => new(InfinitySettings.Get(this).Container),
        GrabBagCategory.TreasureBag => new(InfinitySettings.Get(this).TreasureBag),
        GrabBagCategory.Extractinator => new(InfinitySettings.Get(this).Extractinator),
        _ => Requirement.None,
    };

    protected override Optional<GrabBagCategory> GetCategory(Item item) {
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

    // public override void ModifyDisplayedConsumables(Item consumable, List<Item> displayed) {
    //     Item? key = consumable.type == ItemID.LockBox ? Main.LocalPlayer.FindItemRaw(ItemID.GoldenKey) : null ;
    //     if (key is not null) displayed.Add(key);
    // }
}

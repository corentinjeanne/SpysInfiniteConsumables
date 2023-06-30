using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using SPIC.Configs;

namespace SPIC.VanillaGroups; 
public enum GrabBagCategory : byte {
    None = CategoryHelper.None,
    Container,
    TreasureBag,
    // Unknown = CategoryHelper.Unknown,
}

public class GrabBagRequirements {
    [LabelKey($"${Localization.Keys.Groups}.GrabBag.Containers")]
    public ItemCountWrapper Containers = new(99){Items=10};
    [LabelKey($"${Localization.Keys.Groups}.GrabBag.Boss")]
    public ItemCountWrapper TreasureBags = new(){Items=3};
}

public class GrabBag : ItemGroup<GrabBag, GrabBagCategory>, IConfigurable<GrabBagRequirements>, IDetectable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.GrabBag.Name");
    public override int IconType => ItemID.FairyQueenBossBag;

    public override Requirement<ItemCount> GetRequirement(GrabBagCategory bag, Item consumable) => bag switch {
        GrabBagCategory.Container => new CountRequirement<ItemCount>(this.Settings().Containers),
        GrabBagCategory.TreasureBag => new CountRequirement<ItemCount>(this.Settings().TreasureBags),
        GrabBagCategory.None /* or GrabBagCategory.Unknown */ or _ => new NoRequirement<ItemCount>(),
    };

    public override GrabBagCategory GetCategory(Item item) {

        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        if (Main.ItemDropsDB.GetRulesForItemID(item.type).Count != 0) return GrabBagCategory.Container;

        return GrabBagCategory.None; // GrabBagCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityDarkPurple;
    public override TooltipLineID LinePosition => TooltipLineID.Consumable;

    public bool IncludeUnknown => false;
}

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
    Crate,
    TreasureBag,
    Unknown = CategoryHelper.Unknown,
}

public class GrabBagRequirements {
    [Label($"${Localization.Keys.Groups}.GrabBag.Crates")]
    public ItemCountWrapper Crates = new(99){Items=10};
    [Label($"${Localization.Keys.Groups}.GrabBag.Boss")]
    public ItemCountWrapper TreasureBags = new(){Items=3};
}

public class GrabBag : ItemGroup<GrabBag, GrabBagCategory>, IConfigurable<GrabBagRequirements>, IDetectable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.GrabBag.Name");
    public override int IconType => ItemID.FairyQueenBossBag;

    public override bool DefaultsToOn => false;

    public override Requirement<ItemCount> Requirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Crate => new CountRequirement<ItemCount>(this.Settings().Crates),
        GrabBagCategory.TreasureBag => new CountRequirement<ItemCount>(this.Settings().TreasureBags),
        GrabBagCategory.None or GrabBagCategory.Unknown or _ => new NoRequirement<ItemCount>(),
    };

    public override GrabBagCategory GetCategory(Item item) {

        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        if (ItemID.Sets.IsFishingCrate[item.type]) return GrabBagCategory.Crate;

        return GrabBagCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityDarkPurple;
    public override TooltipLineID LinePosition => TooltipLineID.Consumable;

    public bool IncludeUnknown => false;
}

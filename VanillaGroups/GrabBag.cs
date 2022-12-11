using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using SPIC.Config;

namespace SPIC.VanillaGroups; 
public enum GrabBagCategory : byte {
    None = CategoryHelper.None,
    Crate,
    TreasureBag,
    NotSupported = CategoryHelper.NotSupported, // BUG hooks not working
    Unknown = CategoryHelper.Unknown,
}

public class GrabBagRequirements {
    [Label("$Mods.SPIC.Groups.GrabBag.crates")]
    public ItemCountWrapper Crates = new(99){Items=10};
    [Label("$Mods.SPIC.Groups.GrabBag.boss")]
    public ItemCountWrapper TreasureBags = new(){Items=3};
}

public class GrabBag : ItemGroup<GrabBag, GrabBagCategory>, IConfigurable<GrabBagRequirements>, IDetectable {
    
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.FairyQueenBossBag;

    public override bool DefaultsToOn => false;

    public override Requirement<ItemCount> Requirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Crate => new CountRequirement<ItemCount>(this.Settings().Crates),
        GrabBagCategory.TreasureBag => new CountRequirement<ItemCount>(this.Settings().TreasureBags),
        GrabBagCategory.NotSupported => new NotSupportedRequirement<ItemCount>(),
        GrabBagCategory.None or GrabBagCategory.Unknown or _ => new NoRequirement<ItemCount>(),
    };

    public override GrabBagCategory GetCategory(Item item) {

        switch (item.type){
        case ItemID.CanOfWorms or ItemID.Oyster: return GrabBagCategory.NotSupported; // tML inconsistency anf hook bug
        }
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        if (ItemID.Sets.IsFishingCrate[item.type]) return GrabBagCategory.Crate;

        return GrabBagCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityDarkPurple;
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.Groups.GrabBag.name"));
    public override TooltipLineID LinePosition => TooltipLineID.Consumable;
}

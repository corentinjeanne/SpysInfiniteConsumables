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
    Crate, // worm can
    TreasureBag,
    Unknown = CategoryHelper.Unknown
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
#nullable disable
    public GrabBagRequirements Settings { get; set; }
#nullable restore
    public override Requirement<ItemCount> Requirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Crate => new CountRequirement<ItemCount>(Settings.Crates),
        GrabBagCategory.TreasureBag => new CountRequirement<ItemCount>(Settings.TreasureBags),
        GrabBagCategory.None or GrabBagCategory.Unknown or _ => new NoRequirement<ItemCount>(),
    };

    public override GrabBagCategory GetCategory(Item item) {
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        if (ItemID.Sets.IsFishingCrate[item.type]) return GrabBagCategory.Crate;

        return GrabBagCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityDarkPurple;
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
    public override string LinePosition => "Consumable";
}

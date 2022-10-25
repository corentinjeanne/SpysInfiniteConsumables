using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;
namespace SPIC.VanillaConsumableTypes; 
public enum GrabBagCategory : byte {
    None = Category.None,
    Crate, // worm can
    TreasureBag,
    Unknown = Category.Unknown
}

public class GrabBagRequirements {
    [Label("$Mods.SPIC.Types.GrabBag.crates")]
    public ItemCountWrapper Crates = new(10, 99);
    [Label("$Mods.SPIC.Types.GrabBag.boss")]
    public ItemCountWrapper TreasureBags = new(3);
}

public class GrabBag : ConsumableType<GrabBag>, IStandardConsumableType<GrabBagCategory, GrabBagRequirements>, ICustomizable, IDetectable {
    
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.FairyQueenBossBag;

    public bool DefaultsToOn => false;
    public GrabBagRequirements Settings { get; set; }

    public IRequirement Requirement(GrabBagCategory bag) => bag switch {
        GrabBagCategory.Crate => new ItemCountRequirement(Settings.Crates),
        GrabBagCategory.TreasureBag => new ItemCountRequirement(Settings.TreasureBags),
        GrabBagCategory.None or GrabBagCategory.Unknown or _ => null,
    };


    public GrabBagCategory GetCategory(Item item) {
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;
        if (ItemID.Sets.IsFishingCrate[item.type]) return GrabBagCategory.Crate;

        return GrabBagCategory.Unknown;
    }

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityDarkPurple;
    public TooltipLine TooltipLine => TooltipHelper.AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
    public string LinePosition => "Consumable";
}

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;
public enum GrabBagCategory {
    None = ConsumableType.NoCategory,
    Crate,
    TreasureBag,
    Unkown = ConsumableType.UnknownCategory
}

public class GrabBagRequirements {
    [Label("$Mods.SPIC.Types.GrabBag.crates")]
    public Configs.Requirement Crates = 10;
    [Label("$Mods.SPIC.Types.GrabBag.boss")]
    public Configs.Requirement TreasureBags = 3;
}

public class GrabBag : ConsumableType<GrabBag>, ICustomizable, IDetectable {
    
    public override Mod Mod => SpysInfiniteConsumables.Instance;

    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Types.GrabBag.name");

    public override int MaxStack(byte bag) => (GrabBagCategory)bag switch {
        GrabBagCategory.TreasureBag => 999,
        GrabBagCategory.Crate => 99,
        GrabBagCategory.None or GrabBagCategory.Unkown or _ => 999,
    };
    public override int Requirement(byte bag) {
        GrabBagRequirements reqs = (GrabBagRequirements)ConfigRequirements;
        return (GrabBagCategory)bag switch {
            GrabBagCategory.Crate => reqs.Crates,
            GrabBagCategory.TreasureBag => reqs.TreasureBags,
            GrabBagCategory.None or GrabBagCategory.Unkown or _ => NoRequirement,
        };
    }

    public override byte GetCategory(Item item) {
        if (ItemID.Sets.BossBag[item.type]) return (byte)GrabBagCategory.TreasureBag;

        if (ItemID.Sets.IsFishingCrate[item.type])
            return (byte)GrabBagCategory.Crate;

        return (byte)GrabBagCategory.Unkown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor() => Colors.RarityDarkPurple; // new(150, 100, 255);
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("GrabBag", Terraria.Localization.Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
    public override string MissingLinePosition => "Consumable";
    public override string LocalizedCategoryName(byte category) => ((GrabBagCategory)category).ToString();

    public override GrabBagRequirements CreateRequirements() => new();
}

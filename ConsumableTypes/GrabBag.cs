using System.ComponentModel;
using Terraria;
using Terraria.ID;
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
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Crates")]
    public int Crates = 40;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Boss")]
    public int TreasureBags = 3;
}

public class GrabBag : ConsumableType<GrabBag>, ICustomizable, IDetectable {

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
    
    public override Microsoft.Xna.Framework.Color DefaultColor() => new(150, 100, 255);
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("GrabBag", Terraria.Localization.Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
    public override string MissingLinePosition => "Consumable";
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.GrabBag.{(GrabBagCategory)category}";

    public override GrabBagRequirements CreateRequirements() => new();
}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Infinities;
public enum GrabBagCategory {
    None = Infinity.NoCategory,
    Crate,
    TreasureBag,
    Unkown = Infinity.UnknownCategory
}

public class GrabBag : Infinity<GrabBag> {

    public override int MaxStack(byte bag) => (GrabBagCategory)bag switch {
        GrabBagCategory.TreasureBag => 999,
        GrabBagCategory.Crate => 99,
        GrabBagCategory.None or GrabBagCategory.Unkown or _ => 999,
    };
    public override int Requirement(byte bag) {
        Configs.Requirements inf = Configs.Requirements.Instance;
        return (GrabBagCategory)bag switch {
            GrabBagCategory.Crate => inf.bags_Crates,
            GrabBagCategory.TreasureBag => inf.bags_TreasureBags,
            GrabBagCategory.None or GrabBagCategory.Unkown or _ => Infinity.NoRequirement,
        };
    }

    public override bool Enabled => Configs.Requirements.Instance.InfiniteGrabBags;

    public override byte GetCategory(Item item) {
        // var categories = Configs.Requirements.Instance.GetCustomCategories(item.type);
        // if (categories.GrabBagCategories.HasValue) return categories.GrabBagCategories.Value;

        if (ItemID.Sets.BossBag[item.type]) return (byte)GrabBagCategory.TreasureBag;

        var autos = Configs.CategoryDetection.Instance.GetDetectedCategories(item.type);
        if (ItemID.Sets.IsFishingCrate[item.type] || autos.GrabBag)
            return (byte)GrabBagCategory.Crate;

        return (byte)GrabBagCategory.Unkown;
    }
    
    public override Microsoft.Xna.Framework.Color Color => Configs.InfinityDisplay.Instance.color_Bags;
    public override TooltipLine TooltipLine => AddedLine("GrabBag", Terraria.Localization.Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
    public override string MissingLinePosition => "Consumable";
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.GrabBag.{(GrabBagCategory)category}";
}

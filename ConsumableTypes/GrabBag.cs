using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;
public enum GrabBagCategory : byte {
    None = Category.None,
    Crate,
    TreasureBag,
    Unknown = Category.Unknown
}

public class GrabBagRequirements {
    [Label("$Mods.SPIC.Types.GrabBag.crates")]
    public Configs.Requirement Crates = 10;
    [Label("$Mods.SPIC.Types.GrabBag.boss")]
    public Configs.Requirement TreasureBags = 3;
}

public class GrabBag : ConsumableType<GrabBag>, IStandardConsumableType<GrabBagCategory, GrabBagRequirements>, ICustomizable, IDetectable {
    
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.FairyQueenBossBag;

    public bool DefaultsToOn => false;
    public GrabBagRequirements Settings { get; set; }

    public int MaxStack(GrabBagCategory bag) => bag switch {
        GrabBagCategory.TreasureBag => 999,
        GrabBagCategory.Crate => 99,
        GrabBagCategory.None or GrabBagCategory.Unknown or _ => 999,
    };
    public int Requirement(GrabBagCategory bag) {
        return bag switch {
            GrabBagCategory.Crate => Settings.Crates,
            GrabBagCategory.TreasureBag => Settings.TreasureBags,
            GrabBagCategory.None or GrabBagCategory.Unknown or _ => IConsumableType.NoRequirement,
        };
    }

    public GrabBagCategory GetCategory(Item item) {
        if (ItemID.Sets.BossBag[item.type]) return GrabBagCategory.TreasureBag;

        if (ItemID.Sets.IsFishingCrate[item.type])
            return GrabBagCategory.Crate;

        return GrabBagCategory.Unknown;
    }

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityDarkPurple;
    public TooltipLine TooltipLine => TooltipHelper.AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
    public string LinePosition => "Consumable";



}

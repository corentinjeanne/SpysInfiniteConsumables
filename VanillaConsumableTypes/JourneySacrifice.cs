using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;
namespace SPIC.VanillaConsumableTypes;
public enum JourneySacrificeCategory : byte {
    None = Category.None,
    OnlySacrifice,
    Consumable,
}
public class JourneySacrificeSettings {
    [Label("$Mods.SPIC.Types.Journey.sacrifices")]
    public bool includeNonConsumable;
}

public class JourneySacrifice : ConsumableType<JourneySacrifice>, IStandardConsumableType<JourneySacrificeCategory, JourneySacrificeSettings>{

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.GoldBunny;

    public bool DefaultsToOn => false;
    public JourneySacrificeSettings Settings { get; set; }

    public IRequirement Requirement(JourneySacrificeCategory category) => null;


    public JourneySacrificeCategory GetCategory(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(item.type, out int value) || value == 0)
            return JourneySacrificeCategory.None;

        foreach(IConsumableType type in InfinityManager.ConsumableTypes()){
            if(type != this && InfinityManager.GetRequirement(item, type.UID) is not NoRequirement) return JourneySacrificeCategory.Consumable;
        }
        return JourneySacrificeCategory.OnlySacrifice;
    }

    public IRequirement GetRequirement(Item item) {
        JourneySacrificeCategory category = item.GetCategory<JourneySacrificeCategory>(ID);
        if(category == JourneySacrificeCategory.None
                || (category == JourneySacrificeCategory.OnlySacrifice && !Settings.includeNonConsumable))
            return null;

        return new ItemCountRequirement(new(CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type], item.maxStack));
    }

    public TooltipLine TooltipLine => TooltipHelper.AddedLine("JourneyResearch", Language.GetTextValue("Mods.SPIC.Types.Journey.lineValue"));

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.JourneyMode;
}
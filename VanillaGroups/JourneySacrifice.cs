using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
namespace SPIC.VanillaGroups;
public enum JourneySacrificeCategory : byte {
    None = CategoryHelper.None,
    OnlySacrifice,
    Consumable,
}
public class JourneySacrificeSettings {
    [Label("$Mods.SPIC.Groups.Journey.sacrifices")]
    public bool includeNonConsumable;
}

public class JourneySacrifice : ItemGroup<JourneySacrifice>, IConfigurable<JourneySacrificeSettings>{

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.GoldBunny;

    public override bool DefaultsToOn => false;
    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.JourneyMode;

    public static bool IsConsumable(Item item){
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.ConsumableGroups<IConsumableGroup<Item, ItemCount>>()) {
            if (group != Instance && !group.Includes(item)) return false;
        }
        return true;
    }

    public override Requirement<ItemCount> GetRequirement(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new NoRequirement<ItemCount>();
        return IsConsumable(item) || this.Settings().includeNonConsumable ? new CountRequirement<ItemCount>(new(item){Items=CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]}) : new NoRequirement<ItemCount>();
    }

    public override bool Includes(Item consumable) => IsConsumable(consumable) || this.Settings().includeNonConsumable;

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("JourneyResearch", Language.GetTextValue("Mods.SPIC.Groups.Journey.lineValue"));

}
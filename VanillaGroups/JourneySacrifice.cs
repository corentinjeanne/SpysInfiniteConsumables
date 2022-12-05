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

    public override Requirement<ItemCount> GetRequirement(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new NoRequirement<ItemCount>();
        bool consu = false;
        foreach(IConsumableGroup<Item, ItemCount> group in InfinityManager.ConsumableGroups<IConsumableGroup<Item, ItemCount>>()){
            if(group != this && !item.GetRequirement(group).IsNone){
                consu = true;
                break;
            }
        }
        return consu || this.Settings().includeNonConsumable ? new CountRequirement<ItemCount>(new(item){Items=CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]}) : new NoRequirement<ItemCount>();
    }

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("JourneyResearch", Language.GetTextValue("Mods.SPIC.Groups.Journey.lineValue"));

}
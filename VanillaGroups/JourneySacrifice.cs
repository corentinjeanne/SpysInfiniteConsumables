using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
namespace SPIC.VanillaGroups;
public enum JourneySacrificeCategory : byte {
    None = Category.None,
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
#nullable disable
    public JourneySacrificeSettings Settings { get; set; }
#nullable restore

    public override Requirement GetRequirement(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new NoRequirement();
        bool consu = false;
        foreach(IConsumableGroup<Item> group in InfinityManager.ConsumableGroups<IConsumableGroup<Item>>()){
            if(group != this && item.GetRequirement(group) is not NoRequirement){
                consu = true;
                break;
            }
        }
        return consu || Settings.includeNonConsumable ? new CountRequirement(new ItemCount(item, CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type])) : new NoRequirement();
    }

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("JourneyResearch", Language.GetTextValue("Mods.SPIC.Groups.Journey.lineValue"));

}
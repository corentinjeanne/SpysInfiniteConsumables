using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
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

public class JourneySacrifice : ItemGroup<JourneySacrifice>, IConfigurable<JourneySacrificeSettings>{

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.GoldBunny;

    public override bool DefaultsToOn => false;
    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.JourneyMode;
    public JourneySacrificeSettings Settings { get; set; }

    // public JourneySacrificeCategory GetCategory(Item item) {
    //     if(!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(item.type, out int value) || value == 0)
    //         return JourneySacrificeCategory.None;

    //     foreach(IConsumableGroup type in InfinityManager.ConsumableTypes()){
    //         if(type != this && InfinityManager.GetRequirement(item, type.UID) is not NoRequirement) return JourneySacrificeCategory.Consumable;
    //     }
    //     return JourneySacrificeCategory.OnlySacrifice;
    // }

    public override IRequirement GetRequirement(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return null;
        bool consu = false;
        foreach(IConsumableGroup type in InfinityManager.ConsumableGroups()){
            if(type != this && item.GetRequirement(type.UID) is not NoRequirement){
                consu = true;
                break;
            }
        }
        return consu || Settings.includeNonConsumable ? new ItemCountRequirement(new(CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type], item.maxStack)) : null;
    }

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("JourneyResearch", Language.GetTextValue("Mods.SPIC.Types.Journey.lineValue"));

}
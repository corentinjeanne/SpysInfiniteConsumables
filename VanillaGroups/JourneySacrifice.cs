using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
namespace SPIC.VanillaGroups;

public class JourneySacrificeSettings {
    [Label($"${Localization.Keys.Groups}.Journey.Sacrifices")]
    public bool includeNonConsumable;
}

public class JourneySacrifice : ItemGroup<JourneySacrifice>, IConfigurable<JourneySacrificeSettings>{
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.Journey.Name");
    public override int IconType => ItemID.GoldBunny;

    public override bool DefaultsToOn => false;
    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.JourneyMode;

    public static bool IsConsumable(Item item){
        foreach (IConsumableGroup<Item, ItemCount> group in InfinityManager.ConsumableGroups<IConsumableGroup<Item, ItemCount>>()) {
            if (group != Instance && !item.GetRequirement(group).IsNone) return true;
        }
        return false;
    }

    public override Requirement<ItemCount> GetRequirement(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new NoRequirement<ItemCount>();
        return IsConsumable(item) || this.Settings().includeNonConsumable ? new CountRequirement<ItemCount>(new(item){Items=CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]}) : new NoRequirement<ItemCount>();
    }

    public override TooltipLine TooltipLine => new(Mod, InternalName, Language.GetTextValue($"{Localization.Keys.Groups}.Journey.LineValue"));

}
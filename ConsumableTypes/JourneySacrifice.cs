using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.ConsumableTypes;

public enum JourneySacrificeCategory {
    None = ConsumableType.NoCategory,
    OnlySacrifice,
    Consumable,
}
public class JourneySacrificeRequirements {
    public bool IncludeOnlySacrifices;
}

public class JourneySacrifice : ConsumableType<JourneySacrifice> {
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("JourneyResearch", "Research cost");

    public override int MaxStack(byte category) => 999;

    public override int Requirement(byte category) => -1;

    public override string CategoryKey(byte category) => ((JourneySacrificeCategory)category).ToString();
    

    public override JourneySacrificeRequirements CreateRequirements() => new();

    public override Color DefaultColor() => Colors.JourneyMode;

    public override byte GetCategory(Item item) {
        if(!CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(item.type, out int value) || value == 0)
            return (byte)JourneySacrificeCategory.None;


        foreach(int typeID in InfinityManager.EnabledTypes()){
            if(typeID != UID && InfinityManager.GetRequirement(item, typeID) != NoRequirement) return (byte)JourneySacrificeCategory.Consumable;
        }
        return (byte)JourneySacrificeCategory.OnlySacrifice;
    }

    public override int GetRequirement(Item item) {
        JourneySacrificeCategory category = (JourneySacrificeCategory)item.GetCategory(UID);
        if(category == JourneySacrificeCategory.None
                || (category == JourneySacrificeCategory.OnlySacrifice && !((JourneySacrificeRequirements)ConfigRequirements).IncludeOnlySacrifices))
            return 0;

        return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
    }
}
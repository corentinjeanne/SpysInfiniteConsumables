using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.Groups;

public class JourneySacrificeSettings {
    [LabelKey($"${Localization.Keys.Groups}.JourneySacrifice.Sacrifices")]
    public bool includeNonConsumable;
}

public class JourneySacrifice : ModGroupStatic<JourneySacrifice, ItemMG, Item> {

    public override int IconType => ItemID.GoldBunny;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.JourneyMode;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<JourneySacrificeSettings>(this);
    }

    public bool IsConsumable(Item item) {
        foreach (ModGroup<ItemMG, Item> group in MetaGroup.Groups) {
            if (group != this && !MetaGroup.GetRequirement(item, group).IsNone) return true;
        }
        return false;
    }

    public override Requirement GetRequirement(Item item) {
        if (!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new();
        return IsConsumable(item) || Config.Obj.includeNonConsumable ? new(CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]) : new();
    }

    public Wrapper<JourneySacrificeSettings> Config = null!;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "JourneyResearch", this.GetLocalizedValue("LineValue")), TooltipLineID.JourneyResearch);
}
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class JourneySacrificeSettings {
    [LabelKey($"${Localization.Keys.Infinties}.JourneySacrifice.Sacrifices")]
    public bool includeNonConsumable;
}

public class JourneySacrifice : InfinityStatic<JourneySacrifice, Items, Item> {

    public override int IconType => ItemID.GoldBunny;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.JourneyMode;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<JourneySacrificeSettings>(this);
    }

    public bool IsConsumable(Item item) {
        foreach (Infinity<Items, Item> infinity in Group.Infinities) {
            if (infinity != this && !Group.GetRequirement(item, infinity).IsNone) return true;
        }
        return false;
    }

    public override Requirement GetRequirement(Item item) {
        if (!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new();
        return IsConsumable(item) || Config.Value.includeNonConsumable ? new(CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]) : new();
    }

    public Wrapper<JourneySacrificeSettings> Config = null!;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "JourneyResearch", this.GetLocalizedValue("LineValue")), TooltipLineID.JourneyResearch);
}
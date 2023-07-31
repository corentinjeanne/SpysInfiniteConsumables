using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace SPIC.Infinities;

public enum JourneyCategory { NotConsumable, Consumable }

public sealed class JourneySacrificeSettings {
    [LabelKey($"${Localization.Keys.Infinities}.JourneySacrifice.Sacrifices")]
    public bool includeNonConsumable;
}

public sealed class JourneySacrifice : InfinityStatic<JourneySacrifice, Items, Item> {

    public override int IconType => ItemID.GoldBunny;
    public override bool DefaultsToOn => false;
    public override Color DefaultColor => Colors.JourneyMode;

    public override void Load() {
        base.Load();
        DisplayOverrides += JourneyDisplay;
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<JourneySacrificeSettings>(this);
    }


    public JourneyCategory GetCategory(Item item) {
        foreach (Infinity<Items, Item> infinity in Group.Infinities) {
            if (infinity != this && !Group.GetRequirement(item, infinity).IsNone) return JourneyCategory.Consumable;
        }
        return JourneyCategory.NotConsumable;
    }

    public override Requirement GetRequirement(Item item, List<object> extras) {
        JourneyCategory category = GetCategory(item);
        extras.Add(category);
        return category == JourneyCategory.Consumable || Config.Value.includeNonConsumable ? new(item.ResearchUnlockCount) : new();
    }

    public Wrapper<JourneySacrificeSettings> Config = null!;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "JourneyResearch", this.GetLocalizedValue("Tooltip")), TooltipLineID.JourneyResearch);
    
    public static void JourneyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        if (Main.LocalPlayer.difficulty != PlayerDifficultyID.Creative && Instance.Group.Config.UsedInfinities == 0) visibility = InfinityVisibility.Hidden;
        if(Main.CreativeMenu.GetItemByIndex(0).IsSimilar(item)) visibility = InfinityVisibility.Exclusive;
    }
}
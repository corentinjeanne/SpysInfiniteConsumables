using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace SPIC.Infinities;

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


    public bool IsConsumable(Item item) {
        foreach (Infinity<Items, Item> infinity in Group.Infinities) {
            if (infinity != this && !Group.GetRequirement(item, infinity).IsNone) return true;
        }
        return false;
    }

    public override Requirement GetRequirement(Item item, List<object> extras) {
        if (!CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(item.type)) return new();
        bool isConsumable = IsConsumable(item);
        extras.Add(isConsumable);
        return isConsumable || Config.Value.includeNonConsumable ? new(CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type]) : new();
    }

    public Wrapper<JourneySacrificeSettings> Config = null!;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) => (new(Mod, "JourneyResearch", this.GetLocalizedValue("LineValue")), TooltipLineID.JourneyResearch);
    
    public static void JourneyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        if(Main.LocalPlayer.difficulty == PlayerDifficultyID.Creative) return;
        if(Instance.Group.Config.MaxUsedInfinities == 0) visibility = InfinityVisibility.Hidden;
    }
}
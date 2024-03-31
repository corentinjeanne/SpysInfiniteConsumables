using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.ComponentModel;
using SpikysLib;
using SpikysLib.Configs.UI;

namespace SPIC.Default.Infinities;

public enum JourneyCategory { NotConsumable, Consumable }

public sealed class JourneySacrificeSettings {
    [LabelKey($"${Localization.Keys.Infinities}.JourneySacrifice.Sacrifices")]
    [DefaultValue(true)] public bool hideWhenResearched = true;
}

public sealed class JourneySacrifice : Infinity<Items, Item> {

    public static JourneySacrifice Instance = null!;
    public static Wrapper<JourneySacrificeSettings> Config = null!;


    public override int IconType => ItemID.GoldBunny;
    public override bool Enabled { get; set; } = false;
    public override Color Color { get; set; } = Colors.JourneyMode;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<JourneySacrificeSettings>(this);
        Displays.Tooltip.Instance.RegisterTooltipLine(this, GetTooltipLine);
    }

    public override Requirement GetRequirement(Item item, List<object> extras) => new(item.ResearchUnlockCount);

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Mod, "JourneyResearch", this.GetLocalizedValue("Tooltip")), TooltipLineID.JourneyResearch);

    public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        if (Main.LocalPlayer.difficulty != PlayerDifficultyID.Creative && Instance.Group.Config.UsedInfinities == 0) visibility = InfinityVisibility.Hidden;
        if(Main.CreativeMenu.GetItemByIndex(0).IsSimilar(item)) visibility = InfinityVisibility.Exclusive;
        else if(Config.Value.hideWhenResearched && Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type) == item.ResearchUnlockCount) visibility = InfinityVisibility.Hidden;
    }
}
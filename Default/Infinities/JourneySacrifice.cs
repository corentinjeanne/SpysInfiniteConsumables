using Terraria;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using SpikysLib;
using SPIC.Default.Displays;
using SPIC.Configs;

namespace SPIC.Default.Infinities;

public sealed class JourneySacrificeRequirements {
    [DefaultValue(true)] public bool hideWhenResearched = true;
}

public sealed class JourneySacrifice : Infinity<Item>, IClientConfigProvider<JourneySacrificeRequirements>, ITooltipLineDisplay {
    public static JourneySacrifice Instance = null!;
    public JourneySacrificeRequirements ClientConfig { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;

    public override bool DefaultEnabled => false;
    public override Color DefaultColor => Colors.JourneyMode;

    protected override long GetRequirementInner(Item item) => item.ResearchUnlockCount;

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Instance.Mod, "JourneyResearch", Instance.GetLocalizedValue("TooltipLine")), TooltipLineID.JourneyResearch);

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (Main.CreativeMenu.GetItemByIndex(0).IsSimilar(item)) visibility = InfinityVisibility.Exclusive;
        else if (ClientConfig.hideWhenResearched && Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type) == item.ResearchUnlockCount) visibility = InfinityVisibility.Hidden;
    }
}
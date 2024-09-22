using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SpikysLib;
using SPIC.Default.Displays;
using SPIC.Configs;

namespace SPIC.Default.Infinities;

public sealed class JourneySacrificeClient {
    public bool hideWhenResearched;
}

public sealed class JourneySacrifice : Infinity<Item>, IClientConfigProvider<JourneySacrificeClient>, ITooltipLineDisplay {
    public static JourneySacrifice Instance = null!;
    public JourneySacrificeClient ClientConfig { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;

    public sealed override InfinityDefaults Defaults => new() {
        Enabled = false,
        Color = Colors.JourneyMode
    };

    protected override long GetRequirementInner(Item item) => item.ResearchUnlockCount;

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Instance.Mod, "JourneyResearch", Instance.GetLocalizedValue("TooltipLine")), TooltipLineID.JourneyResearch);

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (Main.CreativeMenu.GetItemByIndex(0).IsSimilar(item)) visibility = InfinityVisibility.Exclusive;
        else if (ClientConfig.hideWhenResearched && Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type) == item.ResearchUnlockCount) visibility = InfinityVisibility.Hidden;
    }
}
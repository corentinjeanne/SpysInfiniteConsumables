using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SpikysLib;
using SPIC.Default.Displays;
using SPIC.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace SPIC.Default.Infinities;

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class JourneySacrificeDisplay {
    public bool hideWhenResearched;
}

public sealed class JourneySacrifice : Infinity<Item>, IClientConfigProvider<JourneySacrificeDisplay>, ITooltipLineDisplay {
    public static JourneySacrifice Instance = null!;
    public JourneySacrificeDisplay ClientConfig { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;

    public sealed override InfinityDefaults Defaults => new() {
        Enabled = false,
        Color = Colors.JourneyMode
    };

    protected override long GetRequirementInner(Item item) => item.ResearchUnlockCount;

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Instance.Mod, "JourneyResearch", Instance.GetLocalizedValue("TooltipLine")), TooltipLineID.JourneyResearch);

    protected override void ModifyDisplayedInfinity(Item item, int context, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (context == ItemSlot.Context.CreativeSacrifice) visibility = InfinityVisibility.Exclusive;
        else if (ClientConfig.hideWhenResearched && Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type) == item.ResearchUnlockCount) visibility = InfinityVisibility.Hidden;
    }
}
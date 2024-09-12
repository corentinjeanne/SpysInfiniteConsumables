using Terraria;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.CodeAnalysis;
using SPIC.Components;
using Terraria.ModLoader;
using SpikysLib;
using SPIC.Default.Displays;
using SPIC.Configs;

namespace SPIC.Default.Infinities;

public sealed class JourneySacrificeRequirements {
    [DefaultValue(true)] public bool hideWhenResearched = true;
}

public sealed class JourneySacrifice : Infinity<Item>, IClientConfigurableComponents<JourneySacrificeRequirements> {
    public static Group<Item> Group = new(() => ConsumableItem.InfinityGroup);
    public static JourneySacrifice Instance = null!;
    public static TooltipDisplay TooltipDisplay = new(GetTooltipLine);

    public override bool DefaultEnabled => false;
    public override Color DefaultColor => Colors.JourneyMode;

    protected override Optional<Requirement> GetRequirement(Item item) => new Requirement(item.ResearchUnlockCount);

    public static (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Instance.Mod, "JourneyResearch", Instance.GetLocalizedValue("TooltipLine")), TooltipLineID.JourneyResearch);

    protected override Optional<InfinityVisibility> GetVisibility(Item item) {
        if(Main.CreativeMenu.GetItemByIndex(0).IsSimilar(item)) return InfinityVisibility.Exclusive;
        else if(InfinityDisplays.Get(this).hideWhenResearched && Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type) == item.ResearchUnlockCount) return InfinityVisibility.Hidden;
        return default;
    }
}
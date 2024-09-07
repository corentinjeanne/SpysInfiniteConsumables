using Terraria;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.CodeAnalysis;

namespace SPIC.Default.Infinities;

public enum JourneyCategory { NotConsumable, Consumable }

public sealed class JourneySacrificeRequirements {
    [DefaultValue(true)] public bool hideWhenResearched = true;
}

public sealed class JourneySacrifice : Infinity<Item>, IClientConfigurableComponents<JourneySacrificeRequirements> {

    public override GroupInfinity<Item> Group => Consumable.Instance;
    public static JourneySacrifice Instance = null!;


    public override bool DefaultEnabled => false;
    public override Color DefaultColor => Colors.JourneyMode;

    protected override Optional<Requirement> GetRequirement(Item item) => new Requirement(item.ResearchUnlockCount);

    // public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) => (new(Mod, "JourneyResearch", this.GetLocalizedValue("TooltipLine")), TooltipLineID.JourneyResearch);

    // public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
    //     if(Main.CreativeMenu.GetItemByIndex(0).IsSimilar(item)) visibility = InfinityVisibility.Exclusive;
    //     else if(Config.hideWhenResearched && Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type) == item.ResearchUnlockCount) visibility = InfinityVisibility.Hidden;
    // }
}
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class JourneyResearch : Infinity<JourneyResearch> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Infinities.jouney");
    public override int IconType => 2890;

    public override bool DefaultValue => false;
}
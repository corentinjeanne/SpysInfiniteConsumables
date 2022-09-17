using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class GrabBags : Infinity<GrabBags> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Infinities.bags");
    public override int IconType => 4782;

    public override bool DefaultValue => false;

}
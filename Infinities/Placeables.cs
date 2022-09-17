using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class Placeables : Infinity<Placeables> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Infinities.placeables");
    public override int IconType => 3061;
    
    public override bool DefaultValue => false;
}
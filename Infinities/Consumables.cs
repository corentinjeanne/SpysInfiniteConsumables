using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class Consumables : Infinity<Consumables> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Infinities.consumables");
    public override int IconType => 3104;

    public override bool DefaultValue => true;
}
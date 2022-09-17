using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class Currencies : Infinity<Currencies> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Infinities.currencies");
    public override int IconType => 855;

    public override bool DefaultValue => false;
}
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public class Materials : Infinity<Materials> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string LocalizedName => Language.GetTextValue("Mods.SPIC.Infinities.materials");
    public override int IconType => 398;

    public override bool DefaultValue => false;

}
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;
public enum AmmoCategory {
    None = ConsumableType.NoCategory,
    Basic,
    Explosive,
    Special
}
public class AmmoRequirements {
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.StandardAmmo")]
    public int Standard= -4;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.SpecialAmmo")]
    public int Special = -1;
}

public class Ammo : ConsumableType<Ammo>, IAmmunition, ICustomizable {

    public override int MaxStack(byte category) => (AmmoCategory)category switch {
        AmmoCategory.Basic => 999,
        AmmoCategory.Special => 999,
        AmmoCategory.Explosive => 999,
        AmmoCategory.None or _ => 999,
    };

    public override int Requirement(byte category) {
        AmmoRequirements reqs = (AmmoRequirements)ConfigRequirements;
        return (AmmoCategory)category switch {
            AmmoCategory.Basic => reqs.Standard,
            AmmoCategory.Special or AmmoCategory.Explosive => reqs.Special,
            AmmoCategory.None or _ => NoRequirement,
        };
    }

    public bool ConsumesAmmo(Item item) => item.useAmmo > AmmoID.None;
    public Item GetAmmo(Player player, Item item)
        => player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)
            ? System.Array.Find(player.inventory, i => i.type == ammoType) : null;
    public TooltipLine AmmoLine(Item weapon, Item ammo) => TooltipHelper.AddedLine(Name + "Consumes", Language.GetTextValue($"Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name));

    public override byte GetCategory(Item item) {
        if (!item.consumable || item.ammo == AmmoID.None) return (byte)AmmoCategory.None;
        if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
            return (byte)AmmoCategory.Basic;
        return (byte)AmmoCategory.Special;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor() => new(0, 180, 60);
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Ammo", Lang.tip[34].Value);

    public override object CreateRequirements() => new AmmoRequirements();

    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.Ammo.{(AmmoCategory)category}";

}

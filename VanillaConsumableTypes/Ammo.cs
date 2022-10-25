using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;
namespace SPIC.VanillaConsumableTypes;
public enum AmmoCategory : byte {
    None = Category.None,
    Basic,
    Explosive,
    Special
}
public class AmmoRequirements {
    [Label("$Mods.SPIC.Types.Ammo.standard")]
    public ItemCountWrapper Standard = new(4.0f);
    [Label("$Mods.SPIC.Types.Ammo.special")]
    public ItemCountWrapper Special = new(1.0f);
}

public class Ammo : ConsumableType<Ammo>, IStandardConsumableType<AmmoCategory, AmmoRequirements>, IDefaultAmmunition, ICustomizable, IDetectable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.EndlessQuiver;
    
    public bool DefaultsToOn => true;

    public AmmoRequirements Settings { get; set; }

    public IRequirement Requirement(AmmoCategory category) => category switch {
        AmmoCategory.Basic => new ItemCountRequirement(Settings.Standard),
        AmmoCategory.Special or AmmoCategory.Explosive => new ItemCountRequirement(Settings.Special),
        AmmoCategory.None or _ => null,
    };

    public AmmoCategory GetCategory(Item item) {
        if (!item.consumable || item.ammo == AmmoID.None) return AmmoCategory.None;
        if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
            return AmmoCategory.Basic;
        return AmmoCategory.Special;
    }

    public bool ConsumesAmmo(Item item) => item.useAmmo > AmmoID.None;
    public Item GetAmmo(Player player, Item item)
        => player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)
            ? System.Array.Find(player.inventory, i => i.type == ammoType) : null;

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityLime;
    public TooltipLine TooltipLine => TooltipHelper.AddedLine("Ammo", Lang.tip[34].Value);

}
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;
public enum AmmoCategory : byte {
    None = IConsumableType.NoCategory,
    Basic,
    Explosive,
    Special
}
public class AmmoRequirements {
    [Label("$Mods.SPIC.Types.Ammo.standard")]
    public Configs.Requirement Standard = -4;
    [Label("$Mods.SPIC.Types.Ammo.special")]
    public Configs.Requirement Special = -1;
}

public class Ammo : ConsumableType<Ammo>, IStandardConsumableType<AmmoCategory, AmmoRequirements>, IDefaultAmmunition, ICustomizable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.EndlessQuiver;
    
    public bool DefaultsToOn => true;

    public AmmoRequirements Settings { get; set; }

    public int MaxStack(AmmoCategory category) => category switch {
        AmmoCategory.Basic => 999,
        AmmoCategory.Special => 999,
        AmmoCategory.Explosive => 999,
        AmmoCategory.None or _ => 999,
    };

    public int Requirement(AmmoCategory category) {
        return category switch {
            AmmoCategory.Basic => Settings.Standard,
            AmmoCategory.Special or AmmoCategory.Explosive => Settings.Special,
            AmmoCategory.None or _ => IConsumableType.NoRequirement,
        };
    }
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
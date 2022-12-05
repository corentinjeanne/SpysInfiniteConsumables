using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

using SPIC.Config;
using Terraria.Localization;

namespace SPIC.VanillaGroups;
public enum AmmoCategory : byte {
    None = CategoryHelper.None,
    Basic,
    Explosive,
    Special
}
public class AmmoRequirements {
    [Label("$Mods.SPIC.Groups.Ammo.standard")]
    public ItemCountWrapper Standard = new(){Stacks=4};
    [Label("$Mods.SPIC.Groups.Ammo.special")]
    public ItemCountWrapper Special = new(){Stacks=1};
}

public class Ammo : ItemGroup<Ammo, AmmoCategory>, IAlternateDisplay<Item>, IConfigurable<AmmoRequirements>, IDetectable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.EndlessQuiver;

    public override Color DefaultColor => Colors.RarityLime;

    public override Requirement<ItemCount> Requirement(AmmoCategory category) => category switch {
        AmmoCategory.Basic => new CountRequirement<ItemCount>(this.Settings().Standard),
        AmmoCategory.Special or AmmoCategory.Explosive => new CountRequirement<ItemCount>(this.Settings().Special),
        AmmoCategory.None or _ => new NoRequirement<ItemCount>(),
    };

    public override AmmoCategory GetCategory(Item weapon) {
        if (!weapon.consumable || weapon.ammo == AmmoID.None) return AmmoCategory.None;
        if (weapon.ammo == AmmoID.Arrow || weapon.ammo == AmmoID.Bullet || weapon.ammo == AmmoID.Rocket || weapon.ammo == AmmoID.Dart)
            return AmmoCategory.Basic;
        return AmmoCategory.Special;
    }

    public bool HasAlternate(Player player, Item item, [MaybeNullWhen(false)] out Item ammo) {
        ammo = item.useAmmo > AmmoID.None && player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true) ?
            System.Array.Find(player.inventory, i => i.type == ammoType) ?? null:
            null;
        return ammo is not null;
    }

    public TooltipLine AlternateTooltipLine(Item consumable, Item alternate) => TooltipHelper.AddedLine("WeaponConsumes", Language.GetTextValue("Mods.SPIC.ItemTooltip.weaponAmmo", alternate.Name));

    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Ammo", Lang.tip[34].Value);
}
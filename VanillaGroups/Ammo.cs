using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

using SPIC.Configs;
using Terraria.Localization;

namespace SPIC.VanillaGroups;
public enum AmmoCategory : byte {
    None = CategoryHelper.None,
    Basic,
    Explosive,
    Special
}
public class AmmoRequirements {
    [Label($"${Localization.Keys.Groups}.Ammo.Standard")]
    public ItemCountWrapper Standard = new(){Stacks=4};
    [Label($"${Localization.Keys.Groups}.Ammo.Special")]
    public ItemCountWrapper Special = new(){Stacks=1};
}

public class Ammo : ItemGroup<Ammo, AmmoCategory>, IConfigurable<AmmoRequirements>, IDetectable, IStandardAmmunition<Item> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.Ammo.Name");
    public override int IconType => ItemID.EndlessQuiver;

    public override Color DefaultColor => Colors.RarityLime;

    public override Requirement<ItemCount> Requirement(AmmoCategory category) => category switch {
        AmmoCategory.Basic => new CountRequirement<ItemCount>(this.Settings().Standard),
        AmmoCategory.Special or AmmoCategory.Explosive => new CountRequirement<ItemCount>(this.Settings().Special),
        AmmoCategory.None or _ => new NoRequirement<ItemCount>(),
    };

    public override AmmoCategory GetCategory(Item ammo) {

        if(ammo.type == ItemID.DD2EnergyCrystal) return AmmoCategory.Special;
        if (!ammo.consumable || ammo.ammo == AmmoID.None) return AmmoCategory.None;
        if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Rocket || ammo.ammo == AmmoID.Dart)
            return AmmoCategory.Basic;
        return AmmoCategory.Special;
    }

    public bool HasAmmo(Player player, Item item, [MaybeNullWhen(false)] out Item ammo) {
        ammo = item.useAmmo > AmmoID.None && player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true) ?
            System.Array.Find(player.inventory, i => i.type == ammoType) ?? null:
            null;
        return ammo is not null;
    }

    public TooltipLine WeaponLine(Item consumable, Item alternate) => new(Mod, "WeaponConsumes", Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.WeaponAmmo", alternate.Name));

    public override TooltipLine TooltipLine => new(Mod, "Ammo", Lang.tip[34].Value);

    public bool IncludeUnknown => false;
}
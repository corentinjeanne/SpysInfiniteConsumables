using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.ComponentModel;
using SPIC.Default.Displays;
using SpikysLib.Extensions;

namespace SPIC.Default.Infinities;
public enum AmmoCategory {
    None,
    Classic,
    // Explosive,
    // Cannon,
    Special
}
public sealed class AmmoRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.Ammo.Classic")]
    public Count Classic = 4 * 999;
    [LabelKey($"${Localization.Keys.Infinities}.Ammo.Special")]
    public Count Special = 999;
}

public sealed class Ammo : Infinity<Item, AmmoCategory>, ITooltipLineDisplay {

    public override Group<Item> Group => Items.Instance;
    public static Ammo Instance = null!;
    public static AmmoRequirements Config = null!;


    public override int IconType => ItemID.EndlessQuiver;
    public override Color Color { get; set; } = Colors.RarityLime;

    public override Requirement GetRequirement(AmmoCategory category) => category switch {
        AmmoCategory.Classic => new(Config.Classic),
        AmmoCategory.Special /*or AmmoCategory.Explosive*/ => new(Config.Special),
        _ => new(),
    };

    public override AmmoCategory GetCategory(Item ammo) {
        if(ammo.type == ItemID.DD2EnergyCrystal) return AmmoCategory.Special;
        if (!ammo.consumable || ammo.ammo == AmmoID.None) return AmmoCategory.None;
        if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Rocket || ammo.ammo == AmmoID.Dart)
            return AmmoCategory.Classic;
        return AmmoCategory.Special;
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) {
        if (displayed == item.type) return (new(Mod, "Ammo", Lang.tip[34].Value), TooltipLineID.Ammo);
        return (new(Mod, "WeaponConsumes", Lang.tip[52].Value + Lang.GetItemName(displayed)), TooltipLineID.WandConsumes);
    }

    public override void ModifyRequirement(Item consumable, ref Requirement requirement, List<object> extras) {
        base.ModifyRequirement(consumable, ref requirement, extras);
        if(requirement.Count > consumable.maxStack) requirement = new(requirement.Count, requirement.Multiplier);
    }

    public override void ModifyDisplayedConsumables(Item consumable, List<Item> displayed) {
        Item? ammo = consumable.useAmmo > AmmoID.None ? Main.LocalPlayer.ChooseAmmo(consumable) : null;
        if (ammo is not null) displayed.Add(ammo);
    }

    public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index >= 50 && 58 > index) visibility = InfinityVisibility.Exclusive;
    }
}
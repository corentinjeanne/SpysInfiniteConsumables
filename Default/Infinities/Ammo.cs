﻿using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Default.Components;

namespace SPIC.Default.Infinities;
public enum AmmoCategory {
    None,
    Classic,
    // Explosive,
    // Cannon,
    Special
}
public sealed class AmmoRequirements {
    public Count<AmmoCategory> Classic = 4 * 999;
    public Count<AmmoCategory> Special = 999;
}

public sealed class Ammo : Infinity<Item>, IConfigurableComponents<AmmoRequirements> {
    public static Customs<Item, AmmoCategory> Customs = new(i => new(i.type));
    public static Group<Item> Group = new(() => Consumable.InfinityGroup);
    public static Category<Item, AmmoCategory> Category = new(GetRequirement, GetCategory);
    public static Ammo Instance = null!;

    public override Color DefaultColor => new(34, 221, 151, 255); // Vortex

    private static Optional<Requirement> GetRequirement(AmmoCategory category) => category switch {
        AmmoCategory.Classic => new(InfinitySettings.Get(Instance).Classic),
        AmmoCategory.Special /*or AmmoCategory.Explosive*/ => new(InfinitySettings.Get(Instance).Special),
        _ => Requirement.None,
    };

    private static Optional<AmmoCategory> GetCategory(Item ammo) {
        if(ammo.type == ItemID.DD2EnergyCrystal) return AmmoCategory.Special;
        if (!ammo.consumable || ammo.ammo == AmmoID.None) return AmmoCategory.None;
        if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Rocket || ammo.ammo == AmmoID.Dart)
            return AmmoCategory.Classic;
        return AmmoCategory.Special;
    }

    // public override void ModifyRequirement(Item consumable, ref Requirement requirement, List<object> extras) {
    //     base.ModifyRequirement(consumable, ref requirement, extras);
    //     if(requirement.Count > consumable.maxStack) requirement = new(requirement.Count, requirement.Multiplier);
    // }

    // public override void ModifyDisplayedConsumables(Item consumable, List<Item> displayed) {
    //     Item? ammo = consumable.useAmmo > AmmoID.None ? Main.LocalPlayer.ChooseAmmo(consumable) : null;
    //     if (ammo is not null) displayed.Add(ammo);
    // }

    // public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
    //     int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
    //     if (index >= 50 && 58 > index) visibility = InfinityVisibility.Exclusive;
    // }
}
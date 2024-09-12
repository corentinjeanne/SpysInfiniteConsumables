using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Components;
using SpikysLib.Constants;
using System.Collections.Generic;

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
    public static Group<Item> Group = new(() => ConsumableItem.InfinityGroup);
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

    protected override void ModifyRequirement(Item consumable, ref Requirement requirement) {
        if(requirement.Count > consumable.maxStack) requirement = new(requirement.Count, requirement.Multiplier);
    }

    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? ammo = item.useAmmo > AmmoID.None ? Main.LocalPlayer.ChooseAmmo(item) : null;
        if (ammo is not null) displayed.Add(ammo);
    }

    protected override Optional<InfinityVisibility> GetVisibility(Item item) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        return InventorySlots.Coins.Start <= index && index < InventorySlots.Ammo.End ? new(InfinityVisibility.Exclusive) : default;
    }
}
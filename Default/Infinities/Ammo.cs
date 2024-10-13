using Terraria;
using Terraria.ID;
using SPIC.Configs;
using System.Collections.Generic;
using Terraria.ModLoader;
using SpikysLib;
using SpikysLib.Constants;
using SPIC.Default.Displays;
using Terraria.ModLoader.Config;
using SpikysLib.Configs.UI;

namespace SPIC.Default.Infinities;

public enum AmmoCategory {
    None,
    Classic,
    // Explosive,
    // Cannon,
    Special
}

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class AmmoRequirements {
    public Count<AmmoCategory> Classic = 4 * 999;
    public Count<AmmoCategory> Special = 999;
}

public sealed class Ammo : Infinity<Item, AmmoCategory>, IConfigProvider<AmmoRequirements>, ITooltipLineDisplay {
    public static Ammo Instance = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;
    public AmmoRequirements Config { get; set; } = null!;

    public sealed override InfinityDefaults Defaults => new() { Color = new(34, 221, 151, 255) }; // Vortex

    public override long GetRequirement(AmmoCategory category) => category switch {
        AmmoCategory.Classic => Config.Classic,
        AmmoCategory.Special /*or AmmoCategory.Explosive*/ => Config.Special,
        _ => 0,
    };

    protected override AmmoCategory GetCategoryInner(Item ammo) {
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

    protected override void ModifyRequirement(Item consumable, ref long requirement) {
        if (requirement > consumable.maxStack) requirement = consumable.maxStack;
    }

    protected override void ModifyDisplayedConsumables(Item consumable, ref List<Item> displayed) {
        Item? ammo = consumable.useAmmo > AmmoID.None ? Main.LocalPlayer.ChooseAmmo(consumable) : null;
        if (ammo is not null) displayed.Add(ammo);
    }

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (InventorySlots.Coins.Start <= index && index < InventorySlots.Ammo.End) visibility = InfinityVisibility.Exclusive;
    }
}
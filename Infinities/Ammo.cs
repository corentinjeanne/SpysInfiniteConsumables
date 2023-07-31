using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace SPIC.Infinities;
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

public sealed class Ammo : InfinityStatic<Ammo, Items, Item, AmmoCategory> {

    public override int IconType => ItemID.EndlessQuiver;
    public override Color DefaultColor => Colors.RarityLime;

    public override void Load() {
        base.Load();
        RequirementOverrides += MaxStack;
        DisplayOverrides += AmmoSlots;
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = Group.AddConfig<AmmoRequirements>(this);
    }

    public override Requirement GetRequirement(AmmoCategory category) => category switch {
        AmmoCategory.Classic => new(Config.Value.Classic),
        AmmoCategory.Special /*or AmmoCategory.Explosive*/ => new(Config.Value.Special),
        _ => new(),
    };

    public override AmmoCategory GetCategory(Item ammo) {
        if(ammo.type == ItemID.DD2EnergyCrystal) return AmmoCategory.Special; // TODO test // ? Keep
        if (!ammo.consumable || ammo.ammo == AmmoID.None) return AmmoCategory.None;
        if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Rocket || ammo.ammo == AmmoID.Dart)
            return AmmoCategory.Classic;
        return AmmoCategory.Special;
    }

    public static Wrapper<AmmoRequirements> Config = null!;

    public override Item DisplayedValue(Item consumable) => consumable.useAmmo > AmmoID.None ? Main.LocalPlayer.ChooseAmmo(consumable) ?? consumable : consumable;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) {
        Item ammo = DisplayedValue(item);
        if (ammo == item) return (new(Mod, "Ammo", Lang.tip[34].Value), TooltipLineID.Ammo);
        return (new(Mod, "WeaponConsumes", Lang.tip[52].Value + ammo.Name), TooltipLineID.WandConsumes);
    }


    public static void MaxStack(Item consumable, ref Requirement requirement, List<object> extras) {
        if(requirement.Count > consumable.maxStack){
            requirement = new(requirement.Count, requirement.Multiplier);
        }
    }
    
    public static void AmmoSlots(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index >= 50 && 58 > index) visibility = InfinityVisibility.Exclusive;
    }
}
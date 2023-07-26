using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

// TODO dd2 mana consumption

namespace SPIC.Groups;
public enum AmmoCategory {
    None,
    Basic,
    Explosive, // ? Keep ?
    Cannon, // TODO impl
    Special
}
public class AmmoRequirements {
    [LabelKey($"${Localization.Keys.Groups}.Ammo.Standard")]
    public Count Standard = 4 * 999;
    [LabelKey($"${Localization.Keys.Groups}.Ammo.Special")]
    public Count Special = 999;
}

public class Ammo : ModGroupStatic<Ammo, ItemMG, Item, AmmoCategory> {

    public override int IconType => ItemID.EndlessQuiver;
    public override Color DefaultColor => Colors.RarityLime;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<AmmoRequirements>(this);
    }

    public override Requirement GetRequirement(AmmoCategory category) => category switch {
        AmmoCategory.Basic => new(Config.Obj.Standard),
        AmmoCategory.Special or AmmoCategory.Explosive => new(Config.Obj.Special),
        AmmoCategory.None or _ => new(),
    };

    public override AmmoCategory GetCategory(Item ammo) {

        if(ammo.type == ItemID.DD2EnergyCrystal) return AmmoCategory.Special;
        if (!ammo.consumable || ammo.ammo == AmmoID.None) return AmmoCategory.None;
        if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Rocket || ammo.ammo == AmmoID.Dart)
            return AmmoCategory.Basic;
        return AmmoCategory.Special;
    }

    public Wrapper<AmmoRequirements> Config = null!;

    public override Item DisplayedValue(Item consumable) => consumable.useAmmo > AmmoID.None ? Main.LocalPlayer.ChooseAmmo(consumable) ?? consumable : consumable;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) {
        Item ammo = DisplayedValue(item);
        if (ammo == item) return (new(Mod, "Ammo", Lang.tip[34].Value), TooltipLineID.Ammo);
        return (new(Mod, "WeaponConsumes", Lang.tip[52].Value + ammo.Name), TooltipLineID.WandConsumes);
    }
}
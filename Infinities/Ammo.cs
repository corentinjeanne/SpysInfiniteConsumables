using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Infinities;
public enum AmmoCategory {
    None = Infinity.NoCategory,
    Basic,
    Explosive,
    Special
}

public class Ammo : Infinity<Ammo> {

    public override int MaxStack(byte category) => (AmmoCategory)category switch {
        AmmoCategory.Basic => 999,
        AmmoCategory.Special => 999,
        AmmoCategory.Explosive => 999,
        AmmoCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        Configs.Requirements requirements = Configs.Requirements.Instance;
        return (AmmoCategory)category switch {
            AmmoCategory.Basic => requirements.ammo_Standard,
            AmmoCategory.Special or AmmoCategory.Explosive => requirements.ammo_Special,
            AmmoCategory.None or _ => Infinity.NoRequirement,
        };
    }

    public override bool ConsumesAmmo(Item item) => item.useAmmo > AmmoID.None;
    public override Item GetAmmo(Player player, Item item)
        => player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)
            ? System.Array.Find(player.inventory, i => i.type == ammoType) : null;
    


    public override bool Enabled => Configs.Requirements.Instance.InfiniteConsumables;

    public override byte GetCategory(Item item) {
        if (!item.consumable || item.ammo == AmmoID.None) return (byte)AmmoCategory.None;
        if (Configs.CategoryDetection.Instance.GetDetectedCategories(item.type).Explosive) return (byte)AmmoCategory.Explosive;
        if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
            return (byte)AmmoCategory.Basic;
        return (byte)AmmoCategory.Special;
    }

    public override Microsoft.Xna.Framework.Color Color => Configs.InfinityDisplay.Instance.color_Ammo;
    public override TooltipLine TooltipLine => AddedLine("Ammo", Lang.tip[34].Value);
    public override string CategoryKey(byte category) => $"Mods.SPIC.Categories.Ammo.{(AmmoCategory)category}";
}

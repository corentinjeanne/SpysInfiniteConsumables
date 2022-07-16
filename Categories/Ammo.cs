using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {
    namespace Categories {
        public enum Ammo {
            None,
            Basic,
            Explosive,
            Special
        }
    }

    public static class AmmoExtension {
        public static int MaxStack(this Ammo ammo) => ammo switch {
            Ammo.Basic => 999,
            Ammo.Special => 999,
            Ammo.Explosive => 999,
            _ => 999,
        };

        public static int Requirement(this Ammo ammo) {
            Configs.Requirements inf = Configs.Requirements.Instance;
            return ammo switch {
                Ammo.Basic => inf.ammo_Standard,
                Ammo.Special or Ammo.Explosive => inf.ammo_Special,
                _ => 0,
            };
        }

        public static Ammo GetAmmoCategory(this Item item) {          

            Configs.CustomCategories categories = Configs.Requirements.Instance.GetCustomCategories(item.type); 
            if(categories.Ammo.HasValue) return categories.Ammo.Value;

            if(!item.consumable || item.ammo == AmmoID.None) return Ammo.None;
            Configs.DetectedCategories autos = Configs.CategoryDetection.Instance.GetDetectedCategories(item.type);
            if (autos.Explosive) return Ammo.Explosive;

            if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
                return Ammo.Basic;

            return Ammo.Special;
        }

        public static int GetAmmoRequirement(this Item item){
            Configs.Requirements config = Configs.Requirements.Instance;

            Configs.CustomRequirements requirements = config.GetCustomRequirements(item.type);
            if(requirements.Ammo.HasValue) return requirements.Ammo.Value;

            Ammo ammo = CategoryHelper.GetCategories(item).Ammo;
            // TODO move journey requirement sw else
            if(ammo != Ammo.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return ammo.Requirement();
        }
        public static int GetAmmoInfinity(this Player player, Item item)
            => item.GetAmmoInfinity(player.CountItems(item.type));

        public static int GetAmmoInfinity(this Item item, int count)
            => (int)CategoryHelper.CalculateInfinity(item.type, CategoryHelper.GetCategories(item).Ammo.MaxStack(), count, CategoryHelper.GetRequirements(item).Ammo, 1);
        
    }
}
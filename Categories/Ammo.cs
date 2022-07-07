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
            Configs.Infinities inf = Configs.Infinities.Instance;
            return ammo switch {
                Ammo.Basic => inf.ammo_Standard,
                Ammo.Special or Ammo.Explosive => inf.ammo_Special,
                _ => 0,
            };
        }

        public static Ammo GetAmmoCategory(this Item item) {          

            Configs.CustomCategories categories = Configs.Infinities.Instance.GetCustomCategories(item.type); 
            if(categories.Ammo.HasValue) return categories.Ammo.Value;

            if(!item.consumable || item.ammo == AmmoID.None) return Ammo.None;
            Configs.AutoCategories autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
            if (autos.Explosive) return Ammo.Explosive;

            if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
                return Ammo.Basic;

            return Ammo.Special;
        }

        public static int GetAmmoRequirement(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.Ammo.HasValue) return infinities.Ammo.Value;

            Ammo ammo = Category.GetCategories(item).Ammo;
            // TODO move journey requirement sw else
            if(ammo != Ammo.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return ammo.Requirement();
        }
        public static int GetAmmoInfinity(this Player player, Item item)
         => GetAmmoInfinity(player.CountAllItems(item.type), item);


        public static int GetAmmoInfinity(int count, Item item)
            => (int)Category.Infinity(item.type, Category.GetCategories(item).Ammo.MaxStack(), count, Category.GetRequirements(item).Ammo);
        
    }
}
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

        public static int Infinity(this Ammo ammo) {
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

        public static int GetAmmoInfinity(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.Ammo.HasValue) return Utility.InfinityToItems(infinities.Ammo.Value, item.type);
            
            Ammo ammo = Category.GetCategories(item).Ammo;
            if(ammo != Ammo.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return Utility.InfinityToItems(ammo.Infinity(), item.type, ammo.MaxStack());
        }
        public static bool HasInfiniteAmmo(this Player player, Item item)
         => IsInfiniteAmmo(player.CountAllItems(item.type), item);


        public static bool IsInfiniteAmmo(int count, Item item) => Category.IsInfinite(count,Category.GetInfinities(item).Ammo);
        
    }
}
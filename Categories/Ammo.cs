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
            Ammo.None => 999,
            _ => throw new System.NotImplementedException(),
        };

        public static int Infinity(this Ammo ammo) {
            Configs.Ammo a = Configs.Infinities.Instance.Ammos;
            return ammo switch {
                Ammo.Basic => a.Standard,
                Ammo.Special => a.Special,
                Ammo.Explosive => a.Explosives,
                Ammo.None => 0,
                _ => throw new System.NotImplementedException(),
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
            if(config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];

            Ammo ammo = Category.GetCategories(item).Ammo;
            return Utility.InfinityToItems(ammo.Infinity(), item.type, ammo.MaxStack());
        }
        public static bool HasInfiniteAmmo(this Player player, int type)
         => IsInfiniteAmmo(player.CountAllItems(type), type);


        public static bool IsInfiniteAmmo(int count, int type)
            =>  count >= Category.GetInfinities(type).Ammo;
        
    }
}
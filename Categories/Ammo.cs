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

            if (item == null) return Ammo.None;
            

            var categories = Configs.Infinities.Instance.GetCustomCategories(item.type); 
            if(categories.Ammo.HasValue) return categories.Ammo.Value;

            if(!item.consumable || item.ammo == AmmoID.None) return Ammo.None;

            var autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
            if (autos.Explosive) return Ammo.Explosive;

            if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
                return Ammo.Basic;

            return Ammo.Special;
        }
        public static bool HasInfinite(this Player player, int type, Ammo ammo)
         => HasInfinite(player.CountAllItems(type), type, ammo);


        public static bool HasInfinite(int count, int type, Ammo ammo){
            Configs.Infinities config = Configs.Infinities.Instance;

            int items;
            var infinities = config.GetCustomInfinities(type);
            if (infinities.Ammo.HasValue)
                items = Utility.InfinityToItems(infinities.Ammo.Value, type, Ammo.None.MaxStack());
            else {
                if (config.JourneyRequirement) items = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];
                else {
                    if (ammo == Ammo.None) return false;
                    items = Utility.InfinityToItems(ammo.Infinity(), type, ammo.MaxStack());
                }
            }
            return count >= items;
        }
    }
}
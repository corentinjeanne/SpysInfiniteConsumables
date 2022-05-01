using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {
    namespace Categories {
        public enum Ammo {
            None,
            Basic,
            Explosives,
            Special
        }
    }
    public static class AmmoExtension {
        public static int MaxStack(this Ammo ammo) => ammo switch {
            Ammo.Basic => 999,
            Ammo.Special => 999,
            Ammo.Explosives => 999,
            Ammo.None => 999,
            _ => throw new System.NotImplementedException(),
        };
        public static int Infinity(this Ammo ammo) {
            Configs.Ammo a = Configs.ConsumableConfig.Instance.Ammos;
            return ammo switch {
                Ammo.Basic => a.Standard,
                Ammo.Special => a.Special,
                Ammo.Explosives => a.Explosives,
                Ammo.None => 0,
                _ => throw new System.NotImplementedException(),
            };
        }
        public static Ammo GetAmmoCategory(this Item item) {

            if (item == null) return Ammo.None;

            var categories = Configs.ConsumableConfig.Instance.GetCategoriesOverride(item.type); 
            if(categories.Ammo.HasValue) return categories.Ammo.Value;

            if(!item.consumable || item.ammo == AmmoID.None) return Ammo.None;
            if (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Rocket || item.ammo == AmmoID.Dart)
                return Ammo.Basic;

            return Ammo.Special;
        }
        public static bool HasInfinite(this Player player, int type, Ammo ammo) {
            Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

            int items;
            var infinities = config.GetInfinitiesOverride(type);
            if (infinities.Ammo.HasValue)
                items = Utility.InfinityToItems(infinities.Ammo.Value, type, Ammo.None.MaxStack());
            else {
                if (config.JourneyRequirement) items = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];
                else {
                    if (ammo == Ammo.None) return false;
                    items = Utility.InfinityToItems(ammo.Infinity(), type, ammo.MaxStack());
                }
            }
            return player.CountAllItems(type) >= items;
        }
    }
}
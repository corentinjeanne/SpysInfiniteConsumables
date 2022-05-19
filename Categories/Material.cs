using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {

    namespace Categories {
        public enum Material {
            None,
            Basic,
            Ore,
            Furniture,
            Miscellaneous,
            NonStackable
        }
    }
    public static class MaterialExtension {
        public static int MaxStack(this Material category) => category switch {
            Material.Basic => 999,
            Material.Ore => 999,
            Material.Furniture => 99,
            Material.Miscellaneous => 999,
            Material.NonStackable => 1,
            Material.None => 999,
            _ => throw new System.NotImplementedException(),
        };
        public static int Infinity(this Material material) {
            Configs.Materials m = Configs.Infinities.Instance.Materials;

            return material switch {
                Material.Basic => m.Basics,
                Material.Ore => m.Ores,
                Material.Furniture => m.Furnitures,
                Material.Miscellaneous => m.Miscellaneous,
                Material.NonStackable => m.NonStackable,
                Material.None => 0,
                _ => throw new System.NotImplementedException(),
            };
        }
        public static Material GetMaterialCategory(this Item item) {

            int type = item.type;
            if (item == null || !ItemID.Sets.IsAMaterial[type]) return Material.None;

            if (Globals.SpicItem.MaxStack(type) == 1) return Material.NonStackable;

            Consumable consumable = item.GetConsumableCategory().GetValueOrDefault();

            if (consumable.IsFurniture()) return Material.Furniture;

            if(consumable == Consumable.Ore) return Material.Ore;

            if (consumable.IsCommonTile()
                    || type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow 
                    || type == ItemID.Wire || type == ItemID.BottledWater
                    || type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
                return Material.Basic;

            return Material.Miscellaneous;
        }
        public static bool HasInfinite(this Player player, int type, Material material) {
            Configs.Infinities config = Configs.Infinities.Instance;

            int items;
            if (config.JourneyRequirement) items = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type];
            else {
                if (material == Material.None) return false;
                items = Utility.InfinityToItems(material.Infinity(), type, material.MaxStack());
            }


            return player.CountAllItems(type, true) >= items;
        }
    }
}
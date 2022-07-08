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
            _ => 999,
        };
        public static int Requirement(this Material material) {
            Configs.Infinities inf = Configs.Infinities.Instance;

            return material switch {
                Material.Basic => inf.materials_Basics,
                Material.Ore => inf.materials_Ores,
                Material.Furniture => inf.materials_Furnitures,
                Material.Miscellaneous => inf.materials_Miscellaneous,
                Material.NonStackable => inf.materials_NonStackable,
                _ => 0,
            };
        }
        public static Material GetMaterialCategory(this Item item) {

            int type = item.type;
            switch (type){
            case ItemID.FallenStar: return Material.Miscellaneous;
            }
            if (item == null || !ItemID.Sets.IsAMaterial[type]) return Material.None;

            if (Globals.SpicItem.MaxStack(type) == 1) return Material.NonStackable;

            Placeable placeable = item.GetPlaceableCategory();

            if (placeable.IsFurniture()) return Material.Furniture;

            if(placeable == Placeable.Ore) return Material.Ore;

            if (placeable.IsCommonTile()
                    || type == ItemID.MusketBall || type == ItemID.EmptyBullet || type == ItemID.WoodenArrow 
                    || type == ItemID.Wire || type == ItemID.BottledWater
                    || type == ItemID.DryRocket || type == ItemID.DryBomb || type == ItemID.EmptyDropper)
                return Material.Basic;

            return Material.Miscellaneous;
        }
        public static int GetMaterialRequirement(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;
            Material material = Category.GetCategories(item).Material;
            if(material != Material.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return material.Requirement();
        }

        public static long GetMaterialInfinity(this Player player, Item item)
            => Category.Infinity(item.type, Category.GetCategories(item).Material.MaxStack(), player.CountAllItems(item.type, true), Category.GetRequirements(item).Material, 0.5f, Category.ARIDelegates.LargestMultiple);
    }
}
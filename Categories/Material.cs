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
            Configs.Requirements inf = Configs.Requirements.Instance;
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
            if(item.IsACoin) return Material.Basic;
            
            if (!ItemID.Sets.IsAMaterial[type]) return Material.None;

            if (Globals.ConsumptionItem.MaxStack(type) == 1) return Material.NonStackable;

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
            Configs.Requirements config = Configs.Requirements.Instance;
            Material material = CategoryManager.GetTypeCategories(item).Material;
            if(material != Material.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return material.Requirement();
        }

        public static int GetMaterialInfinity(this Player player, Item item)
            => item.GetMaterialInfinity(player.CountItems(item.type, true));

        public static int GetMaterialInfinity(this Item item, long count)
            => (int)CategoryManager.CalculateInfinity(item.type, CategoryManager.GetTypeCategories(item).Material.MaxStack(), count, CategoryManager.GetTypeRequirements(item).Material, 0.5f, CategoryManager.ARIDelegates.LargestMultiple);
    }
}
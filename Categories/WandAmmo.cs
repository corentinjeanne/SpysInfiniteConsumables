using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Creative;

using SPIC.Categories;
using Terraria.ID;

namespace SPIC {
    namespace Categories {
        public enum WandAmmo {
            None,
            Block,
            Mechanical,
            Wiring, // Not Placeable
        }
    }
    public static class WandAmmoExtension {

        private static readonly HashSet<int> _tiles = new();
        public static void SaveWandAmmo(int tile) { if(!_tiles.Contains(tile)) _tiles.Add(tile); }
        public static void ClearWandAmmos() => _tiles.Clear();

        public static int MaxStack(this WandAmmo wandAmmo) => wandAmmo switch {
            WandAmmo.None => 999,
            WandAmmo.Block => 999,
            WandAmmo.Wiring => 999,
            WandAmmo.Mechanical => 999,
            _ => throw new System.NotImplementedException(),
        };

        public static int Infinity(this WandAmmo wandAmmo) {
            Configs.Infinities c = Configs.Infinities.Instance;
            return wandAmmo switch {
                WandAmmo.None => 0,
                WandAmmo.Block => c.CommonTiles.Blocks,
                WandAmmo.Wiring => c.CommonTiles.Wiring,
                WandAmmo.Mechanical => c.OtherTiles.Mechanical,
                _ => throw new System.NotImplementedException(),
            };
        }

        public static WandAmmo? GetWandAmmoCategory(this Item item) {

            if (item == null) return null;


            var categories = Configs.Infinities.Instance.GetCustomCategories(item.type);
            if (categories.WandAmmo.HasValue) return categories.WandAmmo.Value;
            var autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
            if (autos.WandAmmo) return WandAmmo.Block;

            if(_tiles.Contains(item.type)) return WandAmmo.Block;
            if (item.FitsAmmoSlot() && item.mech) return item.useStyle == ItemUseStyleID.None ? WandAmmo.Wiring : WandAmmo.Mechanical;

            return null;
        }

        public static int GetWandAmmoInfinity(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.WandAmmo.HasValue) return Utility.InfinityToItems(infinities.WandAmmo.Value, item.type);
            if(config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];

            WandAmmo wandAmmo = Category.GetCategories(item).WandAmmo ?? WandAmmo.None;
            return Utility.InfinityToItems(wandAmmo.Infinity(), item.type, wandAmmo.MaxStack());
        }

        public static bool HasInfiniteWandAmmo(this Player player, int type)
            => Category.IsInfinite(player.CountAllItems(type), Category.GetInfinities(type).WandAmmo);
    }
}
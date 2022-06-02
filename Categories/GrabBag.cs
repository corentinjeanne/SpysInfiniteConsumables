using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
using Terraria.ModLoader;

namespace SPIC {
    namespace Categories {
        public enum GrabBag {
            None,
            Crate,
            TreasureBag,
        }
    }
    public static class GrabBagExtension {
        public static int MaxStack(this GrabBag bag) => bag switch {
            GrabBag.TreasureBag => 999,
            GrabBag.Crate => 99,
            GrabBag.None => 999,
            _ => throw new System.NotImplementedException(),
        };
        
        public static int Infinity(this GrabBag bag) {
            Configs.GrabBag b = Configs.Infinities.Instance.Bags;
            return bag switch {
                GrabBag.Crate => b.Crates,
                GrabBag.TreasureBag => b.TreasureBags,
                GrabBag.None => 0,
                _ => throw new System.NotImplementedException(),
            };
        }

        public static GrabBag? GetGrabBagCategory(this Item item) {
            if (item == null) return null;

            var categories = Configs.Infinities.Instance.GetCustomCategories(item.type);
            if (categories.GrabBag.HasValue) return categories.GrabBag.Value;
            var autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
            if(autos.GrabBag) return GrabBag.Crate;

            if (ItemID.Sets.BossBag[item.type] || ItemLoader.IsModBossBag(item)) return GrabBag.TreasureBag;
            if (ItemID.Sets.IsFishingCrate[item.type] || autos.GrabBag)
                return GrabBag.Crate;

            return null;
        }
        public static int GetGrabBagInfinity(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.GrabBag.HasValue) return Utility.InfinityToItems(infinities.GrabBag.Value, item.type);
            if(config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];

            GrabBag grabBag = Category.GetCategories(item).GrabBag ?? GrabBag.None;
            return Utility.InfinityToItems(grabBag.Infinity(), item.type, grabBag.MaxStack());
        }

        public static bool HasInfiniteGrabBag(this Player player, int type)
            => Category.IsInfinite(player.CountAllItems(type, true), Category.GetInfinities(type).GrabBag);

    }
}
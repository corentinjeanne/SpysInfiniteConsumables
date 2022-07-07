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
            _ => 999,
        };
        
        public static int Requirement(this GrabBag bag) {
            Configs.Infinities inf = Configs.Infinities.Instance;
            return bag switch {
                GrabBag.Crate => inf.bags_Crates,
                GrabBag.TreasureBag => inf.bags_TreasureBags,
                _ => 0,
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
        public static int GetGrabBagRequirement(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.GrabBag.HasValue) return Utility.RequirementToItems(infinities.GrabBag.Value, item.type);

            GrabBag grabBag = Category.GetCategories(item).GrabBag ?? GrabBag.None;
            if(grabBag != GrabBag.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return grabBag.Requirement();
        }

        public static int GetGrabBagInfinity(this Player player, Item item)
            => (int)Category.Infinity(item.type, Category.GetCategories(item).GrabBag?.MaxStack() ?? 999, player.CountAllItems(item.type, true), Category.GetRequirements(item).GrabBag);

    }
}
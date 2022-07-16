﻿using Terraria;
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
            Configs.Requirements inf = Configs.Requirements.Instance;
            return bag switch {
                GrabBag.Crate => inf.bags_Crates,
                GrabBag.TreasureBag => inf.bags_TreasureBags,
                _ => 0,
            };
        }

        public static GrabBag? GetGrabBagCategory(this Item item) {
            var categories = Configs.Requirements.Instance.GetCustomCategories(item.type);
            if (categories.GrabBag.HasValue) return categories.GrabBag.Value;
            var autos = Configs.CategoryDetection.Instance.GetDetectedCategories(item.type);
            if(autos.GrabBag) return GrabBag.Crate;

            if (ItemID.Sets.BossBag[item.type] || ItemLoader.IsModBossBag(item)) return GrabBag.TreasureBag;
            if (ItemID.Sets.IsFishingCrate[item.type] || autos.GrabBag)
                return GrabBag.Crate;

            return null;
        }
        public static int GetGrabBagRequirement(this Item item){
            Configs.Requirements config = Configs.Requirements.Instance;

            Configs.CustomRequirements requirements = config.GetCustomRequirements(item.type);
            if(requirements.GrabBag.HasValue) return Utility.RequirementToItems(requirements.GrabBag.Value, item.type);

            GrabBag grabBag = CategoryHelper.GetCategories(item).GrabBag ?? GrabBag.None;
            if(grabBag != GrabBag.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return grabBag.Requirement();
        }

        public static int GetGrabBagInfinity(this Player player, Item item)
            => item.GetGrabBagInfinity(player.CountItems(item.type, true));

        public static int GetGrabBagInfinity(this Item item, int count)
            => (int)CategoryHelper.CalculateInfinity(item.type, CategoryHelper.GetCategories(item).GrabBag?.MaxStack() ?? 999, count, CategoryHelper.GetRequirements(item).GrabBag, 1);

    }
}
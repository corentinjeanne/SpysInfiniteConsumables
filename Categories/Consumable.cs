﻿using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {
    namespace Categories {
        public enum Consumable {
            None,

            Weapon,
            Recovery,
            Buff,
            PlayerBooster,
            WorldBooster,

            Summoner,
            Critter,
            Explosive,
            Tool
        }
    }
    public static class ConsumableExtension {

        public static int MaxStack(this Consumable category) => category switch {
            Consumable.Weapon => 999,
            Consumable.Recovery => 99,
            Consumable.Buff => 30,

            Consumable.PlayerBooster => 99,
            Consumable.WorldBooster => 20,

            Consumable.Summoner => 20,
            Consumable.Critter => 99,
            Consumable.Explosive => 99,
            Consumable.Tool => 999,

            _ => 999,
        };

        public static int Requirement(this Consumable category) {
            Configs.Requirements c = Configs.Requirements.Instance;
            return category switch {
                Consumable.Weapon => c.consumables_Weapons,
                Consumable.Recovery or Consumable.Buff => c.consumables_Potions,
                Consumable.PlayerBooster or Consumable.WorldBooster => c.consumables_Boosters,
                
                Consumable.Summoner => c.consumables_Summoners,
                Consumable.Critter => c.consumables_Critters,
                Consumable.Tool or Consumable.Explosive => c.consumables_Tools,

                _ => 0,
            };
        }
        public static Consumable? GetConsumableCategory(this Item item) {

            var categories = Configs.Requirements.Instance.GetCustomCategories(item.type);
            if (categories.Consumable.HasValue) return categories.Consumable.Value;

            var autos = Configs.CategoryDetection.Instance.GetDetectedCategories(item.type);
            if (autos.Consumable.HasValue) return autos.Consumable;

            if (!item.consumable || item.Placeable()) return Consumable.None;

            if(item.bait != 0) return Consumable.Critter;

            if(item.useStyle == ItemUseStyleID.None) return Consumable.None;

            // Vanilla inconsitancies or special items
            switch (item.type) {
            case ItemID.FallenStar: return Consumable.None;
            case ItemID.PirateMap or ItemID.EmpressButterfly: return Consumable.Summoner;
            case ItemID.LicenseBunny or ItemID.LicenseCat or ItemID.LicenseDog: return Consumable.Critter;
            case ItemID.CombatBook: return Consumable.WorldBooster;
            }

            if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
                return Consumable.Summoner;
            
            if (item.makeNPC != NPCID.None) return Consumable.Critter;

            if (item.damage > 0) return Consumable.Weapon;

            if (item.buffType != 0 && item.buffTime != 0) return Consumable.Buff;
            if (item.healLife > 0 || item.healMana > 0 || item.potion) return Consumable.Recovery;

            if (item.shoot != ProjectileID.None)
                return autos.Explosive ? Consumable.Explosive : Consumable.Tool;
            
            if (item.hairDye != -1) return Consumable.PlayerBooster;

            // Most modded summoners, booster and non buff potions, modded liquids...
            return null;
        }

        public static int GetConsumableRequirement(this Item item){
            Configs.Requirements config = Configs.Requirements.Instance;

            Configs.CustomRequirements requirements = config.GetCustomRequirements(item.type);
            if(requirements.Consumable.HasValue) return requirements.Consumable.Value;
            
            Consumable consumable = CategoryManager.GetTypeCategories(item).Consumable ?? Consumable.Tool;
            if(consumable != Consumable.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return consumable.Requirement();
        }

        public static int GetConsumableInfinity(this Player player, Item item)
            => item.GetConsumableInfinity(player.CountItems(item.type));

        public static int GetConsumableInfinity(this Item item, int count)
            => (int)CategoryManager.CalculateInfinity(item.type, CategoryManager.GetTypeCategories(item).Consumable?.MaxStack() ?? 999, count, CategoryManager.GetTypeRequirements(item).Consumable, 1);
    }
}
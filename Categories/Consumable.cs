using Terraria;
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
            Configs.Infinities c = Configs.Infinities.Instance;
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

            var categories = Configs.Infinities.Instance.GetCustomCategories(item.type);
            if (categories.Consumable.HasValue) return categories.Consumable.Value;
            
            var autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
            if (autos.Consumable.HasValue) return autos.Consumable;

            if (!item.consumable || item.useStyle == ItemUseStyleID.None) return Consumable.None;
            if (item.createTile != -1 || item.createWall != -1) return Consumable.None;

            // Vanilla inconsitancies or special items
            switch (item.type) {
            case ItemID.FallenStar: return Consumable.None;
            case ItemID.PirateMap or ItemID.EmpressButterfly: return Consumable.Summoner;
            case ItemID.LicenseBunny or ItemID.LicenseCat or ItemID.LicenseDog: return Consumable.Critter;
            case ItemID.CombatBook: return Consumable.WorldBooster;
            }

            if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
                return Consumable.Summoner;
            
            if (item.makeNPC != NPCID.None || item.bait != 0) return Consumable.Critter;

            if (item.damage > 0) return Consumable.Weapon;

            if (item.buffType != 0 && item.buffTime != 0) return Consumable.Buff;
            if (item.healLife > 0 || item.healMana > 0 || item.potion) return Consumable.Recovery;


            if (item.shoot != ProjectileID.None){
                if(autos.Explosive) return Consumable.Explosive;
                return Consumable.Tool;
            }
            
            if (item.hairDye != -1) return Consumable.PlayerBooster;

            // Most modded summoners, booster and non buff potions, modded liquids...
            return null;
        }

        public static int GetConsumableRequirement(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.Consumable.HasValue) return infinities.Consumable.Value;
            
            Consumable consumable = Category.GetCategories(item).Consumable ?? Consumable.None;
            if(consumable != Consumable.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return consumable.Requirement();
        }

        public static int GetConsumableInfinity(this Player player, Item item)
            => GetConsumableInfinity(player.CountAllItems(item.type), item);

        public static int GetConsumableInfinity(int count, Item item)
         => (int)Category.Infinity(item.type, Category.GetCategories(item).Consumable?.MaxStack() ?? 999, count, Category.GetRequirements(item).Consumable);


    }
}
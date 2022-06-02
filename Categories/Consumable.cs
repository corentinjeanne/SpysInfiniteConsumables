﻿using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {

    namespace Categories {
        public enum Consumable {
            None,

            // Weapons
            Weapon,

            // Potions
            Recovery, // Mushroom, leasser potions bottle water, ManaPotion
            Buff, // ales

            // Boosters
            PlayerBooster,
            WorldBooster,

            // NPCs
            Summoner,
            Critter,

            // Other consumables
            Explosive,
            Tool,

            // Common tiles
            Block,
            Platform,
            Torch,
            Ore,
            Wall,

            // Furnitures
            LightSource,
            Container,
            Functional,
            CraftingStation,
            Housing,
            Decoration,
            MusicBox,

            // Other placeables
            Mechanical,
            Bucket,
            Seed
        }
    }
    public static class ConsumableExtension {

        public static bool IsTile(this Consumable category) => Consumable.Block <= category;
        public static bool IsCommonTile(this Consumable category) => category.IsTile() && category <= Consumable.Wall;
        public static bool IsFurniture(this Consumable category) => Consumable.Torch <= category  && category <= Consumable.MusicBox;

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
            Consumable.Block => 999,
            Consumable.Torch => 999,
            Consumable.Ore => 999,
            Consumable.Platform => 999,
            Consumable.Wall => 999,
            Consumable.Bucket => 99,
            Consumable.Seed => 99,
            Consumable.MusicBox => 1,
            Consumable.Container => 99,
            Consumable.LightSource => 99,
            Consumable.Housing => 99,
            Consumable.CraftingStation => 99,
            Consumable.Functional => 99,
            Consumable.Mechanical => 999,
            Consumable.Decoration => 99,
            Consumable.None => 999,
            _ => throw new System.NotImplementedException(),
        };

        public static int Infinity(this Consumable category) {
            Configs.Consumable c = Configs.Infinities.Instance.Consumables;
            Configs.CommonTiles t = Configs.Infinities.Instance.CommonTiles;
            Configs.OthersTiles o = Configs.Infinities.Instance.OtherTiles;
            Configs.Furnitures f = Configs.Infinities.Instance.Furnitures;

            return category switch {
                Consumable.Weapon => c.Weapons,
                Consumable.Recovery => c.Recovery,
                Consumable.Buff => c.Buffs,
                Consumable.PlayerBooster => c.Boosters,
                Consumable.WorldBooster => c.Boosters,
                Consumable.Summoner => c.Summoners,
                Consumable.Critter => c.Critters,
                Consumable.Explosive => c.Explosives,
                Consumable.Tool => c.Tools,

                Consumable.Block => t.Blocks,
                Consumable.Torch => t.PlatformsAndTorches,
                Consumable.Platform => t.PlatformsAndTorches,
                Consumable.Ore => t.Ores,
                Consumable.Wall => t.Walls,

                Consumable.LightSource => f.LightSources,
                Consumable.Container => f.Containers,
                Consumable.CraftingStation => f.Functional,
                Consumable.Functional => f.Functional,
                Consumable.MusicBox => f.Decorations,
                Consumable.Housing => f.Decorations,
                Consumable.Decoration => f.Decorations,

                Consumable.Bucket => o.Buckets,
                Consumable.Mechanical => o.Mechanical,
                Consumable.Seed => o.Seeds,
                Consumable.None => 0,
                _ => throw new System.NotImplementedException(),
            };
        }
        public static Consumable? GetConsumableCategory(this Item item) {

            var categories = Configs.Infinities.Instance.GetCustomCategories(item.type);
            if (categories.Consumable.HasValue) return categories.Consumable.Value;
            
            var autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
            if (autos.Consumable.HasValue) return autos.Consumable;

            if (!item.consumable || item.useStyle == ItemUseStyleID.None) return Consumable.None;



            // Vanilla inconsitancies or special items
            switch (item.type) {
            case ItemID.Actuator: return Consumable.Mechanical;
            case ItemID.PirateMap or ItemID.EmpressButterfly: return Consumable.Summoner;
            case ItemID.LicenseBunny or ItemID.LicenseCat or ItemID.LicenseDog: return Consumable.Critter;
            case ItemID.CombatBook: return Consumable.WorldBooster;
            case ItemID.Hellstone: return Consumable.Ore;
            }

            if (item.createWall != -1) return Consumable.Wall;
            if (item.createTile != -1) {

                int tileType = item.createTile;
                if (item.accessory) return Consumable.MusicBox;
                if (TileID.Sets.Platforms[tileType]) return Consumable.Platform;

                if (Main.tileAlch[tileType] || TileID.Sets.TreeSapling[tileType] || TileID.Sets.Grass[tileType]) return Consumable.Seed;
                if (Main.tileContainer[tileType]) return Consumable.Container;

                if (item.mech) return Consumable.Mechanical;
                if (Main.tileSpelunker[tileType]) return Consumable.Ore;

                if (Main.tileFrameImportant[tileType]) {

                    bool GoodTile(int t) => t == tileType;

                    if (TileID.Sets.Torch[tileType]) return Consumable.Torch;
                    if(System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTorch, GoodTile)) return Consumable.LightSource;

                    if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsChair, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTable, GoodTile))
                        return Consumable.Housing;

                    if (Globals.SpicRecipe.CraftingStations.Contains(tileType)) return Consumable.CraftingStation;

                    if (TileID.Sets.HasOutlines[tileType]) return Consumable.Functional;

                    return Consumable.Decoration;

                }
                return Consumable.Block;
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

        public static int GetConsumableInfinity(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if(infinities.Consumable.HasValue) return Utility.InfinityToItems(infinities.Consumable.Value, item.type);
            if(config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            
            Consumable consumable = Category.GetCategories(item).Consumable ?? Consumable.None;
            return Utility.InfinityToItems(consumable.Infinity(), item.type, consumable.MaxStack());
        }

        public static bool AlwaysDrop(int type) => AlwaysDrop(new Item(type));

        // TODO Update as tml updates
        // WallXxX
        // 2x5, 3x5, 3x6
        // Sunflower, Gnome
        // Chest
        // drop in 2x1 bug : num instead of num3
        public static bool AlwaysDrop(this Item item) {
            if (item.createTile < TileID.Dirt || item.createWall >= WallID.None || item.createTile == TileID.TallGateClosed) return false;
            if (item.createTile == TileID.GardenGnome || item.createTile == TileID.Sunflower || TileID.Sets.BasicChest[item.createTile]) return true;

            TileObjectData data = TileObjectData.GetTileData(item.createTile, item.placeStyle);

            // No data or 1x1 moditem
            if (data == null || (item.ModItem != null && data.Width > 1 && data.Height > 1)) return false;
            if ((data.Width == 2 && data.Height == 1) || (data.Width == 2 && data.Height == 5) || (data.Width == 3 && data.Height == 4) || (data.Width == 3 && data.Height == 5) || (data.Width == 3 && data.Height == 6)) return true;

            return data.AnchorWall || (TileID.Sets.HasOutlines[item.createTile] && System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, t => t == item.createTile));

        }

        public static bool HasInfiniteConsumable(this Player player, int type, bool ignoreAllwaysDrop = false)
            => IsInfiniteConsumable(player.CountAllItems(type), type, ignoreAllwaysDrop);

        public static bool IsInfiniteConsumable(int count, int type, bool ignoreAllwaysDrop = false) => Category.IsInfinite(
            count, Category.GetInfinities(type).Consumable,
            Configs.Infinities.Instance.PreventItemDupication && !ignoreAllwaysDrop
                && (Main.netMode != NetmodeID.SinglePlayer || AlwaysDrop(type))
        );


    }
}
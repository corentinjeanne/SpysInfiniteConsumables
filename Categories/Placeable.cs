using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.GameContent.Creative;

using SPIC.Categories;
namespace SPIC {

    namespace Categories {
        public enum Placeable{
            None,

            Block,
            Wall,
            Wiring,
            Torch,
            Ore,
            Gem,

            LightSource,
            Container,
            Functional,
            CraftingStation,
            Decoration,
            MusicBox,

            Mechanical,
            Liquid,
            Seed,
            Paint,
        }
    }
    public static class PlaceableExtension {

        public static bool IsCommonTile(this Placeable category) => category < Placeable.LightSource;
        public static bool IsFurniture(this Placeable category) => !category.IsCommonTile() && category < Placeable.Mechanical;
        public static bool IsMisc(this Placeable category) => !category.IsCommonTile() && !category.IsFurniture();
        
        public static int MaxStack(this Placeable category) => category switch {
            Placeable.Block => 999,
            Placeable.Wall => 999,
            Placeable.Torch => 999,
            Placeable.Ore => 999,
            Placeable.Gem => 999,
            Placeable.Wiring => 999,

            Placeable.LightSource => 99,
            Placeable.Container => 99,
            Placeable.CraftingStation => 99,
            Placeable.Functional => 99,
            Placeable.Decoration => 99,
            Placeable.MusicBox => 1,

            Placeable.Mechanical => 999,
            Placeable.Liquid => 99,
            Placeable.Seed => 99,

            Placeable.Paint => 999,

            _ => 999,
        };

        public static int Requirement(this Placeable category) {
            Configs.Infinities infs = Configs.Infinities.Instance;
            return category switch {
                Placeable.Block or Placeable.Wall or Placeable.Wiring => infs.placeables_Tiles,
                Placeable.Torch => infs.placeables_Torches,
                Placeable.Ore => infs.placeables_Ores,

                Placeable.LightSource or Placeable.MusicBox
                        or Placeable.Functional or Placeable.Decoration
                        or Placeable.Container or Placeable.CraftingStation
                    => infs.placeables_Furnitures,
                
                Placeable.Liquid => infs.placeables_Liquids,
                Placeable.Mechanical => infs.placeables_Mechanical,
                Placeable.Seed => infs.placeables_Seeds,
                Placeable.Paint => infs.placeables_Paints,
                _ => 0,
            };
        }

        private static readonly System.Collections.Generic.Dictionary<int, Placeable> _wands = new();
        public static void RegisterWandAmmo(Item wand) => RegisterWandAmmo(wand.tileWand, GetPlaceableCategory(wand, true));
        public static void RegisterWandAmmo(int type, Placeable category) => _wands.TryAdd(type, category);

        public static bool IsWandAmmo(int type) => IsWandAmmo(type, out _);
        public static bool IsWandAmmo(int type, out Placeable placeable) => _wands.TryGetValue(type, out placeable);
        public static void ClearWandAmmos() => _wands.Clear();


        public static Placeable GetPlaceableCategory(this Item item, bool checkWandAmmo = false) {

            if (!checkWandAmmo) {
                Configs.CustomCategories categories = Configs.Infinities.Instance.GetCustomCategories(item.type);
                if (categories.Placeable.HasValue) return categories.Placeable.Value;
                Configs.AutoCategories autos = Configs.CategorySettings.Instance.GetAutoCategories(item.type);
                if (autos.Placeable.HasValue) return autos.Placeable.Value;

                if(item.paint != 0) return Placeable.Paint;

                if (!IsWandAmmo(item.type) && (!item.consumable || item.useStyle == ItemUseStyleID.None))
                    return Placeable.None;
            }

            switch (item.type) {
            case ItemID.Hellstone: return Placeable.Ore;
            }

            if (item.createWall != -1) return Placeable.Wall;
            if (item.createTile != -1) {

                int tileType = item.createTile;
                if (item.accessory) return Placeable.MusicBox;
                if (TileID.Sets.Platforms[tileType]) return Placeable.Block;

                if (Main.tileAlch[tileType] || TileID.Sets.TreeSapling[tileType] || TileID.Sets.Grass[tileType]) return Placeable.Seed;
                if (Main.tileContainer[tileType]) return Placeable.Container;

                if (item.mech) return Placeable.Mechanical;

                if (Main.tileFrameImportant[tileType]) {
                    bool GoodTile(int t) => t == tileType;

                    if (TileID.Sets.Torch[tileType]) return Placeable.Torch;
                    if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTorch, GoodTile)) return Placeable.LightSource;

                    if (Globals.SpicRecipe.CraftingStations.Contains(tileType)) return Placeable.CraftingStation;

                    if (System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsChair, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, GoodTile) || System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsTable, GoodTile))
                        return Placeable.Functional;

                    if (TileID.Sets.HasOutlines[tileType]) return Placeable.Functional;

                    return Placeable.Decoration;
                }

                if (Main.tileSpelunker[tileType]) return Placeable.Ore;
                
                return Placeable.Block;
            }

            if (!checkWandAmmo && IsWandAmmo(item.type, out Placeable ammo)) return ammo;

            return Placeable.None;
        }

        public static int GetPlaceableRequirement(this Item item){
            Configs.Infinities config = Configs.Infinities.Instance;

            Configs.CustomInfinities infinities = config.GetCustomInfinities(item.type);
            if (infinities.Placeable.HasValue) return infinities.Placeable.Value;

            Placeable placeable = Category.GetCategories(item).Placeable;
            if (placeable != Placeable.None && config.JourneyRequirement) return CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
            return placeable.Requirement();
        }

        public static bool CanNoDuplicationWork(int type) => CanNoDuplicationWork(new Item(type));
        public static bool CanNoDuplicationWork(Item item = null) => Main.netMode == NetmodeID.SinglePlayer && (item == null || !AlwaysDrop(item));

        public static bool AlwaysDrop(int type) => AlwaysDrop(new Item(type));

        // TODO Update as tml updates
        // Wires and actuators
        // Wall XxX
        // 2x5, 3x5, 3x6
        // Sunflower, Gnome
        // Chest
        // drop in 2x1 bug : num instead of num3
        public static bool AlwaysDrop(this Item item) {
            if(item.type == ItemID.Wire || item.type == ItemID.Actuator) return true;
            if (item.createTile < TileID.Dirt || item.createWall != WallID.None || item.createTile == TileID.TallGateClosed) return false;
            if (item.createTile == TileID.GardenGnome || item.createTile == TileID.Sunflower || TileID.Sets.BasicChest[item.createTile]) return true;

            TileObjectData data = TileObjectData.GetTileData(item.createTile, item.placeStyle);

            // No data or 1x1 moditem
            if (data == null || (item.ModItem != null && data.Width > 1 && data.Height > 1)) return false;
            if ((data.Width == 2 && data.Height == 1) || (data.Width == 2 && data.Height == 5) || (data.Width == 3 && data.Height == 4) || (data.Width == 3 && data.Height == 5) || (data.Width == 3 && data.Height == 6)) return true;

            return data.AnchorWall || (TileID.Sets.HasOutlines[item.createTile] && System.Array.Exists(TileID.Sets.RoomNeeds.CountsAsDoor, t => t == item.createTile));
        }

        public static int GetPlaceableInfinity(this Player player, Item item, bool ignoreAllwaysDrop = false)
            => GetPlaceableInfinity(player.CountAllItems(item.type), item, ignoreAllwaysDrop);

        public static int GetPlaceableInfinity(int count, Item item, bool ignoreAllwaysDrop = false)
         => (int)Category.Infinity(item.type, Category.GetCategories(item).Placeable.MaxStack(), count, Category.GetRequirements(item).Placeable, 1,
            !(Configs.Infinities.Instance.PreventItemDupication || ignoreAllwaysDrop) && !CanNoDuplicationWork(item) ? Category.ARIDelegates.NotInfinite : Category.ARIDelegates.ItemCount
        );

    }
}
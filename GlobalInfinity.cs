using Microsoft.Xna.Framework;
using System.Collections.Generic;

using Terraria;
using Terraria.ObjectData;
using Terraria.ModLoader;
using Terraria.ID;

namespace SPIC {
    
    public class InfiniteConsumables : GlobalItem {

        public static List<int> bucketTypes = new List<int>();

        public static List<int> wandTiles = new List<int>();
        public static List<int> wiring = new List<int>();

        private static bool doOnce = false;
        public override void SetDefaults(Item item) {
            if(!doOnce){
                doOnce = true;
                wiring.Add(ItemID.Wire);
                wiring.Add(ItemID.Actuator);
            }
            // Block category for tiles paced by wands
            if(item.tileWand != -1){ // Activates on the wand only
                ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
                // mod.Logger.Debug("Wand tile:" + item.Name);
                if(!wandTiles.Contains(item.tileWand)) wandTiles.Add(item.tileWand);
            }

            // Liquids detection
            if(item.Name.ToLower().Contains("bucket")){
                if(!bucketTypes.Contains(item.type)) {
                    bucketTypes.Add(item.type);
                    // mod.Logger.Debug($"added {item.Name} to buclet list, {item.shoot}, {item.createTile}");
                }
            }

        }

        public override bool ConsumeItem(Item item, Player player) {
            mod.Logger.Debug($"Item {item.Name} consumed");
            return !Utilities.IsInfinite(item, player);
        }
        
        public override bool ConsumeAmmo(Item item, Player player) {
            if(Utilities.GetCategory(item) == ConsumableCategory.Blacklist) return true; // doesn't do the call for the weapon
            // mod.Logger.Debug($"Ammo {item.Name} consumed");
            
            bool inf = Utilities.IsInfinite(item, player);
            NoProjectileDup.lastSpawnedProjectiles = new KeyValuePair<Player, bool>(player, inf);
            return !inf;
        }

        public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            // mod.Logger.Debug("Shoot by " + item.Name);

            if(Utilities.GetCategory(item) == ConsumableCategory.Blacklist) return true; // doesn't do it for the gun
            bool inf = Utilities.IsInfinite(item, player);
            NoProjectileDup.lastSpawnedProjectiles = new KeyValuePair<Player, bool>(player, inf);
            return true;
        }

        public override bool OnPickup(Item item, Player player){
            
            if(wiring.Contains(item.type) && ModContent.GetInstance<ConsumableConfig>().PreventItemDupication && Utilities.IsInfinite(item, player)
                    && (player.HeldItem.type == ItemID.WireCutter || player.HeldItem.type == ItemID.WireKite)){
                item.stack=0;
            }
            return base.OnPickup(item, player);
        }

    }

    public class InfiniteLiquids : ModPlayer {
        private int[] buckets;
        private int emptyBucket;
        public override void PostUpdate(){
            if(buckets == null) SetBucketArrays();

            int empty = emptyBucket;
            int[] count = (int[])buckets.Clone(); // old & new values to compare
            UpdateBucketCount();

            int delta = emptyBucket - empty;
            if(!(delta == 1 || delta == -1)) return; // no item use

            for (int i = 0; i < buckets.Length; i++) {

                if(delta * (buckets[i] - count[i]) == -1){ // Use a bucket

                    if(Utilities.IsInfinite(player.HeldItem, player)){
                        player.HeldItem.stack++;
                        if(ModContent.GetInstance<ConsumableConfig>().PreventItemDupication){
                            Utilities.RemoveFromInventory(player,player.HeldItem.type == ItemID.EmptyBucket ? player.HeldItem.type : ItemID.EmptyBucket);
                        }
                        UpdateBucketCount();
                    }
                    break;
                }
            }
        }
        public void SetBucketArrays(){
            buckets = new int[InfiniteConsumables.bucketTypes.Count];
            // mod.Logger.Debug($"Created array of {buckets.Length} ints");
            UpdateBucketCount();
        }
        public void UpdateBucketCount(){
            for (int i = 0; i < buckets.Length; i++)
                buckets[i] = Utilities.TotalInInventory(player, InfiniteConsumables.bucketTypes[i]);
            emptyBucket = Utilities.TotalInInventory(player, ItemID.EmptyBucket);
        }
    }

    public class NoTileDup : GlobalTile {

        public static List<int> FurnitureList {get; private set;}

        public override void SetDefaults(){
            FurnitureList = new List<int>();
            Tile tile = new Tile();
            TileObjectData data;
            for (ushort i = 0; i < TileLoader.TileCount; i++){
                tile.ResetToType(i);
                data = TileObjectData.GetTileData(tile);
                if(data == null) continue;
                if(data.AnchorBottom.tileCount != 0 || data.AnchorTop.tileCount != 0 || data.AnchorLeft.tileCount != 0
                        || data.AnchorRight.tileCount != 0 || data.AnchorWall)
                    FurnitureList.Add(i);
            }
        }

        private List<Vector2> noDropTilesCoords = new List<Vector2>();

        public override void PlaceInWorld(int i, int j, Item item) {
            // mod.Logger.Debug("Place " + item.Name);
            if(ModContent.GetInstance<ConsumableConfig>().PreventItemDupication){
                bool inf = Utilities.IsInfinite(item, Main.player[item.owner]);
                if(inf) noDropTilesCoords.Add(new Vector2(i,j));
            }
        }
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem){
            if(!fail && noDropTilesCoords.Contains(new Vector2(i,j))){
                noDropTilesCoords.Remove(new Vector2(i,j));
                if(!noItem)noItem = ModContent.GetInstance<ConsumableConfig>().PreventItemDupication;
            }
        }
    }

    public class NoWallDup : GlobalWall {
    
        private Dictionary<Vector2, bool> noWallDrop = new Dictionary<Vector2, bool>();
        public override void PlaceInWorld(int i, int j, int type, Item item) {
            if(ModContent.GetInstance<ConsumableConfig>().PreventItemDupication){
                bool inf = Utilities.IsInfinite(item, Main.player[item.owner]);
                if(inf) noWallDrop.Add(new Vector2(i,j), false);
            }

        }
        public override void KillWall(int i, int j, int type, ref bool fail){
            if(!fail && noWallDrop.ContainsKey(new Vector2(i,j)))
                noWallDrop[new Vector2(i,j)] = true; // removed when next drop is called
        }

        public override bool Drop(int i, int j, int type, ref int dropType) { 
            if(!noWallDrop.ContainsKey(new Vector2(i,j))) return true;
            if(noWallDrop[new Vector2(i,j)]) // Remove the key if the wall has been destoyed
                noWallDrop.Remove(new Vector2(i,j));
            return !ModContent.GetInstance<ConsumableConfig>().PreventItemDupication;

        }

    }

    public class NoProjectileDup : GlobalProjectile {
        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;

        public static KeyValuePair<Player,  bool> lastSpawnedProjectiles = new KeyValuePair<Player, bool>();

        private KeyValuePair<Player,  bool> potentialOwner = new KeyValuePair<Player, bool>();
        
        public override void SetDefaults(Projectile projectile){
            potentialOwner = lastSpawnedProjectiles; // Need to be done because projectile.owner is not set
        }

        public override bool PreKill(Projectile projectile, int timeLeft){
            if(projectile.noDropItem || !ModContent.GetInstance<ConsumableConfig>().PreventItemDupication || potentialOwner.Key == null)
                return true;
            if(Main.player[projectile.owner] == potentialOwner.Key)
                projectile.noDropItem = potentialOwner.Value;

            return true;
        }
    }
}
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace SPIC {

	public enum ConsumableCategory {
		Blacklist,

		Thrown,
		Recovery,
		Buff,
		Ammo,
		SpecialAmmo,
		Summoner,
		Critter,

		Block,
		Furniture,
		Wall,
		Liquid, // TODO

		Other,
		Custom

	}

	public class ConsumableStack {
		public const int NotConsumables = 1;
		public const int Throwns = 999;
		public const int Healings = 30;
		public const int Buffs = 30;
		public const int Ammos = 999;
		public const int SpecialAmmos = 999;
		public const int Summoners = 20;
		public const int Critters = 999;
		public const int Blocks = 999;
		public const int Walls = 999;
		public const int Furnitures = 99;
		public const int Liquids = 99;
		public const int Others = 999;
		public const int Max = 999;
		public static int Assumption(ConsumableCategory category){
			switch (category){
			case ConsumableCategory.Blacklist:   return NotConsumables;
			case ConsumableCategory.Thrown:      return Throwns;
			case ConsumableCategory.Recovery:     return Healings;
			case ConsumableCategory.Buff:        return Buffs;
			case ConsumableCategory.Ammo:        return Ammos;
			case ConsumableCategory.SpecialAmmo: return SpecialAmmos;
			case ConsumableCategory.Summoner:    return Summoners;
			case ConsumableCategory.Critter:     return Critters;
			case ConsumableCategory.Block:       return Blocks;
			case ConsumableCategory.Furniture:   return Furnitures;
			case ConsumableCategory.Wall:        return Walls;
			case ConsumableCategory.Liquid:      return Liquids;
			default:                             return Others;
			}
		}
	}

	public class Utilities {
		public static Mod mod;

        public static bool ModifiedMaxStack {get; private set;}

		public static int MaxStack(Item item){
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
			if(ModifiedMaxStack || item.maxStack > 999){ // stack modifier mod installed
				ModifiedMaxStack = true;
				foreach(WierdStack ws in config.wierdStacks){
					if(ws.Item().Type == item.type) return ws.Stack();
				}
				return ConsumableStack.Assumption(GetCategory(item, true));
			}
			return item.maxStack;
		}
		public static int CountToItem(int count, Item item)
			=> count > 0 ? count * MaxStack(item): -count;
		
		public static ConsumableCategory CategoryFromName(string name){
			string lowName = name.ToLower();
			for (int i = 0; i < (int)ConsumableCategory.Custom + 1; i++){
				string catName = $"{(ConsumableCategory)i}".ToLower();
				if(lowName == catName) return (ConsumableCategory)i;
			}
			throw new UsageException("Wrong category: " + name);
		}

		public static int NameToType(string name, bool noCaps = true){
			string fullName = name.Replace("_", " ");
			if(noCaps) fullName = fullName.ToLower();
			for (var k = 0; k < ItemLoader.ItemCount; k++) {
				string testedName = noCaps ? Lang.GetItemNameValue(k).ToLower() : Lang.GetItemNameValue(k);
				if (fullName == testedName) {
					return k;
				}
			}
			throw new UsageException("Invalid Name" + name);
		}
		public static void RemoveFromInventory(Player player, int type, int count = 1){
            foreach(Item i in player.inventory){
                if(i.type != type) continue;
                i.stack-=count;
                return;
            }
        }
		public static int TotalInInventory(Player player, Item item){
			return TotalInInventory(player, item.type);
		}

		public static int TotalInInventory(Player player, int type){
			int total = 0;
			foreach (Item i in player.inventory){
				if(i.type == type) total += i.stack;
			}
			return total;
		}

		public static ConsumableCategory GetCategory(int type, bool ignoreCustoms = false){
			Item item = new Item();
			item.SetDefaults(type);
			return GetCategory(item,ignoreCustoms);
		}
		public static ConsumableCategory GetCategory(Item item, bool ignoreCustoms = false){
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
			if(!ignoreCustoms){ // Custom category
				foreach(VeryCustomInfinity c in config.customCustoms){
					if(c.Item.Type == item.type)                        return ConsumableCategory.Custom;
				}
				foreach(CustomInfinity c in config.customValues){
					if(c.Item.Type == item.type)                        return c.Category;
				}
			}

			// Auto category
			if(InfiniteConsumables.wandTiles.Contains(item.type))       return ConsumableCategory.Block;
			if(InfiniteConsumables.wiring.Contains(item.type))          return ConsumableCategory.Block;
			if(InfiniteConsumables.bucketTypes.Contains(item.type))     return ConsumableCategory.Liquid;
			if(!item.consumable)                                        return ConsumableCategory.Blacklist;
			if(item.buffType != 0 && item.buffTime != 0)                return ConsumableCategory.Buff;
			if(item.thrown)                                             return ConsumableCategory.Thrown;
			if(item.healLife != 0 || item.healMana != 0)                return ConsumableCategory.Recovery;
			if(item.ammo != AmmoID.None){
				if(item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Dart || item.ammo == AmmoID.Rocket)
																		return ConsumableCategory.Ammo;
				else                                                    return ConsumableCategory.SpecialAmmo;
			}
			if((0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] < 17)
					|| item.DD2Summon) 
										                                return ConsumableCategory.Summoner;
			if(item.makeNPC != NPCID.None || item.bait != 0)            return ConsumableCategory.Critter;
			
			
			if(item.createTile != -1){
				if(!NoTileDup.FurnitureList.Contains(item.createTile))  return ConsumableCategory.Block;
				else                                                    return ConsumableCategory.Furniture;
			}
			if(item.createWall != -1)                                   return ConsumableCategory.Wall;


			return                                                             ConsumableCategory.Other;
		}
		public static bool IsInfinite(int type, Player player) {
			Item item = new Item();
			item.SetDefaults(type);
			return IsInfinite(item, player);
		}
		public static bool IsInfinite(Item item, Player player) {
			ConsumableCategory category = GetCategory(item);
			int total = TotalInInventory(player, item);
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			// mod.Logger.Debug($"{player.name}: {total} {item.Name} in inventory ({category})");

			switch (category) {
			case ConsumableCategory.Blacklist: 
				return false;
			case ConsumableCategory.Thrown:
				return total >= CountToItem(config.thrown, item) && config.InfiniteConsumables;
			case ConsumableCategory.Buff:
				return total >= CountToItem(config.buffPotions, item) && config.InfiniteConsumables;
			case ConsumableCategory.Ammo:
				return total >= CountToItem(config.ammunitions, item) && config.InfiniteConsumables;
			case ConsumableCategory.SpecialAmmo:
				return total >= CountToItem(config.specialAmmunitions, item) && config.InfiniteConsumables;
			case ConsumableCategory.Summoner: // Not Working
			    return total >= CountToItem(config.summoning, item) && config.InfiniteConsumables;
			case ConsumableCategory.Critter: // To keep ?
				return !config.PreventItemDupication && item.bait == 0 ? total >= CountToItem(config.critters, item) && config.InfiniteTiles :
					total == CountToItem(config.critters, item) && config.InfiniteTiles; // Prevent item dupplication
			case ConsumableCategory.Block:
				return total >= CountToItem(config.blocks, item) && config.InfiniteTiles;
			case ConsumableCategory.Furniture:
			    return !config.PreventItemDupication ? total >= CountToItem(config.furnitures, item) && config.InfiniteTiles :
					total == CountToItem(config.furnitures, item) && config.InfiniteTiles; // Prevent item dupplication
			case ConsumableCategory.Wall:
				return total >=CountToItem( config.walls, item) && config.InfiniteTiles;
			case ConsumableCategory.Liquid:
				return total >= CountToItem(config.liquids, item) && config.InfiniteTiles;
			default: // ConsumableCategory.Other
				return total >= CountToItem(config.others, item) && config.InfiniteConsumables;
			}
		}
	}
}
using System.Collections.Generic;
using System.ComponentModel;

using System.IO;
using Newtonsoft.Json;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.Globals;

namespace SPIC.Config {

	public class CustomCategory {

		public ItemDefinition Item;
		public ConsumableConfig.Category Category;
		public CustomCategory() { }
		public CustomCategory(ItemDefinition item, ConsumableConfig.Category category) {
			Item = item; Category = category;
		}
		public override string ToString() => $"{Item}: {Category}";
	}

	public class CustomInfinity {

		public ItemDefinition Item;

		[Range(0, 50000)]
		public int Requirement; // in items
		public CustomInfinity() { }
		public CustomInfinity(ItemDefinition item, int requirement) {
			Item = item; Requirement = requirement;
		}
		public override string ToString() => $"{Item}: {Requirement} items";
	}

	public class ConsumableConfig : ModConfig {

		public enum Category {
			Blacklist,

			Weapon,
			Recovery,
			Buff,
			Booster,
			Summoner,
			Critter,

			Block,
			Furniture,
			Wiring,
			Wall,
			Liquid,

			Ammo,
			Special,

			Bags,
			Tools,
		}
		internal static int CategoryCount = (int)Category.Tools + 1;
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static string CommandCustomPath { get; private set; }
		internal bool modifiedInGame = false;

		public override void OnLoaded() {
			CommandCustomPath = ConfigManager.ModConfigPath + @"\spic_ConsumableConfig.json";
		}
		public void ManualSave() {
			using StreamWriter sw = new(CommandCustomPath);
			string serialisedConfig = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
			sw.Write(serialisedConfig);
			modifiedInGame = false;
		}


		[Header("General")]
		[DefaultValue(true), Label("[i:3104] Infinite Consumables")]
		public bool InfiniteConsumables;
		[Label("[i:3061] Infinite Tiles")]
		public bool InfiniteTiles;
		[Label("[i:398] Infinite Crafting"), Tooltip("Does nothing for now")]
		public bool InfiniteCrafting;

		[DefaultValue(true), Label("[i:1293] Prevent item duplication"), Tooltip(
@"/!\ WIP for multiplayer /!\
Tiles and walls won't drop their item when broken
Critters will turn into smoke
Buckets won't create empty or full buckets when used")]
		public bool PreventItemDupication;

		[DefaultValue(false), Label("[i:2890] Journey requirement"), Tooltip("Uses the journey research cost of items as their infinite requirent")]
		public bool JourneyRequirement;
		[DefaultValue(true), ReloadRequired, Label("[i:3617] Commands"), Tooltip( "Adds the '/spic' command to edit the category of items in-game")]
		public bool Commands;


		[Header("Consumables")]
		[Range(-50, 999), DefaultValue(-1), Label("[i:279] Thrown weapons")]
		public int thrown;
		[Range(-50, 999), DefaultValue(-4), Label("[i:40] Arrows and Bullets")]
		public int ammos;
		[Range(-50, 999), DefaultValue(-1), Label("[i:75] Other Ammunitions")]
		public int specialAmmos;
		[DefaultValue(-2), Label("[i:188] Recovery potions")]
		public int recovery;
		[Range(-50, 999), DefaultValue(-1), Label("[i:2347] Buff potions")]
		public int buffPotions;
		[Range(-50, 999), DefaultValue(5), Label("[i:29] Permanent Boosters")]
		public int boosters;
		[Range(-50, 999), DefaultValue(1), Label("[i:43] Boss and Event summoners")]
		public int summoning;
		[Range(-50, 999), DefaultValue(10), Label("[i:2019] Criters and Baits")]
		public int critters;
		[Range(-50, 999), DefaultValue(3), Label("[i:3093] Crates and Bags")]
		public int bags;
		[Range(-50, 999), DefaultValue(-1), Label("[i:282] Miscellaneous")]
		public int tools;


		[Header("Tiles")]
		[Range(-50, 999), DefaultValue(-1), Label("[i:3] Blocks")]
		public int blocks;
		[Range(-50, 999), DefaultValue(100), Label("[i:1104] Ores")]
		public int ores;
		[Range(-50, 999), DefaultValue(5), Label("[i:343] Containers")]
		public int containers;
		[Range(-50, 999), DefaultValue(5), Label("[i:105] Light sources")]
		public int lightSources;
		[Range(-50, 999), DefaultValue(5), Label("[i:333] Furnitures")]
		public int furnitures;
		[Range(-50, 999), DefaultValue(10), Label("[i:3611] Wiring")]
		public int wiring;
		[Range(-50, 999), DefaultValue(-1), Label("[i:132] Walls")]
		public int walls;
		[Range(-50, 999), DefaultValue(5), Label("[i:27] Seeds")]
		public int seeds;
		[Range(-50, 999), DefaultValue(10), Label("[i:206] Liquids")]
		public int liquids;

		[Header("Crafting")]
		[Range(0, 50), DefaultValue(1), Label("Not Stackable")]
		public int single;
		[Range(-50, 999), DefaultValue(-1), Label("Stackable")]
		public int stack;

		[Header("Custom Categories and values")]
		[Label("[i:1913] Custom categories")]
		public List<CustomCategory> customCategories = new();
		[Label("[i:509] Custom Requirement"), Tooltip("Category 'Custom'")]
		public List<CustomInfinity> customInfinities = new();

		public bool HasCustomCategory(Item item, out Category custom) {
			CustomCategory customCategory = customCategories.Find(c => c.Item.Type == item.type);
			if(customCategory != null) {
				custom = customCategory.Category;
				return true;
			}
			custom = Category.Blacklist;
			return false;
		}
		public bool HasCustomInfinity(int type, out int requirement) {
			CustomInfinity customInfinity = customInfinities.Find(c => c.Item.Type == type);
			if (customInfinity != null) {
				requirement = customInfinity.Requirement;
				return true;
			}
			requirement = 0;
			return false;
		}

		public int InfinityRequirement(int requirement, int type, int LargestStack = 999, float multplier = 1f) {
			if(HasCustomInfinity(type, out int custom)){
				requirement = custom;
			}
			int maxStack = SpicItem.MaxStack(type);
			int req = requirement switch {
				< 0 => -requirement * (maxStack < LargestStack ? maxStack : LargestStack),
				_ => requirement > maxStack? maxStack : requirement
			};
			return req == 0 ? int.MaxValue : custom == 0 ? (int)(req * multplier) : req;
		}
		public static Category CategoryFromName(string name) {
			string lowName = name.ToLower();
			for (int i = 0; i < CategoryCount; i++) {
				if (lowName == $"{(Category)i}".ToLower()) return (Category)i;
			}
			throw new UsageException("Wrong category: " + name);
		}
		public void InGameSetCustomCategory(int type, Category category) {

			ClearCustom(type);
			customCategories.Add(new CustomCategory(new(type), category));
			modifiedInGame = true;
		}
		public void InGameSetCustomCategory(int type, int requirement) {
			ClearCustom(type);
			customInfinities.Add(new CustomInfinity(new(type), requirement));
			modifiedInGame = true;
		}
		private void ClearCustom(int type) {
			for (int i = customInfinities.Count - 1; i >= 0; i--) {
				if (customInfinities[i].Item.Type == type) customInfinities.RemoveAt(i);
			}
			for (int i = customCategories.Count - 1; i >= 0; i--) {
				if (customCategories[i].Item.Type == type) customCategories.RemoveAt(i);
			}
		}
	}
}
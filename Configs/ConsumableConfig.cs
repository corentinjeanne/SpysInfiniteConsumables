using System.Collections.Generic;
using System.ComponentModel;

using System.IO;
using Newtonsoft.Json;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs {
	public class Consumable {
		[Range(-50, 999), Label("[i:279] Thrown weapons")]
		public int Weapons = -1;
		[DefaultValue(-2), Label("[i:188] Recovery potions")]
		public int Recoveries = -2;
		[Range(-50, 999), Label("[i:2348] Buff potions")]
		public int Buffs = -1;
		[Range(-50, 999), Label("[i:29] Permanent Boosters")]
		public int Boosters = 5;
		[Range(-50, 999), Label("[i:43] Boss and Event summoners")]
		public int Summoners = 3;
		[Range(-50, 999), Label("[i:2019] Criters and Baits")]
		public int Critters = 10;
		[Range(-50, 999), Label("[i:282] Explosives"),Tooltip("Does not work for now")]
		public int Explosives = -1;
		[Range(-50, 999), Label("[i:282] Miscellaneous")]
		public int Tools = -1;
	}
	public class Ammo {
		[Range(-50, 999), Label("[i:40] Standard")]
		public int Basic = -4;
		[Range(-50, 999), Label("[i:75] Other")]
		public int Special = -1;
	}
	public class GrabBag {
		[Range(-50, 999), Label("[i:2334] Crates and Bags")]
		public int Crates = 5;
		[Range(-50, 999), Label("[i:3331] Treasure Bags")]
		public int TreasureBags = 3;
	}

	public class CommonTiles {
		[Range(-50, 999), Label("[i:3] Blocks")]
		public int Blocks = -1;
		[Range(-50, 999), Label("[i:8] Platforms and Torches")]
		public int PlatformAndTorches = 100;
		[Range(-50, 999), Label("[i:702] Ores")]
		public int Ores = 100;
		[Range(-50, 999), Label("[i:93] Walls")]
		public int Walls = -1;
	}
	public class Furnitures {
		[Range(-50, 999), Label("[i:105] Light sources")]
		public int LightSources = 3;
		[Range(-50, 999), Label("[i:343] Containers")]
		public int Containers = 3;
		[Range(-50, 999), Label("[i:398] Functional")]
		public int Functional = 3;
		[Range(-50, 999), Label("[i:333] Decoration")]
		public int Decoration = 3;
	}
	public class OthersTiles {
		[Range(-50, 999), Label("[i:3603] Mechanical")]
		public int Mechanical = 3;
		[Range(-50, 999), Label("[i:206] Buckets")]
		public int Buckets = 5;
		[Range(-50, 999), Label("[i:27] Seeds")]
		public int Seeds = 5;
	}
	public class Materials {
		[Range(-50, 999), Label("[i:9] Basic materials")]
		public int Basics = -1;
		[Range(-50, 999), Label("[i:177] Valuable Tiles"), Tooltip("Drops from tiles detected by a metal detector")]
		public int Ores = 100;
		[Range(-50, 999), Label("[i:32] Furnitures")]
		public int Furnitures = 20;
		[Range(-50, 999), Label("[i:575] Miscellaneous")]
		public int Miscellaneous = 50;
		[Range(0, 50), Label("[i:53] Non stackable")]
		public int NonStackable = 2;
	}

	[NullAllowed]
	public class CustomInfinity<T> where T : System.Enum {
		public T Category;
		[Range(0, 50), Tooltip("Only in use when category is None")]
		public int Infinity;
	}

	public class Custom {
		public ItemDefinition Item;
		public CustomInfinity<Categories.Consumable> Consumable;
		public CustomInfinity<Categories.Ammo> Ammo;
		public CustomInfinity<Categories.GrabBag> GrabBag;
		public CustomInfinity<Categories.WandAmmo> WandAmmo;

		public static Custom CreateWith<T>(int type, CustomInfinity<T> customInfinity) where T: System.Enum {
			Custom c = new();
			c.Item = new(type);
			return c.Set(customInfinity);
		}
		public Custom Set<T>(CustomInfinity<T> customInfinity) where T : System.Enum {
			if (typeof(T) == typeof(Categories.Consumable)) Consumable = customInfinity as CustomInfinity<Categories.Consumable>;
			else if (typeof(T) == typeof(Categories.Ammo)) Ammo = customInfinity as CustomInfinity<Categories.Ammo>;
			else if (typeof(T) == typeof(Categories.GrabBag)) GrabBag = customInfinity as CustomInfinity<Categories.GrabBag>;
			else if (typeof(T) == typeof(Categories.WandAmmo)) WandAmmo = customInfinity as CustomInfinity<Categories.WandAmmo>;
			else throw new UsageException();
			return this;
		}
	}

	public class ConsumableConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static ConsumableConfig Instance => _instance ??= ModContent.GetInstance<ConsumableConfig>();
		private static ConsumableConfig _instance;
		public static string ConfigPath { get; private set; }
		private bool m_ModifiedInGame = false;


		[Header("General")]
		[DefaultValue(true), Label("[i:3104] Infinite Consumables")]
		public bool InfiniteConsumables;
		[Label("[i:3061] Infinite Tiles")]
		public bool InfiniteTiles;
		[Label("[i:398] Infinite Crafting")]
		public bool InfiniteCrafting;

		[DefaultValue(false), Label("[i:2890] Journey requirement"), Tooltip("Uses the journey research cost of items as their infinite requirent")]
		public bool JourneyRequirement;
		[DefaultValue(true), Label("[i:1293] Prevent item duplication"), Tooltip(
@"/!\ WIP for multiplayer /!\
Tiles and walls won't drop their item when broken
Critters will turn into smoke
Buckets won't create empty or full buckets when used")]
		public bool PreventItemDupication;
		[DefaultValue(true), ReloadRequired, Label("[i:3617] Commands"), Tooltip("Adds the '/spic' command to edit the category of items in-game")]
		public bool Commands;


		[Header("Consumables")]
		[Label("Consumables")]
		public Consumable Consumables = new();
		[Label("Ammunitions")]
		public Ammo Ammos = new();
		[Label("Grab Bags")]
		public GrabBag Bags = new();


		[Header("Tiles")]
		[Label("Common tiles")]
		public CommonTiles CommonTiles = new();
		[Label("Furitures")]
		public Furnitures Furnitures = new();
		[Label("Other tiles")]
		public OthersTiles OtherTiles = new();


		[Header("Crafting")]
		[Label("Materials")]
		public Materials Materials = new();


		[Header("Custom Categories and values")]
		[Label("[i:1913] Customs")]
		public List<Custom> Customs = new();

		public override void OnLoaded() => ConfigPath = ConfigManager.ModConfigPath + $"\\SPIC_{nameof(ConsumableConfig)}.json";

		public void ManualSave() {
			if (!m_ModifiedInGame) return;
			using StreamWriter sw = new(ConfigPath);
			string serialisedConfig = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
			sw.Write(serialisedConfig);
			m_ModifiedInGame = false;
		}

		public bool HasCustom(int type, out Custom custom) => (custom = GetCustom(type)) != null;
		public Custom GetCustom(int type) => Customs.Find(c => c.Item.Type == type);
		
		public void InGameSetCustom<T>(int type, CustomInfinity<T> customInfinity) where T : System.Enum {
			if(!HasCustom(type, out Custom c)) Customs.Add(Custom.CreateWith(type, customInfinity));
			else c.Set(customInfinity);
		}
	}
}
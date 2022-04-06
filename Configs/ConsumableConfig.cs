using System.Collections.Generic;
using System.ComponentModel;

using System.IO;
using Newtonsoft.Json;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs {
	public class Consumable {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.WeaponsLabel")]
		public int Weapons = -1;
		[DefaultValue(-2), Label("$Mods.SPIC.Configs.Consumables.RecoveryLabel")]
		public int Recovery = -2;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.BuffsLabel")]
		public int Buffs = -1;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.BoostersLabel")]
		public int Boosters = 5;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.SummonersLabel")]
		public int Summoners = 3;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.CrittersLabel")]
		public int Critters = 10;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.ExplosivesLabel"),Tooltip("$Mods.SPIC.Configs.Consumables.ExplosivesTooltip")]
		public int Explosives = -1;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.ToolsLabel")]
		public int Tools = -1;
	}
	public class Ammo {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.StandardLabel")]
		public int Standard = -4;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.SpecialLabel")]
		public int Special = -1;
	}
	public class GrabBag {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.CratesLabel")]
		public int Crates = 5;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.BossLabel")]
		public int TreasureBags = 3;
	}

	public class CommonTiles {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.BlocksLabel")]
		public int Blocks = -1;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.PlatformsLabel")]
		public int PlatformsAndTorches = 100;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.OresLabel")]
		public int Ores = 100;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.WallsLabel")]
		public int Walls = -1;
	}
	public class Furnitures {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.LightsLabel")]
		public int LightSources = 3;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.ContainersLabel")]
		public int Containers = 3;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.FunctionalLabel")]
		public int Functional = 3;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.DecorationsLabel")]
		public int Decorations = 3;
	}
	public class OthersTiles {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.MechanicalLabel")]
		public int Mechanical = 3;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.BucketsLabel")]
		public int Buckets = 5;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.SeedsLabel")]
		public int Seeds = 5;
	}
	public class Materials {
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Materials.BasicsLabel")]
		public int Basics = -1;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Materials.OresLabel"), Tooltip("$Mods.SPIC.Configs.Materials.OresTooltip")]
		public int Ores = 100;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Materials.FurnituresLabel")]
		public int Furnitures = 20;
		[Range(-50, 999), Label("$Mods.SPIC.Configs.Materials.MiscellaneousLabel")]
		public int Miscellaneous = 50;
		[Range(0, 50),    Label("$Mods.SPIC.Configs.Materials.NonStackableLabel")]
		public int NonStackable = 2;
	}

	[NullAllowed]
	public class CustomInfinity<T> where T : System.Enum {
		[Label("$Mods.SPIC.Configs.Customs.Category")]
		public T Category;
		[Range(0, 50), Label("$Mods.SPIC.Configs.Customs.Infinity"), Tooltip("$Mods.SPIC.Configs.Customs.InfinityTooltip")]
		public int Infinity;
	}

	public class Custom {
		[Label("$Mods.SPIC.Configs.Customs.Item")]
		public ItemDefinition Item;

		[Label("$Mods.SPIC.Configs.Infinities.Consumables")]
		public CustomInfinity<Categories.Consumable> Consumable;
		[Label("$Mods.SPIC.Configs.Infinities.Ammos")]
		public CustomInfinity<Categories.Ammo> Ammo;
		[Label("$Mods.SPIC.Configs.Infinities.Bags")]
		public CustomInfinity<Categories.GrabBag> GrabBag;
		[Label("$Mods.SPIC.Configs.Customs.WandAmmo")] 
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

		internal static string Loc(string a) => a;
		[Header("$Mods.SPIC.Configs.General.Header")]
		[DefaultValue(true), Label("$Mods.SPIC.Configs.General.ConsumablesLabel")]
		public bool InfiniteConsumables;
		[Label("$Mods.SPIC.Configs.General.TilesLabel")]
		public bool InfiniteTiles;
		[Label("$Mods.SPIC.Configs.General.CraftingLabel")]
		public bool InfiniteCrafting;

		[DefaultValue(false), Label("$Mods.SPIC.Configs.General.JourneyLabel"), Tooltip("$Mods.SPIC.Configs.General.JourneyTooltip")]
		public bool JourneyRequirement;
		[DefaultValue(true), Label("$Mods.SPIC.Configs.General.DuplicationLabel"), Tooltip("$Mods.SPIC.Configs.General.DuplicationTooltip")]
		public bool PreventItemDupication;
		[DefaultValue(true), ReloadRequired, Label("$Mods.SPIC.Configs.General.CommandsLabel"), Tooltip("$Mods.SPIC.Configs.General.CommandsTooltip")]
		public bool Commands;


		[Header("$Mods.SPIC.Configs.Infinities.ConsumablesHeader")]
		[Label("$Mods.SPIC.Configs.Infinities.Consumables")]
		public Consumable Consumables = new();
		[Label("$Mods.SPIC.Configs.Infinities.Ammos")]
		public Ammo Ammos = new();
		[Label("$Mods.SPIC.Configs.Infinities.Bags")]
		public GrabBag Bags = new();


		[Header("$Mods.SPIC.Configs.Infinities.TilesHeader")]
		[Label("$Mods.SPIC.Configs.Infinities.CommonTiles")]
		public CommonTiles CommonTiles = new();
		[Label("$Mods.SPIC.Configs.Infinities.Furnitures")]
		public Furnitures Furnitures = new();
		[Label("$Mods.SPIC.Configs.Infinities.OtherTiles")]
		public OthersTiles OtherTiles = new();


		[Header("$Mods.SPIC.Configs.Infinities.CraftingHeader")]
		[Label("$Mods.SPIC.Configs.Infinities.Materials")]
		public Materials Materials = new();


		[Header("$Mods.SPIC.Configs.Infinities.CustomHeader")]
		[Label("$Mods.SPIC.Configs.Infinities.CustomsLabel")]
		public List<Custom> Customs = new();

		public override void OnLoaded() => ConfigPath = ConfigManager.ModConfigPath + $"\\{nameof(SPIC)}_{nameof(ConsumableConfig)}.json";

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
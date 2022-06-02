using System.Collections.Generic;
using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;


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
    [Range(50, 999), Label("$Mods.SPIC.Configs.Consumables.StandardLabel")]
    public int Standard = -4;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.SpecialLabel")]
    public int Special = -1;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Consumables.ExplosivesLabel"), Tooltip("$Mods.SPIC.Configs.Consumables.ExplosivesTooltip")]
    public int Explosives = -1;
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
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.WiringLabel")]
    public int Wiring = -1;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Tiles.PlatformsLabel")]
    public int PlatformsAndTorches = 99;
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
    [Range(-50, 0), Label("$Mods.SPIC.Configs.Materials.NonStackableLabel")]
    public int NonStackable = -2;
}


[NullAllowed]
public class CustomInfinity<T> where T : System.Enum {
    [Label("$Mods.SPIC.Configs.Customs.Category")]
    public T Category;
    [Range(0, 50), Label("$Mods.SPIC.Configs.Customs.Infinity"), Tooltip("$Mods.SPIC.Configs.Customs.InfinityTooltip")]
    public int Infinity;
}


public class Custom {
    [Label("$Mods.SPIC.Configs.Infinities.Consumables")]
    public CustomInfinity<Categories.Consumable> Consumable;
    [Label("$Mods.SPIC.Configs.Infinities.Ammos")]
    public CustomInfinity<Categories.Ammo> Ammo;
    [Label("$Mods.SPIC.Configs.Infinities.Bags")]
    public CustomInfinity<Categories.GrabBag> GrabBag;
    [Label("$Mods.SPIC.Configs.Customs.WandAmmo")] 
    public CustomInfinity<Categories.WandAmmo> WandAmmo;

    public static Custom CreateWith<T>(CustomInfinity<T> customInfinity) where T: System.Enum => new Custom().Set(customInfinity);
    
    public Custom Set<T>(CustomInfinity<T> customInfinity) where T : System.Enum {
        System.Type type = typeof(T);
        if (type == typeof(Categories.Consumable)) Consumable = customInfinity as CustomInfinity<Categories.Consumable>;
        else if (type == typeof(Categories.Ammo)) Ammo = customInfinity as CustomInfinity<Categories.Ammo>;
        else if (type == typeof(Categories.GrabBag)) GrabBag = customInfinity as CustomInfinity<Categories.GrabBag>;
        else if (type == typeof(Categories.WandAmmo)) WandAmmo = customInfinity as CustomInfinity<Categories.WandAmmo>;
        else throw new UsageException();
        return this;
    }

    public CustomCategories Categories() => new(
        Ammo?.Category == SPIC.Categories.Ammo.None ? null : Ammo?.Category,
        Consumable?.Category == SPIC.Categories.Consumable.None ? null : Consumable?.Category,
        GrabBag?.Category == SPIC.Categories.GrabBag.None ? null : GrabBag?.Category,
        WandAmmo?.Category == SPIC.Categories.WandAmmo.None ? null : WandAmmo?.Category
    );
    public CustomInfinities Infinities() => new(
        Ammo?.Category == SPIC.Categories.Ammo.None ? this.Ammo.Infinity : null,
        Consumable?.Category == SPIC.Categories.Consumable.None ? this.Consumable.Infinity : null,
        GrabBag?.Category == SPIC.Categories.GrabBag.None ? this.GrabBag.Infinity : null,
        WandAmmo?.Category == SPIC.Categories.WandAmmo.None ? this.WandAmmo.Infinity : null
    );

}

public struct CustomCategories {
    public readonly Categories.Ammo? Ammo;
    public readonly Categories.Consumable? Consumable;
    public readonly Categories.GrabBag? GrabBag;
    public readonly Categories.WandAmmo? WandAmmo;

    public CustomCategories(Categories.Ammo? ammo, Categories.Consumable? consumable, Categories.GrabBag? grabBag, Categories.WandAmmo? wandAmmo) {
        Ammo = ammo; Consumable = consumable; GrabBag = grabBag; WandAmmo = wandAmmo;
    }
}
public struct CustomInfinities {
    public readonly int? Ammo;
    public readonly int? Consumable;
    public readonly int? GrabBag;
    public readonly int? WandAmmo;

    public CustomInfinities(int? ammo, int? consumable, int? grabBag, int? wandAmmo) {
        Ammo = ammo; Consumable = consumable; GrabBag = grabBag; WandAmmo = wandAmmo;
    }
}

public class Infinities : ModConfig {
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public static Infinities Instance => _instance ??= ModContent.GetInstance<Infinities>();
    private static Infinities _instance;


    [Header("$Mods.SPIC.Configs.General.Header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.ConsumablesLabel")]
    public bool InfiniteConsumables;
    [Label("$Mods.SPIC.Configs.General.BagsLabel")]
    public bool InfiniteGrabBags;
    [Label("$Mods.SPIC.Configs.General.TilesLabel")]
    public bool InfiniteTiles;
    [Label("$Mods.SPIC.Configs.General.CurrencyLabel")]
    public bool InfiniteCurrency;
    [Label("$Mods.SPIC.Configs.General.CraftingLabel")]
    public bool InfiniteCrafting;

    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.DuplicationLabel"), Tooltip("$Mods.SPIC.Configs.General.DuplicationTooltip")]
    public bool PreventItemDupication;
    // [DefaultValue(true), ReloadRequired, Label("$Mods.SPIC.Configs.General.CommandsLabel"), Tooltip("$Mods.SPIC.Configs.General.CommandsTooltip")]
    // public bool Commands;
    [DefaultValue(false), Label("$Mods.SPIC.Configs.General.JourneyLabel"), Tooltip("$Mods.SPIC.Configs.General.JourneyTooltip")]
    public bool JourneyRequirement;


    [Header("$Mods.SPIC.Configs.Infinities.ConsumablesHeader")]
    [Label("$Mods.SPIC.Configs.Infinities.ConsumablesLabel")]
    public Consumable Consumables = new();
    [Label("$Mods.SPIC.Configs.Infinities.AmmosLabel")]
    public Ammo Ammos = new();

    [Header("$Mods.SPIC.Configs.Infinities.BagsHeader")]
    [Label("$Mods.SPIC.Configs.Infinities.BagsLabel")]
    public GrabBag Bags = new();


    [Header("$Mods.SPIC.Configs.Infinities.TilesHeader")]
    [Label("$Mods.SPIC.Configs.Infinities.CommonTilesLabel")]
    public CommonTiles CommonTiles = new();
    [Label("$Mods.SPIC.Configs.Infinities.FurnituresLabel")]
    public Furnitures Furnitures = new();
    [Label("$Mods.SPIC.Configs.Infinities.OtherTilesLabel")]
    public OthersTiles OtherTiles = new();


    [Header("$Mods.SPIC.Configs.Infinities.CraftingHeader")]
    [Label("$Mods.SPIC.Configs.Infinities.MaterialsLabel")]
    public Materials Materials = new();


    [Header("$Mods.SPIC.Configs.Infinities.CustomHeader")]
    [Label("$Mods.SPIC.Configs.Infinities.CustomsLabel")]
    public Dictionary<ItemDefinition,Custom> Customs = new();

    public CustomCategories GetCustomCategories(int type)
        => Customs.TryGetValue(new(type), out var custom) ? custom.Categories() : new();

    public CustomInfinities GetCustomInfinities(int type)
        => Customs.TryGetValue(new(type), out var custom) ? custom.Infinities() : new();
    

    // public void InGameSetCustom<T>(int type, CustomInfinity<T> customInfinity) where T : System.Enum{
    //     ItemDefinition key = new(type);
    //     if(Customs.TryGetValue(new(type), out Custom custom)) custom.Set(customInfinity);
    //     else Customs.Add(key,Custom.CreateWith(customInfinity));
    //     _modifiedInGame = true;
    // }
}

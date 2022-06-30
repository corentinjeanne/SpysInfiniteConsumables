using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;


[NullAllowed]
public class CustomInfinity<T> where T : System.Enum {
    [Label("$Mods.SPIC.Configs.Infinities.Custom.Category")]
    public T Category;
    [Range(0, 50), Label("$Mods.SPIC.Configs.Infinities.Custom.Infinity"), Tooltip("$Mods.SPIC.Configs.Infinities.Custom.InfinityTooltip")]
    public int Infinity;
}


public class Custom {
    [Label("$Mods.SPIC.Configs.Infinities.Custom.Consumable")]
    public CustomInfinity<Categories.Consumable> Consumable;
    [Label("$Mods.SPIC.Configs.Infinities.Custom.Placeable")] 
    public CustomInfinity<Categories.Placeable> Placeable;
    [Label("$Mods.SPIC.Configs.Infinities.Custom.Ammo")]
    public CustomInfinity<Categories.Ammo> Ammo;
    [Label("$Mods.SPIC.Configs.Infinities.Custom.Bag")]
    public CustomInfinity<Categories.GrabBag> GrabBag;
    
    public Custom Set<T>(CustomInfinity<T> customInfinity) where T : System.Enum {
        System.Type type = typeof(T);
        if (type == typeof(Categories.Consumable)) Consumable = customInfinity as CustomInfinity<Categories.Consumable>;
        else if (type == typeof(Categories.Ammo)) Ammo = customInfinity as CustomInfinity<Categories.Ammo>;
        else if (type == typeof(Categories.GrabBag)) GrabBag = customInfinity as CustomInfinity<Categories.GrabBag>;
        else if (type == typeof(Categories.Placeable)) Placeable = customInfinity as CustomInfinity<Categories.Placeable>;
        else throw new UsageException();
        return this;
    }

    public CustomCategories Categories() => new(
        Ammo?.Category == SPIC.Categories.Ammo.None ? null : Ammo?.Category,
        Consumable?.Category == SPIC.Categories.Consumable.None ? null : Consumable?.Category,
        GrabBag?.Category == SPIC.Categories.GrabBag.None ? null : GrabBag?.Category,
        Placeable?.Category == SPIC.Categories.Placeable.None ? null : Placeable?.Category
    );
    public CustomInfinities Infinities() => new(
        Ammo?.Category == SPIC.Categories.Ammo.None ? Ammo.Infinity : null,
        Consumable?.Category == SPIC.Categories.Consumable.None ? Consumable.Infinity : null,
        GrabBag?.Category == SPIC.Categories.GrabBag.None ? GrabBag.Infinity : null,
        Placeable?.Category == SPIC.Categories.Placeable.None ? Placeable.Infinity : null
    );

}

public struct CustomCategories {
    public readonly Categories.Ammo? Ammo;
    public readonly Categories.Consumable? Consumable;
    public readonly Categories.Placeable? Placeable;
    public readonly Categories.GrabBag? GrabBag;

    public CustomCategories(Categories.Ammo? ammo, Categories.Consumable? consumable, Categories.GrabBag? grabBag, Categories.Placeable? placeable) {
        Ammo = ammo; Consumable = consumable; GrabBag = grabBag; Placeable = placeable;
    }
}

public struct CustomInfinities {
    public readonly int? Ammo, Consumable, GrabBag, Placeable;

    public CustomInfinities(int? ammo, int? consumable, int? grabBag, int? placeable) {
        Ammo = ammo; Consumable = consumable; GrabBag = grabBag; Placeable = placeable;
    }
}

public class Infinities : ModConfig {
    
    [Header("$Mods.SPIC.Configs.General.Header")]

    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.ConsumablesLabel")]
    public bool InfiniteConsumables;
    [Label("$Mods.SPIC.Configs.General.BagsLabel")]
    public bool InfiniteGrabBags;
    [Label("$Mods.SPIC.Configs.General.PlaceablesLabel")]
    public bool InfinitePlaceables;
    [Label("$Mods.SPIC.Configs.General.CurrenciesLabel")]
    public bool InfiniteCurrencies;
    [Label("$Mods.SPIC.Configs.General.MaterialsLabel")]
    public bool InfiniteMaterials;

    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.DuplicationLabel"), Tooltip("$Mods.SPIC.Configs.General.DuplicationTooltip")]
    public bool PreventItemDupication;
    [DefaultValue(false), Label("$Mods.SPIC.Configs.General.JourneyLabel"), Tooltip("$Mods.SPIC.Configs.General.JourneyTooltip")]
    public bool JourneyRequirement;

    
    [Header("$Mods.SPIC.Configs.Infinities.ConsumablesHeader")]

    [Range(-50, 999), DefaultValue(-2), Label("$Mods.SPIC.Configs.Infinities.WeaponsLabel")]
    public int consumables_Weapons;
    [Range(-50, 999), DefaultValue(-4), Label("$Mods.SPIC.Configs.Infinities.StandardAmmoLabel")]
    public int ammo_Standard;
    [Range(-50, 999), DefaultValue(-2), Label("$Mods.SPIC.Configs.Infinities.SpecialAmmoLabel")]
    public int ammo_Special;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Infinities.PotionsLabel")]
    public int consumables_Potions;
    [Range(-50, 999), DefaultValue(5), Label("$Mods.SPIC.Configs.Infinities.BoostersLabel")]
    public int consumables_Boosters;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Infinities.SummonersLabel")]
    public int consumables_Summoners;
    [Range(-50, 999), DefaultValue(10), Label("$Mods.SPIC.Configs.Infinities.CrittersLabel")]
    public int consumables_Critters;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Infinities.ToolsLabel")]
    public int consumables_Tools;
    

    [Header("$Mods.SPIC.Configs.Infinities.BagsHeader")]

    [Range(-50, 999), DefaultValue(5), Label("$Mods.SPIC.Configs.Infinities.CratesLabel")]
    public int bags_Crates;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Infinities.BossLabel")]
    public int bags_TreasureBags;


    [Header("$Mods.SPIC.Configs.Infinities.PlaceablesHeader")]

    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Infinities.TilesLabel")]
    public int placeables_Tiles;
    [Range(-50, 999), DefaultValue(499), Label("$Mods.SPIC.Configs.Infinities.OresLabel")]
    public int placeables_Ores;
    [Range(-50, 999), DefaultValue(99), Label("$Mods.SPIC.Configs.Infinities.TorchesLabel")]
    public int placeables_Torches;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Infinities.FurnituresLabel")]
    public int placeables_Furnitures;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Infinities.MechanicalLabel")]
    public int placeables_Mechanical;
    [Range(-50, 999), DefaultValue(5), Label("$Mods.SPIC.Configs.Infinities.LiquidsLabel")]
    public int placeables_Liquids;
    [Range(-50, 999), DefaultValue(5), Label("$Mods.SPIC.Configs.Infinities.SeedsLabel")]
    public int placeables_Seeds;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Infinities.PaintsLabel")] // TODO Localization
    public int placeables_Paints;


    [Header("$Mods.SPIC.Configs.Infinities.CurrenciesHeader")]
    [Range(-50, 999), DefaultValue(-10), Label("$Mods.SPIC.Configs.Infinities.CoinsLabel")]
    public int currency_Coins;
    [Range(-50, 999), DefaultValue(100), Label("$Mods.SPIC.Configs.Infinities.CustomCoinsLabel")]
    public int currency_Single;


    [Header("$Mods.SPIC.Configs.Infinities.MaterialsHeader")]

    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Infinities.BasicsLabel")]
    public int materials_Basics;
    [Range(-50, 999), DefaultValue(499d), Label("$Mods.SPIC.Configs.Infinities.OresLabel")]
    public int materials_Ores;
    [Range(-50, 999), DefaultValue(20), Label("$Mods.SPIC.Configs.Infinities.FurnituresLabel")]
    public int materials_Furnitures;
    [Range(-50, 999), DefaultValue(50), Label("$Mods.SPIC.Configs.Infinities.MiscellaneousLabel")]
    public int materials_Miscellaneous;
    [Range(-50, 0), DefaultValue(-2), Label("$Mods.SPIC.Configs.Infinities.NonStackableLabel")]
    public int materials_NonStackable;


    [Header("$Mods.SPIC.Configs.Infinities.CustomHeader")]

    [Label("$Mods.SPIC.Configs.Infinities.CustomsLabel")]
    public Dictionary<ItemDefinition,Custom> customs = new();

    public CustomCategories GetCustomCategories(int type) => customs.TryGetValue(new(type), out var custom) ? custom.Categories() : new();
    public CustomInfinities GetCustomInfinities(int type) => customs.TryGetValue(new(type), out var custom) ? custom.Infinities() : new();


    // public void InGameSetCustom<T>(int type, CustomInfinity<T> customInfinity) where T : System.Enum{
    //     ItemDefinition key = new(type);
    //     if(Customs.TryGetValue(new(type), out Custom custom)) custom.Set(customInfinity);
    //     else Customs.Add(key,Custom.CreateWith(customInfinity));
    //     _modifiedInGame = true;
    // }

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static Infinities Instance => _instance ??= ModContent.GetInstance<Infinities>();
    private static Infinities _instance;
}

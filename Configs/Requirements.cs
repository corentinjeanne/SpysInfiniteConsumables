using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;


[NullAllowed]
public class CustomRequirement<T> where T : System.Enum {
    [Label("$Mods.SPIC.Categories.name")]
    public T Category;
    [Range(0, 50), Label("$Mods.SPIC.Configs.Requirements.Customs.requirement"), Tooltip("$Mods.SPIC.Configs.Requirements.Customs.t_requirement")]
    public int Requirement;
}

// TODO rework into dict
public class Custom {
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Ammo")]
    public CustomRequirement<Infinities.AmmoCategory> Ammo;
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Usable")]
    public CustomRequirement<Infinities.UsableCategory> Usables;
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Placeable")] 
    public CustomRequirement<Infinities.PlaceableCategory> Placeable;
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Bag")]
    public CustomRequirement<Infinities.GrabBagCategory> GrabBag;
    
    public Custom Set<T>(CustomRequirement<T> customRequirement) where T : System.Enum {
        System.Type type = typeof(T);
        if (type == typeof(Infinities.UsableCategory)) Usables = customRequirement as CustomRequirement<Infinities.UsableCategory>;
        else if (type == typeof(Infinities.AmmoCategory)) Ammo = customRequirement as CustomRequirement<Infinities.AmmoCategory>;
        else if (type == typeof(Infinities.GrabBagCategory)) GrabBag = customRequirement as CustomRequirement<Infinities.GrabBagCategory>;
        else if (type == typeof(Infinities.PlaceableCategory)) Placeable = customRequirement as CustomRequirement<Infinities.PlaceableCategory>;
        else throw new UsageException();
        return this;
    }

    public CustomCategories Categories() => new(
        Ammo?.Category == Infinities.AmmoCategory.None ? null : Ammo?.Category,
        Usables?.Category == Infinities.UsableCategory.None ? null : Usables?.Category,
        GrabBag?.Category == Infinities.GrabBagCategory.None ? null : GrabBag?.Category,
        Placeable?.Category == Infinities.PlaceableCategory.None ? null : Placeable?.Category
    );
    public CustomRequirements Requirements() => new(
        Ammo?.Category == Infinities.AmmoCategory.None ? Ammo.Requirement : null,
        Usables?.Category == Infinities.UsableCategory.None ? Usables.Requirement : null,
        GrabBag?.Category == Infinities.GrabBagCategory.None ? GrabBag.Requirement : null,
        Placeable?.Category == Infinities.PlaceableCategory.None ? Placeable.Requirement : null
    );

}

public readonly record struct CustomCategories(Infinities.AmmoCategory? Ammo, Infinities.UsableCategory? Usable, Infinities.GrabBagCategory? GrabBag, Infinities.PlaceableCategory? Placeable);

public readonly record struct CustomRequirements(int? Ammo, int? Usable, int? GrabBag, int? Placeable);

[Label("$Mods.SPIC.Configs.Requirements.name")]
public class Requirements : ModConfig {
    
    [Header("$Mods.SPIC.Configs.Requirements.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Requirements.General.Consumables")]
    public bool InfiniteConsumables;
    [Label("$Mods.SPIC.Configs.Requirements.General.Placeables")]
    public bool InfinitePlaceables;
    [Label("$Mods.SPIC.Configs.Requirements.General.Bags")]
    public bool InfiniteGrabBags;
    [Label("$Mods.SPIC.Configs.Requirements.General.Materials")]
    public bool InfiniteMaterials;
    [Label("$Mods.SPIC.Configs.Requirements.General.Currencies")]
    public bool InfiniteCurrencies;

    // TODO >>> rework infinty order and count
    [DefaultValue(false), Label("$Mods.SPIC.Configs.Requirements.General.Journey"), Tooltip("$Mods.SPIC.Configs.General.t_journey")]
    public bool JourneyRequirement;

    [DefaultValue(true), Label("$Mods.SPIC.Configs.Requirements.General.Duplication"), Tooltip("$Mods.SPIC.Configs.General.t_duplication")]
    public bool PreventItemDupication;

    // TODO >>> turn into dict
    [Header("$Mods.SPIC.Categories.Usable.names")]
    [Range(-50, 999), DefaultValue(-2), Label("$Mods.SPIC.Configs.Requirements.Requirements.Weapons")]
    public int usables_Weapons;
    [Range(-50, 999), DefaultValue(-4), Label("$Mods.SPIC.Configs.Requirements.Requirements.StandardAmmo")]
    public int ammo_Standard;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Requirements.Requirements.SpecialAmmo")]
    public int ammo_Special;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Requirements.Requirements.Potions")]
    public int usables_Potions;
    [Range(-50, 999), DefaultValue(5), Label("$Mods.SPIC.Configs.Requirements.Requirements.Boosters")]
    public int usables_Boosters;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Requirements.Requirements.Summoners")]
    public int usables_Summoners;
    [Range(-50, 999), DefaultValue(10), Label("$Mods.SPIC.Configs.Requirements.Requirements.Critters")]
    public int usables_Critters;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Requirements.Requirements.Tools")]
    public int usables_Tools;
    
    [Header("$Mods.SPIC.Categories.Placeable.names")]
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Requirements.Requirements.Tiles")]
    public int placeables_Tiles;
    [Range(-50, 999), DefaultValue(499), Label("$Mods.SPIC.Configs.Requirements.Requirements.Ores")]
    public int placeables_Ores;
    [Range(-50, 999), DefaultValue(99), Label("$Mods.SPIC.Configs.Requirements.Requirements.Torches")]
    public int placeables_Torches;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Requirements.Requirements.Furnitures")]
    public int placeables_Furnitures;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Requirements.Requirements.Mechanical")]
    public int placeables_Mechanical;
    [Range(-50, 999), DefaultValue(10), Label("$Mods.SPIC.Configs.Requirements.Requirements.Liquids")]
    public int placeables_Liquids;
    [Range(-50, 999), DefaultValue(20), Label("$Mods.SPIC.Configs.Requirements.Requirements.Seeds")]
    public int placeables_Seeds;
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Requirements.Requirements.Paints")]
    public int placeables_Paints;

    [Header("$Mods.SPIC.Categories.GrabBag.names")]
    [Range(-50, 999), DefaultValue(10), Label("$Mods.SPIC.Configs.Requirements.Requirements.Crates")]
    public int bags_Crates;
    [Range(-50, 999), DefaultValue(3), Label("$Mods.SPIC.Configs.Requirements.Requirements.Boss")]
    public int bags_TreasureBags;

    [Header("$Mods.SPIC.Categories.Material.names")]
    [Range(-50, 999), DefaultValue(-1), Label("$Mods.SPIC.Configs.Requirements.Requirements.Basics")]
    public int materials_Basics;
    [Range(-50, 999), DefaultValue(499d), Label("$Mods.SPIC.Configs.Requirements.Requirements.Ores")]
    public int materials_Ores;
    [Range(-50, 999), DefaultValue(20), Label("$Mods.SPIC.Configs.Requirements.Requirements.Furnitures")]
    public int materials_Furnitures;
    [Range(-50, 999), DefaultValue(50), Label("$Mods.SPIC.Configs.Requirements.Requirements.Miscellaneous")]
    public int materials_Miscellaneous;
    [Range(-50, 0), DefaultValue(-2), Label("$Mods.SPIC.Configs.Requirements.Requirements.NonStackable")]
    public int materials_NonStackable;

    [Header("$Mods.SPIC.Categories.Currency.names")]
    [Range(-50, 999), DefaultValue(-10), Label("$Mods.SPIC.Configs.Requirements.Requirements.Coins")]
    public int currency_Coins;
    [Range(-50, 999), DefaultValue(50), Label("$Mods.SPIC.Configs.Requirements.Requirements.CustomCoins")]
    public int currency_Single;

    [Header("$Mods.SPIC.Configs.Requirements.Customs.header")]
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Customs")]
    public Dictionary<ItemDefinition,Custom> customs = new();

    public CustomCategories GetCustomCategories(int type) => customs.TryGetValue(new(type), out var custom) ? custom.Categories() : new();
    public CustomRequirements GetCustomRequirements(int type) => customs.TryGetValue(new(type), out var custom) ? custom.Requirements() : new();

    // public void InGameSetCustom<T>(int type, CustomInfinity<T> customInfinity) where T : System.Enum{
    //     ItemDefinition key = new(type);
    //     if(Customs.TryGetValue(new(type), out Custom custom)) custom.Set(customInfinity);
    //     else Customs.Add(key,Custom.CreateWith(customInfinity));
    //     _modifiedInGame = true;
    // }

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static Requirements Instance => _instance ??= ModContent.GetInstance<Requirements>();
    private static Requirements _instance;
}

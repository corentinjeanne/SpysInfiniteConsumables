using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using Newtonsoft.Json;

namespace SPIC.Configs;

public struct DetectedCategories {
    public readonly Categories.Consumable? Consumable;
    public readonly Categories.Placeable? Placeable;

    public readonly bool GrabBag;
    public readonly bool Explosive;

    public DetectedCategories(Categories.Consumable? consumable, Categories.Placeable? placeable, bool grabBag, bool explosive) {
        Consumable = consumable; Placeable = placeable;
        GrabBag = grabBag; Explosive = explosive;
    }
}

[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static CategoryDetection Instance => _instance ??= ModContent.GetInstance<CategoryDetection>();
    private static CategoryDetection _instance;


    [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    [Label("$Mods.SPIC.Configs.Detection.Categories.Consumables")]
    public readonly Dictionary<ItemDefinition, Categories.Consumable> DetectedConsumables = new();
    [Label("$Mods.SPIC.Configs.Detection.Categories.Explosives")]
    public readonly HashSet<ItemDefinition> DetectedExplosives = new();
    [Label("$Mods.SPIC.Configs.Detection.Categories.Bags")]
    public readonly HashSet<ItemDefinition> DetectedGrabBags = new();
    [Label("$Mods.SPIC.Configs.Detection.Categories.WandAmmo")]
    public readonly Dictionary<ItemDefinition, Categories.Placeable> DetectedWandAmmo = new();

    public void DetectedConsumable(Terraria.Item item, Categories.Consumable consumable) {
        ItemDefinition key = new(item.type);
        if (IsExplosive(item.type) || !DetectedConsumables.TryAdd(key, consumable)) return;
        CategoryHelper.UpdateItem(item);
        _modifiedInGame = true;
    }

    public void DetectedPlaceable(Terraria.Item item, Categories.Placeable placeable) {
        ItemDefinition key = new(item.type);
        if (!DetectedWandAmmo.TryAdd(key, placeable)) return;
        CategoryHelper.UpdateItem(item);
        _modifiedInGame = true;
    }

    public bool DetectedExplosive(Terraria.Item item) {
        ItemDefinition key = new(item.type);
        if (!DetectedExplosives.Add(key)) return false;
        DetectedConsumables.Remove(key);
        CategoryHelper.UpdateItem(item);
        _modifiedInGame = true;
        return true;
    }
    public bool IsExplosive(int type) => DetectedExplosives.Contains(new(type));


    public void DetectedGrabBag(Terraria.Item item) {
        if (DetectedGrabBags.Add(new(item.type))) _modifiedInGame = true;
        CategoryHelper.UpdateItem(item);
    }

    public DetectedCategories GetDetectedCategories(int type){
        if(!DetectMissing) return new();

        ItemDefinition key = new(type);
        return new(
            DetectedConsumables.ContainsKey(key) ? DetectedConsumables[key] : null,
            DetectedWandAmmo.ContainsKey(key) ? DetectedWandAmmo[key] : null,
            DetectedGrabBags.Contains(key),
            DetectedExplosives.Contains(key)
        );
    }


    private static string _configPath;
    private bool _modifiedInGame = false;
    public override void OnLoaded() => _configPath = ConfigManager.ModConfigPath + $"\\{nameof(SPIC)}_{nameof(CategoryDetection)}.json";
    public void ManualSave() {
        if (!_modifiedInGame) return;
        using StreamWriter sw = new(_configPath);
        sw.Write(JsonConvert.SerializeObject(this, ConfigManager.serializerSettings));
        _modifiedInGame = false;
    }
}
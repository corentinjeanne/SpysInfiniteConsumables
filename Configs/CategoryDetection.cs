using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using Newtonsoft.Json;

namespace SPIC.Configs;

public struct DetectedCategories {
    public readonly Infinities.ConsumableCategory? Consumable;
    public readonly Infinities.PlaceableCategory? Placeable;

    public readonly bool GrabBag;
    public readonly bool Explosive;

    public DetectedCategories(Infinities.ConsumableCategory? consumable, Infinities.PlaceableCategory? placeable, bool grabBag, bool explosive) {
        Consumable = consumable; Placeable = placeable;
        GrabBag = grabBag; Explosive = explosive;
    }
}

[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static CategoryDetection Instance => _instance ??= ModContent.GetInstance<CategoryDetection>();
    private static CategoryDetection _instance;


    // [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    [Label("$Mods.SPIC.Configs.Detection.Categories.Consumables")]
    public Dictionary<ItemDefinition, Infinities.ConsumableCategory> DetectedConsumables = new();
    [Label("$Mods.SPIC.Configs.Detection.Categories.Explosives")]
    public HashSet<ItemDefinition> DetectedExplosives = new();
    [Label("$Mods.SPIC.Configs.Detection.Categories.Bags")]
    public HashSet<ItemDefinition> DetectedGrabBags = new();
    [Label("$Mods.SPIC.Configs.Detection.Categories.WandAmmo")]
    public Dictionary<ItemDefinition, Infinities.PlaceableCategory> DetectedWandAmmo = new();
    public void DetectedConsumable(Terraria.Item item, Infinities.ConsumableCategory consumable) {
        ItemDefinition key = new(item.type);
        if (IsExplosive(item.type) || !DetectedConsumables.TryAdd(key, consumable)) return;
        InfinityManager.ClearCache(item);
        _modifiedInGame = true;
    }

    public void DetectedPlaceable(Terraria.Item item, Infinities.PlaceableCategory placeable) {
        ItemDefinition key = new(item.type);
        if (!DetectedWandAmmo.TryAdd(key, placeable)) return;
        InfinityManager.ClearCache(item);
        _modifiedInGame = true;
    }

    public bool DetectedExplosive(Terraria.Item item) {
        ItemDefinition key = new(item.type);
        if (!DetectedExplosives.Add(key)) return false;
        DetectedConsumables.Remove(key);
        InfinityManager.ClearCache(item);
        _modifiedInGame = true;
        return true;
    }
    public bool IsExplosive(int type) => DetectedExplosives.Contains(new(type));


    public void DetectedGrabBag(Terraria.Item item) {
        if (DetectedGrabBags.Add(new(item.type))) _modifiedInGame = true;
        InfinityManager.ClearCache(item);
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
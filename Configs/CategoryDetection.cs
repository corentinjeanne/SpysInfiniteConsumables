using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using Newtonsoft.Json;

namespace SPIC.Configs;

public readonly record struct DetectedCategories(Infinities.UsableCategory? Usable, Infinities.PlaceableCategory? Placeable, bool GrabBag, bool Explosive);

// TODO InfinityDefinition class
[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static CategoryDetection Instance => _instance ??= ModContent.GetInstance<CategoryDetection>();
    private static CategoryDetection _instance;


    // [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    // TODO cutom elements
    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    public Dictionary<string, Dictionary<ItemDefinition, byte>> DetectedCategories = new();

    public bool SaveDetectedCategory(Item item, byte category, int infinityID){
        if(category == Infinities.Infinity.UnknownCategory) throw new("A detected category cannot be unkonwn");
        Infinities.Infinity infinity = InfinityManager.Infinity(infinityID);
        if(!infinity.CategoryDetection) return false;

        ItemDefinition key = new (item.type);
        if (!DetectedCategories[infinity.Name].TryAdd(key, category)) return false;
        InfinityManager.ClearCache(item);
        _modifiedInGame = true;
        return true;
    }

    public bool HasDetectedCategory(int type, int infinityID, out byte category)
        => (category = GetDetectedCategory(type, infinityID)) != Infinities.Infinity.UnknownCategory;
    
    public byte GetDetectedCategory(int type, int infinityID){
        if(!DetectMissing) return Infinities.Infinity.UnknownCategory;
        Infinities.Infinity infinity = InfinityManager.Infinity(infinityID);
        if (!infinity.CategoryDetection) return Infinities.Infinity.UnknownCategory;

        return DetectedCategories.TryGetValue(infinity.Name, out var categories) && categories.TryGetValue(new(type), out byte category)
            ? category
            : Infinities.Infinity.UnknownCategory;
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
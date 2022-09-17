using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using SPIC.ConsumableTypes;
using SPIC.Configs.UI;

namespace SPIC.Configs;

[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {

    [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, byte>> DetectedCategories {
        get => _detectedCategories;
        set {
            foreach(ConsumableType type in InfinityManager.ConsumableTypes){
                if(type is IDetectable) value.TryAdd(type.ToDefinition(), new());
                else value.Remove(type.ToDefinition());
            }
            _detectedCategories = value;
        }
    }
    private Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, byte>> _detectedCategories = new();

    public bool SaveDetectedCategory(Item item, byte category, int typeID){
        if(category == ConsumableType.UnknownCategory) throw new System.ArgumentException("A detected category cannot be unkonwn");
        ConsumableType type = InfinityManager.ConsumableType(typeID);
        if(type is not IDetectable) return false;

        ItemDefinition key = new (item.type);
        if (!DetectedCategories[type.ToDefinition()].TryAdd(key, category)) return false;
        InfinityManager.ClearCache(item.type);
        _modifiedInGame = true;
        return true;
    }
    public bool HasDetectedCategory(int type, int typeID, out byte category){
        category = ConsumableType.NoCategory;
        ConsumableType consumableType = InfinityManager.ConsumableType(typeID);
        return DetectMissing && consumableType is IDetectable
            && DetectedCategories.TryGetValue(consumableType.ToDefinition(), out var categories)&& categories.TryGetValue(new(type), out category);
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static CategoryDetection Instance;

    private bool _modifiedInGame;
    public void ManualSave() {
        if (_modifiedInGame) this.SaveConfig();
        _modifiedInGame = false;
    }
}
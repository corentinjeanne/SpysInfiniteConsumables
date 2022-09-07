using System.Collections.Generic;
using System.ComponentModel;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableTypes;

namespace SPIC.Configs;

public readonly record struct DetectedCategories(ConsumableTypes.UsableCategory? Usable, ConsumableTypes.PlaceableCategory? Placeable, bool GrabBag, bool Explosive);

// TODO InfinityDefinition && UI class
[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {

    // [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    // TODO UI
    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]

    private Dictionary<string, Dictionary<ItemDefinition, byte>> _detectedCategories = new();
    public Dictionary<string, Dictionary<ItemDefinition, byte>> DetectedCategories{
        get => _detectedCategories;
        set {
            // Keep the old ones and add the mising ones
            for (int i = 0; i < InfinityManager.InfinityCount; i++) {
                ConsumableType type = InfinityManager.ConsumableType(i);
                if(type is IDetectable) value.TryAdd(type.Name, new());
                else value.Remove(type.Name);
            }
            _detectedCategories = value;
        }
    }

    public bool SaveDetectedCategory(Item item, byte category, int typeID){
        if(category == ConsumableType.UnknownCategory) throw new("A detected category cannot be unkonwn");
        ConsumableType type = InfinityManager.ConsumableType(typeID);
        if(type is not IDetectable) return false;

        ItemDefinition key = new (item.type);
        if (!DetectedCategories[type.Name].TryAdd(key, category)) return false;
        InfinityManager.ClearCache(item.type);
        _modifiedInGame = true;
        return true;
    }

    public bool HasDetectedCategory(int type, int typeID, out byte category)
        => (category = GetDetectedCategory(type, typeID)) != ConsumableType.UnknownCategory;
    
    public byte GetDetectedCategory(int type, int typeID){
        if(!DetectMissing) return ConsumableType.UnknownCategory;
        ConsumableType consumable = InfinityManager.ConsumableType(typeID);
        if (consumable is not IDetectable) return ConsumableType.UnknownCategory;

        return DetectedCategories.TryGetValue(consumable.Name, out var categories) && categories.TryGetValue(new(type), out byte category)
            ? category
            : ConsumableType.UnknownCategory;
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static CategoryDetection Instance;

    private bool _modifiedInGame = false;
    public void ManualSave() {
        if (_modifiedInGame) this.SaveConfig();
        _modifiedInGame = false;
    }
}
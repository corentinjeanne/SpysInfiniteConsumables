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
    public Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, Category>> DetectedCategories {
        get => _detectedCategories;
        set {
            foreach(IConsumableType type in InfinityManager.ConsumableTypes(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)){
                ConsumableTypeDefinition def = type.ToDefinition();
                if (type is not IDetectable) {
                    value.Remove(def);
                    continue;
                }
                value.TryAdd(def, new());
                System.Type iConsumableGeneric = def.ConsumableType.GetType().GetInterfaces().Find(t => t.GetGenericTypeDefinition() == typeof(IConsumableType<>));
                if (iConsumableGeneric != null) {
                    foreach (ItemDefinition item in value[def].Keys) {
                        value[def][item] = (System.Enum)System.Enum.ToObject(iConsumableGeneric.GenericTypeArguments[0], value[def][item]);
                    }
                }
            }
            _detectedCategories = value;
        }
    }
    private Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, Category>> _detectedCategories = new();

    public bool SaveDetectedCategory(Item item, Category category, int typeID){
        if(category.IsUnknown) throw new System.ArgumentException("A detected category cannot be unkonwn");
        IConsumableType type = InfinityManager.ConsumableType(typeID);
        if(type is not IDetectable) return false;

        ItemDefinition key = new (item.type);
        if (!DetectedCategories[type.ToDefinition()].TryAdd(key, category)) return false;
        InfinityManager.ClearCache(item.type);
        return true;
    }
    public bool HasDetectedCategory(int type, int typeID, out Category category){
        IConsumableType consumableType = InfinityManager.ConsumableType(typeID);
        category = Category.None;
        return DetectMissing && consumableType is IDetectable
            && DetectedCategories.TryGetValue(consumableType.ToDefinition(), out Dictionary<ItemDefinition, Category> categories) && categories.TryGetValue(new(type), out category);
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static CategoryDetection Instance;

    public void UpdateProperties() {
        this.SaveConfig();
    }
}
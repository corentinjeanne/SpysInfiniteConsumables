using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using SPIC.ConsumableTypes;
using SPIC.Configs.UI;
using Terraria.ModLoader;

namespace SPIC.Configs;

[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {

    [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    [CustomModConfigItem(typeof(CustomDictionaryUI)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, CategoryWrapper>> DetectedCategories {
        get => _detectedCategories;
        set {
            _detectedCategories.Clear();
            foreach((ConsumableTypeDefinition def, Dictionary<ItemDefinition, CategoryWrapper> items) in value){
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _detectedCategories.Add(def, items);
                    continue;
                }
                if (def.ConsumableType is not IDetectable type) continue;

                System.Type iConsumableGeneric = type.GetType().GetInterfaces().Find(t => t.GetGenericTypeDefinition() == typeof(IConsumableType<>));
                if (iConsumableGeneric != null) {
                    _detectedCategories[def] = new();
                    foreach (ItemDefinition item in value[def].Keys) {
                        _detectedCategories[def][item] = value[def][item].IsEnum ?
                            value[def][item] :
                            new(System.Enum.ToObject(iConsumableGeneric.GenericTypeArguments[0], value[def][item].value));
                        _detectedCategories[def][item].SaveEnumType = false;
                    }
                } else 
                    _detectedCategories[def] = items;
            }
            foreach(IDetectable type in InfinityManager.ConsumableTypes<IDetectable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)){
                _detectedCategories.TryAdd(type.ToDefinition(), new());
            }
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, CategoryWrapper>> _detectedCategories = new();

    public bool SaveDetectedCategory(Item item, Category category, int typeID){
        if(category.IsUnknown) throw new System.ArgumentException("A detected category cannot be unkonwn");
        IConsumableType type = InfinityManager.ConsumableType(typeID);
        if(type is not IDetectable) return false;

        ItemDefinition key = new (item.type);
        if (!DetectedCategories[type.ToDefinition()].TryAdd(key, new(category.Value))) return false;
        InfinityManager.ClearCache(item.type);
        return true;
    }
    public bool HasDetectedCategory(int type, int typeID, out Category category){
        IConsumableType consumableType = InfinityManager.ConsumableType(typeID);
        category = Category.None;
        if(DetectMissing && consumableType is IDetectable
                && DetectedCategories.TryGetValue(consumableType.ToDefinition(), out Dictionary<ItemDefinition, CategoryWrapper> categories) && categories.TryGetValue(new(type), out CategoryWrapper wrapper)){
            category = wrapper;
            return true;
        }
        return false;
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static CategoryDetection Instance;

    public void UpdateProperties() {
        this.SaveConfig();
    }
}
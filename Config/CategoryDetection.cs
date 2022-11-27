using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Config.UI;
using Terraria.ModLoader;

namespace SPIC.Config;

[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {

    [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
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

                System.Type? iCategoryGen2 = type.GetType().GetInterfaces().Find(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICategory<,>));
                if (iCategoryGen2 != null) {
                    _detectedCategories[def] = new();
                    foreach (ItemDefinition item in value[def].Keys) {
                        _detectedCategories[def][item] = value[def][item].IsEnum ?
                            value[def][item] :
                            new(System.Enum.ToObject(iCategoryGen2.GenericTypeArguments[1], value[def][item].value));
                        _detectedCategories[def][item].SaveEnumType = false;
                    }
                } else 
                    _detectedCategories[def] = items;
            }
            foreach(IDetectable type in InfinityManager.ConsumableGroups<IDetectable>(FilterFlags.NonGlobal | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)){
                _detectedCategories.TryAdd(type.ToDefinition(), new());
            }
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, CategoryWrapper>> _detectedCategories = new();

    public bool SaveDetectedCategory(Item item, Category category, int typeID){
        if(category.IsUnknown) throw new System.ArgumentException("A detected category cannot be unkonwn");
        IConsumableGroup type = InfinityManager.ConsumableGroup(typeID);
        if(type is not IDetectable) return false;

        ItemDefinition key = new (item.type);
        if (!DetectedCategories[type.ToDefinition()].TryAdd(key, new(category.Value))) return false;
        InfinityManager.ClearCache(item);
        return true;
    }
    public bool HasDetectedCategory(int type, int typeID, out Category category){
        IConsumableGroup consumableType = InfinityManager.ConsumableGroup(typeID);
        category = Category.None;
        if(DetectMissing && consumableType is IDetectable
                && DetectedCategories.TryGetValue(consumableType.ToDefinition(), out Dictionary<ItemDefinition, CategoryWrapper>? categories) && categories.TryGetValue(new(type), out CategoryWrapper? wrapper)){
            category = wrapper;
            return true;
        }
        return false;
    }

    public override ConfigScope Mode => ConfigScope.ClientSide;
#nullable disable
    public static CategoryDetection Instance;
#nullable restore

    public void UpdateProperties() {
        this.SaveConfig();
    }
}
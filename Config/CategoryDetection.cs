using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Config.UI;
using Terraria.ModLoader;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Config;

[Label("$Mods.SPIC.Configs.Detection.name")]
public class CategoryDetection : ModConfig {

    [Header("$Mods.SPIC.Configs.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.Detection.General.Detect"), Tooltip("$Mods.SPIC.Configs.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Configs.Detection.Categories.header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, CategoryWrapper>> DetectedItem {
        get => _detectedItems;
        set {
            _detectedItems.Clear();
            foreach((ConsumableTypeDefinition def, Dictionary<ItemDefinition, CategoryWrapper> items) in value){
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _detectedItems.Add(def, items);
                    continue;
                }
                IConsumableGroup group = def.ConsumableType;
                if(group.UID < 0 || group is not IDetectable detectable) continue;

                System.Type? iCategoryGen2 = detectable.GetType().GetInterfaces().Find(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICategory<,>));
                if (iCategoryGen2 != null) {
                    _detectedItems[def] = new();
                    foreach (ItemDefinition key in value[def].Keys) {
                        _detectedItems[def][key] = value[def][key].IsEnum ?
                            value[def][key] :
                            new(System.Enum.ToObject(iCategoryGen2.GenericTypeArguments[1], value[def][key].value));
                        _detectedItems[def][key].SaveEnumType = false;
                    }
                } else 
                    _detectedItems[def] = items;
            }
            foreach(IDetectable type in InfinityManager.ConsumableGroups<IDetectable>(FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled, true)){
                _detectedItems.TryAdd(type.ToDefinition(), new());
            }
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, Dictionary<ItemDefinition, CategoryWrapper>> _detectedItems = new();
    
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableTypeDefinition, Dictionary<string, CategoryWrapper>> DetectedGlobals {
        get => _detectedGlobals;
        set {
            _detectedGlobals.Clear();
            foreach((ConsumableTypeDefinition def, Dictionary<string, CategoryWrapper> items) in value){
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _detectedGlobals.Add(def, items);
                    continue;
                }
                IConsumableGroup group = def.ConsumableType;
                if (group.UID > 0 || group is not IDetectable detectable) continue;

                System.Type? iCategoryGen2 = detectable.GetType().GetInterfaces().Find(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICategory<,>));
                if (iCategoryGen2 != null) {
                    _detectedGlobals[def] = new();
                    foreach (string key in value[def].Keys) {
                        _detectedGlobals[def][key] = value[def][key].IsEnum ?
                            value[def][key] :
                            new(System.Enum.ToObject(iCategoryGen2.GenericTypeArguments[1], value[def][key].value));
                        _detectedGlobals[def][key].SaveEnumType = false;
                    }
                } else 
                    _detectedGlobals[def] = items;
            }
            foreach(IDetectable group in InfinityManager.ConsumableGroups<IDetectable>(FilterFlags.Global | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled, true)){
                _detectedGlobals.TryAdd(group.ToDefinition(), new());
            }
        }
    }
    private readonly Dictionary<ConsumableTypeDefinition, Dictionary<string, CategoryWrapper>> _detectedGlobals = new();

    public bool SaveDetectedCategory(Item item, Category category, int groupID){
        if(category.IsUnknown) throw new System.ArgumentException("A detected category cannot be unkonwn");

        IConsumableGroup group = InfinityManager.ConsumableGroup(groupID);
        if(groupID > 0 && group is IDetectable){
            if (!DetectedItem[group.ToDefinition()].TryAdd(new(item.type), new(category.Value){SaveEnumType = false})) return false;
        }
        else if(groupID < 0 && group is IDetectable detectable){
            if (!DetectedGlobals[group.ToDefinition()].TryAdd(detectable.ToKey(detectable.ToConsumable(item)), new(category.Value){ SaveEnumType = false })) return false;
        }
        else return false;

        InfinityManager.ClearCache(item);
        return true;

    }
    public bool HasDetectedCategory(Item item, int groupID, [MaybeNullWhen(false)] out Category? category){
        IConsumableGroup group = InfinityManager.ConsumableGroup(groupID);
        if(DetectMissing && group is IDetectable detectable &&
            (groupID > 0 ?
                DetectedItem.TryGetValue(group.ToDefinition(), out Dictionary<ItemDefinition, CategoryWrapper>? itemCats) && itemCats.TryGetValue(new(item.type), out CategoryWrapper? wrapper) : 
                DetectedGlobals.TryGetValue(group.ToDefinition(), out Dictionary<string, CategoryWrapper>? globalCats) && globalCats.TryGetValue(detectable.ToKey(group.ToConsumable(item)), out wrapper)) ){
            category = wrapper;
            return true;
        }
        category = null;
        return false;
    }
    public bool HasDetectedCategory(object consumable, int groupID, [MaybeNullWhen(false)] out Category? category){
        IConsumableGroup group = InfinityManager.ConsumableGroup(groupID);
        if(DetectMissing && group is IDetectable detectable &&
            (groupID > 0 ?
                DetectedItem.TryGetValue(group.ToDefinition(), out Dictionary<ItemDefinition, CategoryWrapper>? itemCats) && itemCats.TryGetValue(new(((Item)consumable).type), out CategoryWrapper? wrapper) : 
                DetectedGlobals.TryGetValue(group.ToDefinition(), out Dictionary<string, CategoryWrapper>? globalCats) && globalCats.TryGetValue(detectable.ToKey(consumable), out wrapper)) ){
            category = wrapper;
            return true;
        }
        category = null;
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
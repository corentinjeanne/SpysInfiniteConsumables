using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Config.UI;
using Terraria.ModLoader;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Config;

[Label("$Mods.SPIC.Config.Detection.name")]
public class CategoryDetection : ModConfig {

    [Header("$Mods.SPIC.Config.Detection.General.header")]
    [DefaultValue(true), Label("$Mods.SPIC.Config.Detection.General.Detect"), Tooltip("$Mods.SPIC.Config.Detection.General.t_detect")]
    public bool DetectMissing;

    [Header("$Mods.SPIC.Config.Detection.Categories.header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableGroupDefinition, Dictionary<ItemDefinition, CategoryWrapper>> DetectedItem {
        get => _detectedItems;
        set {
            _detectedItems.Clear();
            foreach((ConsumableGroupDefinition def, Dictionary<ItemDefinition, CategoryWrapper> items) in value){
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
            foreach(IDetectable group in InfinityManager.ConsumableGroups<IDetectable>(FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled, true)){
                _detectedItems.TryAdd(group.ToDefinition(), new());
            }
        }
    }
    [CustomModConfigItem(typeof(CustomDictionaryElement)), ValuesAsConfigItems, ConstantKeys]
    public Dictionary<ConsumableGroupDefinition, Dictionary<string, CategoryWrapper>> DetectedGlobals {
        get => _detectedGlobals;
        set {
            _detectedGlobals.Clear();
            foreach((ConsumableGroupDefinition def, Dictionary<string, CategoryWrapper> items) in value){
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


    public bool SaveDetectedCategory<TConsumable, TCategory>(TConsumable consumable, TCategory category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        if(System.Convert.ToByte(category) == Category.Unknown) throw new System.ArgumentException("A detected category cannot be unkonwn");
        
        CategoryWrapper wrapper = new(category) { SaveEnumType = false };
        if (group is not IDetectable || (group.UID > 0 ?
                (!DetectedItem[group.ToDefinition()].TryAdd(new((consumable as Item)!.type), wrapper)) :
                (!DetectedGlobals[group.ToDefinition()].TryAdd(group.Key(consumable), wrapper)))) {
            return false;
        }

        InfinityManager.ClearCache(consumable, group);
        return true;

    }

    public bool HasDetectedCategory<TConsumable, TCategory>(TConsumable consumable, [NotNullWhen(true)] out TCategory? category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        if(DetectMissing && group is IDetectable &&
            ((group.UID > 0 && consumable is Item item) ?
                (DetectedItem.TryGetValue(group.ToDefinition(), out Dictionary<ItemDefinition, CategoryWrapper>? itemCats) && itemCats.TryGetValue(new(item.type), out CategoryWrapper? wrapper)) : 
                (DetectedGlobals.TryGetValue(group.ToDefinition(), out Dictionary<string, CategoryWrapper>? globalCats) && globalCats.TryGetValue(group.Key(consumable), out wrapper)))){
            category = (TCategory)(Category)wrapper;
            return true;
        }
        category = default;
        return false;
    }


    private readonly Dictionary<ConsumableGroupDefinition, Dictionary<ItemDefinition, CategoryWrapper>> _detectedItems = new();
    private readonly Dictionary<ConsumableGroupDefinition, Dictionary<string, CategoryWrapper>> _detectedGlobals = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
#nullable disable
    public static CategoryDetection Instance;
#nullable restore

}
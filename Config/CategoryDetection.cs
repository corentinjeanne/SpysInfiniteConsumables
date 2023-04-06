using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using SPIC.ConsumableGroup;
using SPIC.Configs.UI;
using Terraria.ModLoader;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Configs;

[Label($"${Localization.Keys.CategoryDetection}.Name")]
public class CategoryDetection : ModConfig {

    [Header($"${Localization.Keys.CategoryDetection}.General.Header")]
    [DefaultValue(true), Label($"${Localization.Keys.CategoryDetection}.General.Detect.Label"), Tooltip($"${Localization.Keys.CategoryDetection}.General.Detect.Tooltip")]
    public bool DetectMissing;


    [Header($"${Localization.Keys.CategoryDetection}.Categories.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ConsumableGroupDefinition, Dictionary<ItemDefinition, CategoryWrapper>> DetectedCategories {
        get => _detectedCategories;
        set {
            _detectedCategories.Clear();
            foreach ((ConsumableGroupDefinition def, Dictionary<ItemDefinition, CategoryWrapper> items) in value) {
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _detectedCategories[def] = items;
                    continue;
                }
                IConsumableGroup group = def.ConsumableGroup;
                if (group is IDetectable && group.GetType().ImplementsInterface(typeof(ICategory<,>), out System.Type? iCategoryGen2)) {
                    _detectedCategories[def] = new();
                    foreach (ItemDefinition key in value[def].Keys) {
                        _detectedCategories[def][key] = value[def][key].type is not null ?
                            value[def][key] :
                            new(value[def][key].value, iCategoryGen2.GenericTypeArguments[1]);
                        _detectedCategories[def][key].SaveEnumType = false;
                    }
                } else {
                    _detectedCategories[def] = items;
                }

            }
            foreach (IDetectable group in InfinityManager.ConsumableGroups<IDetectable>(FilterFlags.Global | FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled, true)) {
                if (!group.GetType().ImplementsInterface(typeof(ICategory<,>), out _)) continue;
                _detectedCategories.TryAdd(group.ToDefinition(), new());
            }
        }
    }

    public bool SaveDetectedCategory<TConsumable, TCategory>(Item representative, TCategory category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        if(System.Convert.ToByte(category) == CategoryHelper.Unknown) throw new System.ArgumentException("A detected category cannot be unkonwn");

        CategoryWrapper wrapper = CategoryWrapper.From(category);
        if (group is not IDetectable || !DetectedCategories[group.ToDefinition()].TryAdd(new(representative.type), wrapper))
            return false;
        
        InfinityManager.ClearConsumableCache(group.ToConsumable(representative), group);
        return true;

    }
    public bool HasDetectedCategory<TConsumable, TCategory>(TConsumable consumable, [NotNullWhen(true)] out TCategory? category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        if(DetectMissing && group is IDetectable) {
            int id = group.CacheID(consumable);
            foreach((ItemDefinition def, CategoryWrapper wrapper) in DetectedCategories[group.ToDefinition()]) {
                if (group.CacheID(group.ToConsumable(new Item(def.Type))) != id) continue;
                category = wrapper.As<TCategory>();
                return true;
            }
        }
        category = default;
        return false;
    }

    private readonly Dictionary<ConsumableGroupDefinition, Dictionary<ItemDefinition, CategoryWrapper>> _detectedCategories = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    public static CategoryDetection Instance = null!;
}
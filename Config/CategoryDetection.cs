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
    public Dictionary<ConsumableGroupDefinition, Dictionary<ItemDefinition, CategoryWrapper>> DetectedItem {
        get => _detectedItems;
        set => SetupGroups(_detectedItems, value, FilterFlags.NonGlobal | FilterFlags.Enabled | FilterFlags.Disabled);
    }

    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ConsumableGroupDefinition, Dictionary<string, CategoryWrapper>> DetectedGlobals {
        get => _detectedGlobals;
        set => SetupGroups(_detectedGlobals, value, FilterFlags.Global | FilterFlags.Global | FilterFlags.Enabled | FilterFlags.Disabled);
    }


    public bool SaveDetectedCategory<TConsumable, TCategory>(TConsumable consumable, TCategory category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        if(System.Convert.ToByte(category) == CategoryHelper.Unknown) throw new System.ArgumentException("A detected category cannot be unkonwn");

        CategoryWrapper wrapper = CategoryWrapper.From(category);
        if (group is not IDetectable || (group.UID > 0 ?
                (!DetectedItem[group.ToDefinition()].TryAdd(new((consumable as Item)!.type), wrapper)) :
                (!DetectedGlobals[group.ToDefinition()].TryAdd(group.Key(consumable), wrapper)))) { // TODO rework like customs
            return false;
        }

        InfinityManager.ClearConsumableCache(consumable, group);
        return true;

    }
    public bool HasDetectedCategory<TConsumable, TCategory>(TConsumable consumable, [NotNullWhen(true)] out TCategory? category, ICategory<TConsumable, TCategory> group) where TConsumable : notnull where TCategory : System.Enum{
        if(DetectMissing && group is IDetectable &&
            ((group.UID > 0 && consumable is Item item) ?
                (DetectedItem.TryGetValue(group.ToDefinition(), out Dictionary<ItemDefinition, CategoryWrapper>? itemCats) && itemCats.TryGetValue(new(item.type), out CategoryWrapper? wrapper)) : 
                (DetectedGlobals.TryGetValue(group.ToDefinition(), out Dictionary<string, CategoryWrapper>? globalCats) && globalCats.TryGetValue(group.Key(consumable), out wrapper)))){
            category = wrapper.As<TCategory>();
            return true;
        }
        category = default;
        return false;
    }


    private static void SetupGroups<TKey>(Dictionary<ConsumableGroupDefinition, Dictionary<TKey, CategoryWrapper>> dest, Dictionary<ConsumableGroupDefinition, Dictionary<TKey, CategoryWrapper>> source, FilterFlags groupFlags) where TKey : notnull {
        dest.Clear();
        foreach ((ConsumableGroupDefinition def, Dictionary<TKey, CategoryWrapper> items) in source) {
            if (def.IsUnloaded) {
                if (!ModLoader.HasMod(def.Mod)) dest[def] = items;
                continue;
            }
            IConsumableGroup group = def.ConsumableGroup;
            if (group is IDetectable && group.GetType().ImplementsInterface(typeof(ICategory<,>), out System.Type? iCategoryGen2)) {
                dest[def] = new();
                foreach (TKey key in source[def].Keys) {
                    dest[def][key] = source[def][key].type is not null ?
                        source[def][key] :
                        new(source[def][key].value, iCategoryGen2.GenericTypeArguments[1]);
                    dest[def][key].SaveEnumType = false;
                }
            } else {
                dest[def] = items;
            }

        }
        foreach (IDetectable group in InfinityManager.ConsumableGroups<IDetectable>(groupFlags, true)) {
            if(!group.GetType().ImplementsInterface(typeof(ICategory<,>), out _)) continue;
            dest.TryAdd(group.ToDefinition(), new());
        }
    }

    private readonly Dictionary<ConsumableGroupDefinition, Dictionary<ItemDefinition, CategoryWrapper>> _detectedItems = new();
    private readonly Dictionary<ConsumableGroupDefinition, Dictionary<string, CategoryWrapper>> _detectedGlobals = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    public static CategoryDetection Instance = null!;
}
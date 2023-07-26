using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using SPIC.Configs.UI;
using System.Diagnostics.CodeAnalysis;

namespace SPIC.Configs;

public class CategoryDetection : ModConfig {

    [Header($"${Localization.Keys.CategoryDetection}.General.Header")]
    [DefaultValue(true)]
    public bool DetectMissing;

    [Header($"${Localization.Keys.CategoryDetection}.Categories.Header")]
    [CustomModConfigItem(typeof(CustomDictionaryElement))]
    public Dictionary<ModGroupDefinition, Dictionary<ItemDefinition, GenericWrapper>> DetectedCategories {
        get => _detectedCategories;
        set {
            _detectedCategories.Clear();
            foreach ((ModGroupDefinition def, Dictionary<ItemDefinition, GenericWrapper> items) in value) {
                if (def.IsUnloaded) continue;
                IModGroup group = InfinityManager.GetModGroup(def.Mod, def.Name)!;
                if (group.GetType().IsSubclassOfGeneric(typeof(ModGroup<,,>), out System.Type? modGroup3)) {
                    _detectedCategories[def] = new();
                    foreach (ItemDefinition key in value[def].Keys) {
                        _detectedCategories[def][key] = value[def][key].GetType() == typeof(GenericWrapper) ?
                            value[def][key].MakeGeneric(modGroup3.GenericTypeArguments[2]):
                            value[def][key];
                    }
                } else {
                    _detectedCategories[def] = items;
                }
            }
        }
    }

    public bool SaveDetectedCategory<TMetaGroup, TConsumable, TCategory>(TConsumable consumable, TCategory category, ModGroup<TMetaGroup, TConsumable, TCategory> group) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> where TCategory : System.Enum, new() {
        if (InfinityManager.GetCategory(consumable, group).Equals(category)) return false;
        TMetaGroup metaGroup = group.MetaGroup;

        GenericWrapper<TCategory> wrapper = new(category);
        DetectedCategories.TryAdd(new(group), new());
        if (!DetectedCategories[new(group)].TryAdd(new(metaGroup.ToItem(consumable).type), wrapper)) return false;
        metaGroup.ClearInfinities();
        return true; 
    }
    public bool HasDetectedCategory<TMetaGroup, TConsumable, TCategory>(TConsumable consumable, ModGroup<TMetaGroup, TConsumable, TCategory> group, [NotNullWhen(true)] out TCategory? category) where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> where TCategory : System.Enum {
        if(DetectMissing && DetectedCategories.TryGetValue(new(group), out Dictionary<ItemDefinition, GenericWrapper>? categories)) {
            TMetaGroup metaGroup = group.MetaGroup;
            ItemDefinition def = new(metaGroup.ToItem(consumable).type);
            if(categories.TryGetValue(def, out GenericWrapper? wrapper)){
                category = (TCategory)wrapper.Value!;
                return true;
            }
        }
        category = default;
        return false;
    }
    private readonly Dictionary<ModGroupDefinition, Dictionary<ItemDefinition, GenericWrapper>> _detectedCategories = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    public static CategoryDetection Instance = null!;
}
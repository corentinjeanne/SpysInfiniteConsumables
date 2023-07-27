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
    public Dictionary<InfinityDefinition, Dictionary<ItemDefinition, GenericWrapper<object>>> DetectedCategories {
        get => _detectedCategories;
        set {
            _detectedCategories.Clear();
            foreach ((InfinityDefinition def, Dictionary<ItemDefinition, GenericWrapper<object>> items) in value) {
                if (def.IsUnloaded) continue;
                IInfinity intinity = InfinityManager.GetInfinity(def.Mod, def.Name)!;
                if (!intinity.GetType().IsSubclassOfGeneric(typeof(Infinity<,,>), out System.Type? intinity3)) continue;
                _detectedCategories[def] = new();
                foreach (ItemDefinition key in value[def].Keys) _detectedCategories[def][key] = value[def][key].MakeGeneric(intinity3.GenericTypeArguments[2]);
            }
        }
    }

    public bool SaveDetectedCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, TCategory category, Infinity<TGroup, TConsumable, TCategory> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : System.Enum, new() {
        if (InfinityManager.GetCategory(consumable, infinity).Equals(category)) return false;
        TGroup group = infinity.Group;

        GenericWrapper<TCategory, object> wrapper = new(category);
        DetectedCategories.TryAdd(new(infinity), new());
        if (!DetectedCategories[new(infinity)].TryAdd(new(group.ToItem(consumable).type), wrapper)) return false;
        group.ClearInfinities();
        return true; 
    }
    public bool HasDetectedCategory<TGroup, TConsumable, TCategory>(TConsumable consumable, Infinity<TGroup, TConsumable, TCategory> infinity, [NotNullWhen(true)] out TCategory? category) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull where TCategory : System.Enum {
        if(DetectMissing && DetectedCategories.TryGetValue(new(infinity), out Dictionary<ItemDefinition, GenericWrapper<object>>? categories)) {
            TGroup group = infinity.Group;
            ItemDefinition def = new(group.ToItem(consumable).type);
            if(categories.TryGetValue(def, out GenericWrapper<object>? wrapper)){
                category = (TCategory)wrapper.Value!;
                return true;
            }
        }
        category = default;
        return false;
    }
    private readonly Dictionary<InfinityDefinition, Dictionary<ItemDefinition, GenericWrapper<object>>> _detectedCategories = new();

    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    public static CategoryDetection Instance = null!;
}
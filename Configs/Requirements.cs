using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using System.IO;
using Newtonsoft.Json;
using SPIC.ConsumableTypes;
using Newtonsoft.Json.Linq;
using SPIC.Infinities;

namespace SPIC.Configs;

[NullAllowed]
public class CustomRequirement<T> where T : System.Enum {
    [Label("$Mods.SPIC.Categories.name")]
    public T Category;
    [Range(0, 50), Label("$Mods.SPIC.Configs.Requirements.Customs.requirement"), Tooltip("$Mods.SPIC.Configs.Requirements.Customs.t_requirement")]
    public int Requirement;
}

// TODO >>> rework into dict
public class Custom {
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Ammo")]
    public CustomRequirement<ConsumableTypes.AmmoCategory> Ammo;
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Usable")]
    public CustomRequirement<ConsumableTypes.UsableCategory> Usables;
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Placeable")] 
    public CustomRequirement<ConsumableTypes.PlaceableCategory> Placeable;
    [Label("$Mods.SPIC.Configs.Requirements.Customs.Bag")]
    public CustomRequirement<ConsumableTypes.GrabBagCategory> GrabBag;
    
    public Custom Set<T>(CustomRequirement<T> customRequirement) where T : System.Enum {
        System.Type type = typeof(T);
        if (type == typeof(ConsumableTypes.UsableCategory)) Usables = customRequirement as CustomRequirement<ConsumableTypes.UsableCategory>;
        else if (type == typeof(ConsumableTypes.AmmoCategory)) Ammo = customRequirement as CustomRequirement<ConsumableTypes.AmmoCategory>;
        else if (type == typeof(ConsumableTypes.GrabBagCategory)) GrabBag = customRequirement as CustomRequirement<ConsumableTypes.GrabBagCategory>;
        else if (type == typeof(ConsumableTypes.PlaceableCategory)) Placeable = customRequirement as CustomRequirement<ConsumableTypes.PlaceableCategory>;
        else throw new UsageException();
        return this;
    }

    public CustomCategories Categories() => new(
        Ammo?.Category == ConsumableTypes.AmmoCategory.None ? null : Ammo?.Category,
        Usables?.Category == ConsumableTypes.UsableCategory.None ? null : Usables?.Category,
        GrabBag?.Category == ConsumableTypes.GrabBagCategory.None ? null : GrabBag?.Category,
        Placeable?.Category == ConsumableTypes.PlaceableCategory.None ? null : Placeable?.Category
    );
    public CustomRequirements Requirements() => new(
        Ammo?.Category == ConsumableTypes.AmmoCategory.None ? Ammo.Requirement : null,
        Usables?.Category == ConsumableTypes.UsableCategory.None ? Usables.Requirement : null,
        GrabBag?.Category == ConsumableTypes.GrabBagCategory.None ? GrabBag.Requirement : null,
        Placeable?.Category == ConsumableTypes.PlaceableCategory.None ? Placeable.Requirement : null
    );

}

public readonly record struct CustomCategories(ConsumableTypes.AmmoCategory? Ammo, ConsumableTypes.UsableCategory? Usable, ConsumableTypes.GrabBagCategory? GrabBag, ConsumableTypes.PlaceableCategory? Placeable);

public readonly record struct CustomRequirements(int? Ammo, int? Usable, int? GrabBag, int? Placeable);

[Label("$Mods.SPIC.Configs.Requirements.name")]
public class Requirements : ModConfig {
    
    public enum InfinityPreset { // ? Rework to be more flexible
        None,
        Default,
        OneForAll,
        AllOf,
        AllOn,
        JourneyCosts
    }

    public InfinityPreset Preset {
        get {

            bool defaults = true;
            int on = 0;
            foreach ((string name, bool state) in Infinities) {
                Infinity inf = InfinityManager.Infinity(name);
                if (defaults && (inf == null || state != inf.DefaultValue)) defaults = false;
                if (state) on++;
            }
            if (on == 0) return InfinityPreset.AllOf;

            if (MaxInfinities == 0) {
                if (defaults) return InfinityPreset.Default;
                if (on == Infinities.Count) return InfinityPreset.AllOn;
            }

            if (MaxInfinities == 1) {
                if (ConsumableTypePriority[0] == JourneySacrifice.Instance.Name && Infinities[JourneyResearch.Instance.Name]) return InfinityPreset.JourneyCosts;
                return InfinityPreset.OneForAll;
            }

            return InfinityPreset.None;
        }
        set {
            switch (value) {
            case InfinityPreset.Default: // ? unloaded stuff
                Infinities = new();
                ConsumableTypePriority = new();
                MaxInfinities = 0;
                break;
            case InfinityPreset.OneForAll:
                MaxInfinities = 1;
                break;
            case InfinityPreset.JourneyCosts:
                Infinities[JourneyResearch.Instance.Name] = true;
                ConsumableTypePriority.Remove(JourneySacrifice.Instance.Name);
                ConsumableTypePriority.Insert(0, JourneySacrifice.Instance.Name);
                MaxInfinities = 1;
                break;
            case InfinityPreset.AllOn:
                foreach ((string name, bool _) in Infinities)
                    Infinities[name] = true;
                MaxInfinities = 0;
                break;
            case InfinityPreset.AllOf:
                foreach ((string name, bool _) in Infinities)
                    Infinities[name] = false;
                break;
            case InfinityPreset.None:
            default:
                break;
            }
        }
    }

    // Dictionary<InfinityDefinition, bool>
    private Dictionary<string, bool> _infinities = new();
    public Dictionary<string, bool> Infinities {
        get => _infinities;
        set {
            // Keep the old ones and add the mising ones
            for (int i = 0; i < InfinityManager.InfinityCount; i++) {
                Infinity infinity = InfinityManager.Infinity(i);
                value.TryAdd(infinity.Name, infinity.DefaultValue);
            }
            _infinities = value;
        }
    }

    // List<ConsumableTypeDefinition>
    private List<string> _consumableTypePriority = new(); // ? (string, bool) to allow disableing specific types
    public List<string> ConsumableTypePriority{
        get => _consumableTypePriority;
        set {
            // Keep the old ones and add the mising ones
            for (int i = 0; i < InfinityManager.ConsumableTypesCount; i++) {
                ConsumableType infinity = InfinityManager.ConsumableType(i);
                if (!value.Contains(infinity.Name))value.Add(infinity.Name);
            }
            _consumableTypePriority = value;
        }
    }

    public int MaxInfinities { get; set; }

    [DefaultValue(true), Label("$Mods.SPIC.Configs.Requirements.General.Duplication"), Tooltip("$Mods.SPIC.Configs.General.t_duplication")]
    public bool PreventItemDupication { get; set; }

    // Dictionary<InfinityFDefinition, Dictionary<ConsumableTypeDefinition, object>>
    private Dictionary<string, Dictionary<string, object>> _requirements = new();
    public Dictionary<string, Dictionary<string, object>> Reqs {
        get => _requirements;
        set {
            InfinityManager.ClearCache();
            // Re-add all past infinties
            foreach ((string inf, Dictionary<string, object> types) in value) {
                foreach ((string name, object data) in types) {
                    string jString = data switch {
                        string s => s,
                        JObject jObj => jObj.ToString(),
                        _ => null,
                    };

                    if (jString == null) continue;

                    ConsumableType type = InfinityManager.ConsumableType(name);
                    if (type is null) {
                        types[name] = jString;
                        continue;
                    }
                    JsonConvert.PopulateObject(jString, types[name]=type.Requirements=type.CreateRequirements(), ConfigManager.serializerSettings);
                }
            }
            // Add the mising ones
            for (int i = 0; i < InfinityManager.InfinityCount; i++){
                string name = InfinityManager.Infinity(i).Name;
                value.TryAdd(name, new());
                foreach (int typeID in InfinityManager.PairedConsumableType(i)) {
                    ConsumableType type = InfinityManager.ConsumableType(typeID);
                    if(!value[name].ContainsKey(type.Name))value[name].Add(type.Name, type.Requirements = type.CreateRequirements());
                }
            }
            _requirements = value;
        }
    }


    // TODO >>> reimplement customs
    // [Header("$Mods.SPIC.Configs.Requirements.Customs.header")]
    // [Label("$Mods.SPIC.Configs.Requirements.Customs.Customs")]
    // public Dictionary<ItemDefinition,Custom> customs = new();

    // public CustomCategories GetCustomCategories(int type) => customs.TryGetValue(new(type), out var custom) ? custom.Categories() : new();
    // public CustomRequirements GetCustomRequirements(int type) => customs.TryGetValue(new(type), out var custom) ? custom.Requirements() : new();

    // public void InGameSetCustom<T>(int type, CustomInfinity<T> customInfinity) where T : System.Enum{
    //     ItemDefinition key = new(type);
    //     if(Customs.TryGetValue(new(type), out Custom custom)) custom.Set(customInfinity);
    //     else Customs.Add(key,Custom.CreateWith(customInfinity));
    //     _modifiedInGame = true;
    // }

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public static Requirements Instance;

    private bool _modifiedInGame = false;
    public void ManualSave() {
        if (_modifiedInGame) this.SaveConfig();
        _modifiedInGame = false;
    }
}

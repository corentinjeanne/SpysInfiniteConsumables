using System.IO;
using System.Collections.Generic;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using Newtonsoft.Json;

namespace SPIC.Configs;

public struct AutoCategories {
    public readonly Categories.Consumable? Consumable;
    public readonly bool GrabBag;
    public readonly bool WandAmmo;
    public readonly bool Explosive;

    public AutoCategories(Categories.Consumable? consumable, bool grabBag, bool wandAmmo, bool explosive) {
        Consumable = consumable;
        GrabBag = grabBag;
        WandAmmo = wandAmmo;
        Explosive = explosive;
    }
}

public class CategorySettings : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static CategorySettings Instance => _instance ??= ModContent.GetInstance<CategorySettings>();
    private static CategorySettings _instance;

    [Label("$Mods.SPIC.Configs.General.CategoryLabel")]
    public bool ShowCategories;

    [Label("Automatic Categories")]
    public Dictionary<ItemDefinition, Categories.Consumable> autoConsumables = new();
    public HashSet<ItemDefinition> autoExplosives = new();
    public HashSet<ItemDefinition> autoGrabBags = new();
    public HashSet<ItemDefinition> autoWands = new();

    public void SaveConsumableCategory(int type, Categories.Consumable consumable) {
        ItemDefinition key = new(type);
        if (IsExplosive(type) || autoConsumables.TryGetValue(key, out _)) return;
        autoConsumables.Add(key, consumable);
        _modifiedInGame = true;
    }

    public bool SaveExplosive(int type) {
        ItemDefinition key = new(type);
        if (autoExplosives.Contains(key)) return false;
        autoExplosives.Add(key);
        autoConsumables.Remove(key);
        _modifiedInGame = true;
        return true;
    }
    public bool IsExplosive(int type) => autoExplosives.Contains(new(type));


    public void SaveGrabBagCategory(int type) {
        ItemDefinition key = new(type);
        if (autoGrabBags.Contains(key)) return;
        autoGrabBags.Add(key);
        _modifiedInGame = true;
    }

    public void SaveWandAmmoCategory(int type) {
        ItemDefinition key = new(type);
        if (autoWands.Contains(key)) return;
        autoWands.Add(key);
        _modifiedInGame = true;
    }

    public AutoCategories GetAutoCategories(int type){
        ItemDefinition key = new(type);
        return new(
            autoConsumables.ContainsKey(key) ? autoConsumables[key] : null,
            autoGrabBags.Contains(key),
            autoWands.Contains(key),
            autoExplosives.Contains(key)
        );
    }


    private static string _configPath;
    private bool _modifiedInGame = false;
    public override void OnLoaded() => _configPath = ConfigManager.ModConfigPath + $"\\{nameof(SPIC)}_{nameof(CategorySettings)}.json";
    public void ManualSave() {
        if (!_modifiedInGame) return;
        using StreamWriter sw = new(_configPath);
        sw.Write(JsonConvert.SerializeObject(this, ConfigManager.serializerSettings));
        _modifiedInGame = false;
    }
}
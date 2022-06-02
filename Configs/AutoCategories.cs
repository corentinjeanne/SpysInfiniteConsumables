using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

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
    [Label("$Mods.SPIC.Configs.General.InfinitesLabel")]
    public bool ShowInfinites;
    [Label("$Mods.SPIC.Configs.General.RequirementLabel")]
    public bool ShowRequirement;


    [Header("Automatic Categories")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.AutoLabel"), Tooltip("$Mods.SPIC.Configs.General.AutoTooltip")]
    public bool AutoCategories;

    private readonly Dictionary<ItemDefinition, Categories.Consumable> _autoConsumables = new();
    private readonly HashSet<ItemDefinition> _autoExplosives = new();
    private readonly HashSet<ItemDefinition> _autoGrabBags = new();
    private readonly HashSet<ItemDefinition> _autoWands = new();

    public void SaveConsumableCategory(int type, Categories.Consumable consumable) {
        ItemDefinition key = new(type);
        if (IsExplosive(type) || !_autoConsumables.TryAdd(key, consumable)) return;
        Category.UpdateItem(type);
        _modifiedInGame = true;
    }

    public bool SaveExplosive(int type) {
        ItemDefinition key = new(type);
        if (!_autoExplosives.Add(key)) return false;
        _autoConsumables.Remove(key);
        Category.UpdateItem(type);
        _modifiedInGame = true;
        return true;
    }
    public bool IsExplosive(int type) => _autoExplosives.Contains(new(type));


    public void SaveGrabBagCategory(int type) {
        if (_autoGrabBags.Add(new(type))) _modifiedInGame = true;
        Category.UpdateItem(type);
    }

    public void SaveWandAmmoCategory(int type) {
        if (!_autoWands.Add(new(type))) _modifiedInGame = true;
        Category.UpdateItem(type);
    }

    public AutoCategories GetAutoCategories(int type){
        if(!AutoCategories) return new();

        ItemDefinition key = new(type);
        return new(
            _autoConsumables.ContainsKey(key) ? _autoConsumables[key] : null,
            _autoGrabBags.Contains(key),
            _autoWands.Contains(key),
            _autoExplosives.Contains(key)
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
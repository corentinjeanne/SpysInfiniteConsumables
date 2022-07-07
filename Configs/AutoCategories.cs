using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using Newtonsoft.Json;

namespace SPIC.Configs;

public struct AutoCategories {
    public readonly Categories.Consumable? Consumable;
    public readonly Categories.Placeable? Placeable;

    public readonly bool GrabBag;
    public readonly bool Explosive;

    public AutoCategories(Categories.Consumable? consumable, Categories.Placeable? placeable, bool grabBag, bool explosive) {
        Consumable = consumable; Placeable = placeable;
        GrabBag = grabBag; Explosive = explosive;
    }
}

public class CategorySettings : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static CategorySettings Instance => _instance ??= ModContent.GetInstance<CategorySettings>();
    private static CategorySettings _instance;

    [Header("$Mods.SPIC.Configs.General.DisplayHeader")]
    [Label("$Mods.SPIC.Configs.General.f_Category")]
    public bool ShowCategories;
    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.f_Infinites")]
    public bool ShowInfinities;
    [Label("$Mods.SPIC.Configs.General.f_Requirement")]
    public bool ShowRequirement;


    [Header("Automatic Categories")]
    [DefaultValue(true), Label("$Mods.SPIC.Configs.General.f_Auto"), Tooltip("$Mods.SPIC.Configs.General.AutoTooltip")]
    public bool AutoCategories;

    private readonly Dictionary<ItemDefinition, Categories.Consumable> _autoConsumables = new();
    private readonly Dictionary<ItemDefinition, Categories.Placeable> _autoPlaceables = new();
    private readonly HashSet<ItemDefinition> _autoExplosives = new();
    private readonly HashSet<ItemDefinition> _autoGrabBags = new();

    public void SaveConsumableCategory(Terraria.Item item, Categories.Consumable consumable) {
        ItemDefinition key = new(item.type);
        if (IsExplosive(item.type) || !_autoConsumables.TryAdd(key, consumable)) return;
        Category.UpdateItem(item);
        _modifiedInGame = true;
    }

    public void SavePlaceableCategory(Terraria.Item item, Categories.Placeable placeable) {
        ItemDefinition key = new(item.type);
        if (!_autoPlaceables.TryAdd(key, placeable)) return;
        Category.UpdateItem(item);
        _modifiedInGame = true;
    }

    public bool SaveExplosive(Terraria.Item item) {
        ItemDefinition key = new(item.type);
        if (!_autoExplosives.Add(key)) return false;
        _autoConsumables.Remove(key);
        Category.UpdateItem(item);
        _modifiedInGame = true;
        return true;
    }
    public bool IsExplosive(int type) => _autoExplosives.Contains(new(type));


    public void SaveGrabBagCategory(Terraria.Item item) {
        if (_autoGrabBags.Add(new(item.type))) _modifiedInGame = true;
        Category.UpdateItem(item);
    }

    public AutoCategories GetAutoCategories(int type){
        if(!AutoCategories) return new();

        ItemDefinition key = new(type);
        return new(
            _autoConsumables.ContainsKey(key) ? _autoConsumables[key] : null,
            _autoPlaceables.ContainsKey(key) ? _autoPlaceables[key] : null,
            _autoGrabBags.Contains(key),
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
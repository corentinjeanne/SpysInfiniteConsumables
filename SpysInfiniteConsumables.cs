using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC;

// TODO documentation
public class SpysInfiniteConsumables : Mod {
    public static SpysInfiniteConsumables Instance { get; private set; }

    public const string LocalizationKeyBase = "Mods.SPIC";
    public static bool MagicStorageLoaded => ModLoader.TryGetMod("MagicStorage", out _);
    public override void Load() {
        Instance = this;

        ConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.GetCurrencies();
        InfinityManager.ClearCache();

        ConsumableTypes.Mixed.RegisterAsGlobal();
        ConsumableTypes.Ammo.Register();
        ConsumableTypes.Usable.Register();
        ConsumableTypes.Placeable.Register();
        ConsumableTypes.GrabBag.Register();
        ConsumableTypes.Material.Register();
        ConsumableTypes.Currency.Register();
        ConsumableTypes.JourneySacrifice.Register();

        Configs.Presets.Defaults.Register();
        Configs.Presets.AllDisabled.Register();
        Configs.Presets.AllEnabled.Register();
        Configs.Presets.OneForAll.Register();
        Configs.Presets.JourneyCosts.Register();
    }

    public override void Unload() {
        ConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
        
        Instance = null;
    }

    // TODO Call for RegisterInfinity, ShouldConsumeItem (and more ?)
}


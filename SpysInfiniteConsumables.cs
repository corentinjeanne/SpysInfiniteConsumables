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

        VanillaConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.GetCurrencies();
        InfinityManager.ClearCache();

        VanillaConsumableTypes.Mixed.RegisterAsGlobal();
        VanillaConsumableTypes.Ammo.Register();
        VanillaConsumableTypes.Usable.Register();
        VanillaConsumableTypes.Placeable.Register();
        VanillaConsumableTypes.GrabBag.Register();
        VanillaConsumableTypes.Material.Register();
        VanillaConsumableTypes.Currency.Register();
        VanillaConsumableTypes.JourneySacrifice.Register();

        Configs.Presets.Defaults.Register();
        Configs.Presets.AllDisabled.Register();
        Configs.Presets.AllEnabled.Register();
        Configs.Presets.OneForAll.Register();
        Configs.Presets.JourneyCosts.Register();
    }

    public override void Unload() {
        VanillaConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
        
        Instance = null;
    }

    // TODO Call for RegisterInfinity, ShouldConsumeItem (and more ?)
}


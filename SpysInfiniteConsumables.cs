using Terraria.ModLoader;


namespace SPIC;

// TODO documentation
public class SpysInfiniteConsumables : Mod {

#nullable disable
    public static SpysInfiniteConsumables Instance { get; private set; }
#nullable restore

    public const string LocalizationKeyBase = "Mods.SPIC";
    public static bool MagicStorageLoaded => ModLoader.TryGetMod("MagicStorage", out _);
    public override void Load() {
        Instance = this;

        VanillaConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.GetCurrencies();
        InfinityManager.ClearCache();

        VanillaConsumableTypes.Ammo.Register();
        VanillaConsumableTypes.Usable.Register();
        VanillaConsumableTypes.Placeable.Register();
        VanillaConsumableTypes.GrabBag.Register();
        VanillaConsumableTypes.Material.Register();
        VanillaConsumableTypes.JourneySacrifice.Register();

        VanillaConsumableTypes.Currency.RegisterAsGlobal();
        VanillaConsumableTypes.Mixed.RegisterAsGlobal();
        
        Config.Presets.Defaults.Register();
        Config.Presets.AllDisabled.Register();
        Config.Presets.AllEnabled.Register();
        Config.Presets.OneForAll.Register();
        Config.Presets.JourneyCosts.Register();
    }

    public override void Unload() {
        VanillaConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
        
        Instance = null;
    }

    // TODO Call for RegisterInfinity, ShouldConsumeItem (and more ?)
}


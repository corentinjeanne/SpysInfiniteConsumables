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

        VanillaGroups.Placeable.ClearWandAmmos();
        InfinityManager.ClearCache();

        VanillaGroups.Ammo.Register();
        VanillaGroups.Usable.Register();
        VanillaGroups.Placeable.Register();
        VanillaGroups.GrabBag.Register();
        VanillaGroups.Material.Register();
        VanillaGroups.JourneySacrifice.Register();

        VanillaGroups.Currency.RegisterAsGlobal();
        VanillaGroups.Mixed.RegisterAsGlobal();
        
        Config.Presets.Defaults.Register();
        Config.Presets.AllDisabled.Register();
        Config.Presets.AllEnabled.Register();
        Config.Presets.OneForAll.Register();
        Config.Presets.JourneyCosts.Register();
    }

    public override void PostSetupContent() {
        CurrencyHelper.GetCurrencies();
    }

    public override void Unload() {
        VanillaGroups.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
        
        Instance = null;
    }

    // TODO Call for RegisterInfinity, ShouldConsumeItem (and more ?)
}


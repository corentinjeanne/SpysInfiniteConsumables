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

        // Infinities.Consumables.Register();
        // Infinities.Placeables.Register();
        // Infinities.GrabBags.Register();
        // Infinities.Materials.Register();
        // Infinities.Currencies.Register();
        // Infinities.JourneyResearch.Register();

        ConsumableTypes.Mixed.RegisterAsGlobal();
        ConsumableTypes.Ammo.Register();
        ConsumableTypes.Usable.Register();
        ConsumableTypes.Placeable.Register();
        ConsumableTypes.GrabBag.Register();
        ConsumableTypes.Material.Register();
        ConsumableTypes.Currency.Register();
        ConsumableTypes.JourneySacrifice.Register();
    }

    public override void Unload() {
        ConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
        
        Instance = null;
    }

    // TODO Call for RegisterInfinity, ShouldConsumeItem (and more ?)
    public override object Call(params object[] args) {
        return base.Call(args);
    }

}


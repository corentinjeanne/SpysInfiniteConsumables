using Terraria.ModLoader;

namespace SPIC;

public class SpysInfiniteConsumables : Mod {
    public static SpysInfiniteConsumables Instance { get; private set; }

    public const string LocalizationKeyBase = "Mods.SPIC";
    public static bool MagicStorageLoaded => ModLoader.TryGetMod("MagicStorage", out _);
    public override void Load() {
        Instance = this;

        ConsumableTypes.Placeable.ClearWandAmmos();
        CurrencyHelper.GetCurrencies();
        InfinityManager.ClearCache();

        Infinities.Consumables.Register();
        Infinities.Placeables.Register();
        Infinities.GrabBags.Register();
        Infinities.Materials.Register();
        Infinities.Currencies.Register();
        Infinities.JourneyResearch.Register();

        ConsumableTypes.Ammo.Register(Infinities.Consumables.ID);
        ConsumableTypes.Usable.Register(Infinities.Consumables.ID);
        ConsumableTypes.Placeable.Register(Infinities.Placeables.ID);
        ConsumableTypes.GrabBag.Register(Infinities.GrabBags.ID);
        ConsumableTypes.Material.Register(Infinities.Materials.ID);
        ConsumableTypes.Currency.Register(Infinities.Currencies.ID);
        ConsumableTypes.JourneySacrifice.Register(Infinities.JourneyResearch.ID);

        Configs.Requirements.Instance.SaveConfig(); // >>> TODO move in a better place
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


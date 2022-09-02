using Terraria.ModLoader;

namespace SPIC;

public class SpysInfiniteConsumables : Mod {
    public static SpysInfiniteConsumables Instance { get; private set; }

    public const string LocalizationKeyBase = "Mods.SPIC";
    public static bool MagicStorageLoaded => ModLoader.TryGetMod("MagicStorage", out _);
    public override void Load() {
        Instance = this;

        Infinities.Placeable.ClearWandAmmos();
        CurrencyHelper.GetCurrencies();
        InfinityManager.ClearCache();

        Infinities.Ammo.Register();
        Infinities.Usable.Register();
        Infinities.Currency.Register();
        Infinities.GrabBag.Register();
        Infinities.Material.Register();
        Infinities.Placeable.Register();
    }

    public override void Unload() {
        Infinities.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
        
        Instance = null;
    }

    // TODO Call for RegisterInfinity, ShouldConsumeItem (and more ?)
    public override object Call(params object[] args) {
        return base.Call(args);
    }
}


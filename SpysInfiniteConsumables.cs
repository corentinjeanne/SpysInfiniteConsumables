using Terraria.ModLoader;


namespace SPIC;

public class SpysInfiniteConsumables : Mod {

#nullable disable
    public static SpysInfiniteConsumables Instance { get; private set; }
#nullable restore

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

    public override object Call(params object[] args) {
        try {
            if (args[0].ToString() == "HasInfinite") {
                int playerID = (int)args[1];
                dynamic consumable = args[2];
                int consumed = (int)args[3];
                string fullName = (string)args[4];

                return InfinityManager.HasInfinite(Terraria.Main.player[playerID], consumable, consumed, InfinityManager.ConsumableGroup(fullName)!);
            }
        }catch(System.InvalidCastException cast){
            Logger.Error("The type of one of the arguments vas incorect", cast);
        }catch(System.Exception error){
            Logger.Error("The call failled", error);
        }
        return base.Call();

    }
}


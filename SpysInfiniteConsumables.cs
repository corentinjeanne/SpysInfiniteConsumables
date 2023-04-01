using Terraria.ModLoader;


namespace SPIC;

public class SpysInfiniteConsumables : Mod {

    public static SpysInfiniteConsumables Instance => s_instance.TryGetTarget(out SpysInfiniteConsumables? instance) ? instance : null!;
    
    public override void Load() {
        s_instance.SetTarget(this);

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
        
        Configs.Presets.Defaults.Register();
        Configs.Presets.AllDisabled.Register();
        Configs.Presets.AllEnabled.Register();
        Configs.Presets.OneForAll.Register();
        Configs.Presets.JourneyCosts.Register();
    }

    public override void PostSetupContent() {
        CurrencyHelper.GetCurrencies();
        Configs.CategoryDetection.Instance.LoadConfig();
        Configs.InfinityDisplay.Instance.LoadConfig();
    }

    public override void Unload() {
        VanillaGroups.Placeable.ClearWandAmmos();
        CurrencyHelper.ClearCurrencies();
        InfinityManager.ClearCache();
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
            Logger.Error("The type of one of the arguments was incorect", cast);
        }catch(System.Exception error){
            Logger.Error("The call failled", error);
        }
        return base.Call();
    }

    private static readonly System.WeakReference<SpysInfiniteConsumables> s_instance = new(null!);
}


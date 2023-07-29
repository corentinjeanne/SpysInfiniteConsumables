using Terraria.ModLoader;

namespace SPIC;

// ItemID.Sets.ShimmerTransformToItem; // ? shimmer + chloro extra Infinity (conversions) 

public sealed class SpysInfiniteConsumables : Mod {

    public static SpysInfiniteConsumables Instance { get; private set; } = null!;

    public override void Load() {
        Instance = this;
    }

    public override void PostSetupContent() {
        CurrencyHelper.GetCurrencies();
    }

    public override void Unload() {
        CurrencyHelper.ClearCurrencies();
        
        InfinityManager.Unload();
        Configs.Presets.PresetLoader.Unload();
        Instance = null!;
    }

    public override object Call(params object[] args) {
        try {
            if (args[0].ToString() == "HasInfinite") {
                int playerID = (int)args[1];
                dynamic consumable = args[2];
                int consumed = (int)args[3];
                string[] parts = ((string)args[4]).Split('/', 2);
                string mod = parts[0];
                string name = parts[0];

                return InfinityManager.HasInfinite(Terraria.Main.player[playerID], consumable, consumed, InfinityManager.GetInfinity(mod, name)!);
            }
        }catch(System.InvalidCastException cast){
            Logger.Error("The type of one of the arguments was incorect", cast);
        }catch(System.Exception error){
            Logger.Error("The call failled", error);
        }
        return base.Call();
    }

    public readonly static string[] Versions = new[] { "2.0.0", "2.1.0", "2.2.0", "2.2.0.1", "2.2.1", "3.0" };
}


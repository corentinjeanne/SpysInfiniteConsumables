using System.Text.RegularExpressions;
using SpikysLib.Extensions;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public sealed class SpysInfiniteConsumables : Mod {

    public static SpysInfiniteConsumables Instance { get; private set; } = null!;

    public override void Load() => Instance = this;

    public override void PostSetupContent() {
        FullInfinity.RegisterExtraLocalization<System.Enum>((infinity, category) => infinity.GetLocalization(category.ToString(), () => Regex.Replace(category.ToString(), "([A-Z])", " $1").Trim()).Value);
        FullInfinity.RegisterExtraLocalization<LocalizedText>((infinity, text) => text.Value);
        FullInfinity.RegisterExtraLocalization<string>((infinity, str) => Language.GetOrRegister(str, () => Regex.Replace(str, "([A-Z])", " $1").Trim()).Value);
        
        Configs.Presets.PresetLoader.SetupPresets();
        if(Configs.InfinityDisplay.Instance.version.Length != 0) {
            Configs.InfinityDisplay.Instance.PortConfig();
            Configs.InfinitySettings.Instance.PortConfig();
        } else {
            Configs.InfinityDisplay.Instance.Load();
            Configs.InfinitySettings.Instance.Load();
        }
    }

    public override void Unload() {
        FullInfinity.ClearExtraLocs();
        
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

                return InfinityManager.HasInfinite(Terraria.Main.player[playerID], consumable, consumed, (dynamic)InfinityManager.GetInfinity(mod, name)!);
            }
        }catch(System.InvalidCastException cast){
            Logger.Error("The type of one of the arguments was incorect", cast);
        }catch(System.Exception error){
            Logger.Error("The call failled", error);
        }
        return base.Call();
    }
}


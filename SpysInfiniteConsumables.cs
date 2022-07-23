using Terraria.ModLoader;

namespace SPIC {

    public class SpysInfiniteConsumables : Mod {
        public static SpysInfiniteConsumables Instance { get; private set; }

        public static bool MagicStorageLoaded => ModLoader.TryGetMod("MagicStorage", out _);
        public override void Load() {
            Instance = this;
            PlaceableExtension.ClearWandAmmos();
            CurrencyExtension.GetCurrencies();
        }

        public override void Unload() {
            PlaceableExtension.ClearWandAmmos();
            CurrencyExtension.ClearCurrencies();
            Instance = null;
        }
    }
}

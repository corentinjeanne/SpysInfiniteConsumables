using Terraria.ModLoader;

namespace SPIC {

    public class SpysInfiniteConsumables : Mod {
        public static SpysInfiniteConsumables Instance { get; private set; }
        
        public override void Load() {
            Instance = this;
            CurrencyExtension.GetCurrencies();
        }
        public override void Unload() {
            Instance = null;
            CurrencyExtension.ClearCurrencies();

        }

    }
}

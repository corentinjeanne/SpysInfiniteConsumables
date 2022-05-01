using Terraria.ModLoader;
 
namespace SPIC {

    public class SpysInfiniteConsumables : Mod {
        public static SpysInfiniteConsumables Instance { get; private set; }
        public bool ContentSetup { get; private set; }
        public override void Load() {
            Instance = this;
        }
        public override void PostSetupContent() {
            ContentSetup = true;
        }
        public override void Unload() {
            Instance = null;
        }
    }
}

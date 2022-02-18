using Terraria.ModLoader;


namespace SPIC {

	public class SpysInfiniteConsumables : Mod {
		public static ModKeybind ShowConsumableCategory;
		public override void Load() {
            ShowConsumableCategory = KeybindLoader.RegisterKeybind(this, "Favorited Quick buff", "N");
        }

    }
}

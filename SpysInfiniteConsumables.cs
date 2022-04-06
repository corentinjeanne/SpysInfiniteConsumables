using Terraria.ModLoader;
using Terraria.Localization;

namespace SPIC {

	public class SpysInfiniteConsumables : Mod {
		public static ModKeybind ShowConsumableCategory;
		public override void Load() {
			//string s = Language.GetTextValue("Mods.SPIC.Hotkeys.Categories");
			ShowConsumableCategory = KeybindLoader.RegisterKeybind(this, "Hold to display the categories of items", "N");
		}
		public override void Unload() {
			ShowConsumableCategory = null;
		}
	}
}

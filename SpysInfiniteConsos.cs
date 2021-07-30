using Terraria.ModLoader;

namespace SPIC {

	public class SpysInfiniteConsos : Mod {

		public override void Load(){
            Utilities.mod = this;
		}
		
		public override void Unload(){
			Utilities.mod = null;
		}
	}
}

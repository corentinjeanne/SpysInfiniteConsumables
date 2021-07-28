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

/*
TODO:
# custom values
# ingame command & show category in game?
# list cleaning
# max stack increase
# right category for furnitures
# liquids
- block/ammo? for placeble ammos
- redo liquids & wiring
- maybe redo dupping for critters & furnitures ?
*/
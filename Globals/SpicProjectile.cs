using Terraria;
using Terraria.ModLoader;

namespace SPIC.Globals {

	public class SpicProjectile : GlobalProjectile {

        public override void Load() {
			On.Terraria.Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks += HookKill_DirtAndFluid;
			On.Terraria.Projectile.ExplodeTiles += HookExplodeTiles;
			On.Terraria.Projectile.ExplodeCrackedTiles += HookExplodeCrackedTiles;
		}

		private static void SaveExplosive(Projectile p){
            if (p.owner < 0) return;

            SpicPlayer spicPlayer = Main.player[p.owner].GetModPlayer<SpicPlayer>();
            spicPlayer.FindPotentialExplosives(p.type);
        }
		private void HookExplodeCrackedTiles(On.Terraria.Projectile.orig_ExplodeCrackedTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ){
            orig(self, compareSpot, radius, minI, maxI, minJ, maxJ);
            SaveExplosive(self);
        }
		private void HookExplodeTiles(On.Terraria.Projectile.orig_ExplodeTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode){
			orig(self, compareSpot, radius, minI, maxI, minJ, maxJ, wallSplode);
            SaveExplosive(self);
		}

		private void HookKill_DirtAndFluid(On.Terraria.Projectile.orig_Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks orig, Terraria.Projectile self, Microsoft.Xna.Framework.Point pt, float size, Terraria.Utils.TileActionAttempt plot) {
            orig(self, pt, size, plot);
            SaveExplosive(self);
        }
	}

}
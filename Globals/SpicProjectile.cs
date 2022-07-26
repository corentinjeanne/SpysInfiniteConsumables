using Terraria;
using Terraria.ModLoader;

namespace SPIC.Globals {

	public class SpicProjectile : GlobalProjectile {

        public override void Load() {
			On.Terraria.Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks += HookKill_DirtAndFluid;
			On.Terraria.Projectile.ExplodeTiles += HookExplodeTiles;
			On.Terraria.Projectile.ExplodeCrackedTiles += HookExplodeCrackedTiles;
            ClearExploded();
        }
        public override void Unload() {
            ClearExploded();
        }

        internal static void ClearExploded() => _explodedProjTypes.Clear();
        private static readonly System.Collections.Generic.HashSet<int> _explodedProjTypes = new();

        private static void Explode(Projectile proj){

            if (proj.owner < 0 || _explodedProjTypes.Contains(proj.type) || !Configs.CategoryDetection.Instance.DetectMissing) return;
            
            DetectionPlayer detectionPlayer = Main.player[proj.owner].GetModPlayer<DetectionPlayer>();
            int type = detectionPlayer.FindPotentialExplosivesType(proj.type);
            Item item = System.Array.Find(detectionPlayer.Player.inventory, i => i.type == type) ?? new(type);

            // TODO move in DetectionPlayer
            if (!item.IsAir && Configs.CategoryDetection.Instance.DetectedExplosive(item)) {
                CategoryManager.UpdateType(item);
                Main.player[proj.owner].GetModPlayer<InfinityPlayer>().UpdateTypeInfinities(item);
                detectionPlayer.RefilExplosive(proj.type, item);
            }
            _explodedProjTypes.Add(proj.type);

        }
		private void HookExplodeCrackedTiles(On.Terraria.Projectile.orig_ExplodeCrackedTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ){
            orig(self, compareSpot, radius, minI, maxI, minJ, maxJ);
            Explode(self);
        }
		private void HookExplodeTiles(On.Terraria.Projectile.orig_ExplodeTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode){
			orig(self, compareSpot, radius, minI, maxI, minJ, maxJ, wallSplode);
            Explode(self);
		}

		private void HookKill_DirtAndFluid(On.Terraria.Projectile.orig_Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks orig, Terraria.Projectile self, Microsoft.Xna.Framework.Point pt, float size, Terraria.Utils.TileActionAttempt plot) {
            orig(self, pt, size, plot);
            Explode(self);
        }
	}

}
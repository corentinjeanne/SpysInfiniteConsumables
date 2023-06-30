using Terraria;
using Terraria.ModLoader;
using SPIC.VanillaGroups;

namespace SPIC.Globals {

	public class ExplosionProjectile : GlobalProjectile {

        public override void Load() {
			On_Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks += HookKill_DirtAndFluid;
			On_Projectile.ExplodeTiles += HookExplodeTiles;
			On_Projectile.ExplodeCrackedTiles += HookExplodeCrackedTiles;
            ClearExploded();
        }
        public override void Unload() => ClearExploded();


        private static void Explode(Projectile proj){
            if (proj.owner < 0 || _explodedProjTypes.Contains(proj.type) || !Configs.CategoryDetection.Instance.DetectMissing) return;
            _explodedProjTypes.Add(proj.type);
            
            DetectionPlayer detectionPlayer = Main.player[proj.owner].GetModPlayer<DetectionPlayer>();
            int type = detectionPlayer.FindPotentialExplosivesType(proj.type);
            Item item = System.Array.Find(detectionPlayer.Player.inventory, i => i.type == type) ?? new(type);

            AmmoCategory ammo = item.GetCategory(Ammo.Instance);
            UsableCategory usable = item.GetCategory(Usable.Instance);
            if(ammo != AmmoCategory.None && ammo != AmmoCategory.Explosive){
                if(Configs.CategoryDetection.Instance.SaveDetectedCategory(item, AmmoCategory.Explosive, Ammo.Instance))
                    detectionPlayer.RefilExplosive(proj.type, item);
            }
            else if(usable != UsableCategory.None && usable != UsableCategory.Explosive){
                if(Configs.CategoryDetection.Instance.SaveDetectedCategory(item, UsableCategory.Explosive, Usable.Instance))
                    detectionPlayer.RefilExplosive(proj.type, item);
            }
        }

		private void HookExplodeCrackedTiles(On_Projectile.orig_ExplodeCrackedTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ){
            orig(self, compareSpot, radius, minI, maxI, minJ, maxJ);
            Explode(self);
        }
		private void HookExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode){
			orig(self, compareSpot, radius, minI, maxI, minJ, maxJ, wallSplode);
            Explode(self);
		}
		private void HookKill_DirtAndFluid(On_Projectile.orig_Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks orig, Projectile self, Microsoft.Xna.Framework.Point pt, float size, Utils.TileActionAttempt plot) {
            orig(self, pt, size, plot);
            Explode(self);
        }


        public static void ClearExploded() => _explodedProjTypes.Clear();
        private static readonly System.Collections.Generic.HashSet<int> _explodedProjTypes = new();

    }

}
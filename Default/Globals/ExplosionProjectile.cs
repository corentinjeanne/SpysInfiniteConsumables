using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using Terraria.DataStructures;

namespace SPIC.Default.Globals {

    public sealed class ExplosionProjectile : GlobalProjectile {

        public override void Load() {
            On_Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks += HookKill_DirtAndFluid;
            On_Projectile.ExplodeTiles += HookExplodeTiles;
            On_Projectile.ExplodeCrackedTiles += HookExplodeCrackedTiles;
        }
        public override void Unload() => ClearExploded();

        public override void OnSpawn(Projectile projectile, IEntitySource source) {
            if (projectile.noDropItem || source is not EntitySource_ItemUse_WithAmmo spawn) return;
            if (!spawn.Player.PickAmmo(spawn.Player.HeldItem, out int proj, out _, out _, out _, out int ammoType, true) || proj != projectile.type) ammoType = DetectionPlayer.FindAmmoType(spawn.Player, projectile.type);
            if (spawn.Player.HasInfinite(ammoType, 1, Ammo.Instance)) projectile.noDropItem = true;
        }

        private static void Explode(Projectile proj) {
            if (proj.owner < 0 || _explodedProjTypes.Contains(proj.type) || !Configs.InfinitySettings.Instance.DetectMissingCategories) return;
            _explodedProjTypes.Add(proj.type);

            int type = DetectionPlayer.FindAmmoType(Main.player[proj.owner], proj.type);

            AmmoCategory ammo = InfinityManager.GetCategory(type, Ammo.Instance);
            // UsableCategory usable = InfinityManager.GetCategory(type, Usable.Instance);
            if (ammo != AmmoCategory.None && ammo != AmmoCategory.Special) {
                if (InfinityManager.SaveDetectedCategory(new(type), AmmoCategory.Special, Ammo.Instance)) DetectionPlayer.RefilExplosive(Main.player[proj.owner], proj.type, type);
            }
            // else if(usable != UsableCategory.None && usable != UsableCategory.Tool){
            //     if(InfinityManager.SaveDetectedCategory(new(type), UsableCategory.Tool, Usable.Instance)) detectionPlayer.RefilExplosive(proj.type, type);
            // }
        }

        private void HookExplodeCrackedTiles(On_Projectile.orig_ExplodeCrackedTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ) {
            orig(self, compareSpot, radius, minI, maxI, minJ, maxJ);
            Explode(self);
        }
        private void HookExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Microsoft.Xna.Framework.Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode) {
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
using Terraria;
using Terraria.ModLoader;
using SPIC.Default.Infinities;
using Terraria.DataStructures;
using Terraria.ID;

namespace SPIC.Default.Globals {

    public sealed class ExplosionProjectile : GlobalProjectile {

        public override bool InstancePerEntity => true;

        public bool infiniteFallingTile;

        public override void Load() {
            On_Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks += HookKill_DirtAndFluid;
            On_Projectile.ExplodeTiles += HookExplodeTiles;
            On_Projectile.ExplodeCrackedTiles += HookExplodeCrackedTiles;
        }

        public override void Unload() => ClearExploded();

        public override bool PreAI(Projectile projectile) {
            InfiniteWorld.Instance.contextProjectile = projectile;
            return true;
        }
        public override void PostAI(Projectile projectile) => InfiniteWorld.Instance.contextProjectile = null;

        public override bool PreKill(Projectile projectile, int timeLeft) {
            InfiniteWorld.Instance.contextProjectile = projectile;
            return true;
        }
        public override void OnKill(Projectile projectile, int timeLeft) => InfiniteWorld.Instance.contextProjectile = null;

        public override void OnSpawn(Projectile projectile, IEntitySource source) {
            if (source is IEntitySource_WithStatsFromItem spawn && IsInfiniteSource(spawn)
            || source is EntitySource_TileBreak tileBreak && InfiniteWorld.Instance.IsInfinite(tileBreak.TileCoords.X, tileBreak.TileCoords.Y, TileFlags.Block)) {
                if (projectile.aiStyle == ProjAIStyleID.FallingTile) infiniteFallingTile = true;
                else projectile.noDropItem = true;
            }
        }

        private static bool IsInfiniteSource(IEntitySource_WithStatsFromItem spawn) {
            if (spawn.Player.HasInfinite(spawn.Item, 1, Usable.Instance)) return true;
            if (spawn.Player.HasInfinite(Placeable.GetAmmo(spawn.Player, spawn.Item) ?? spawn.Item, 1, Placeable.Instance)) return true;
            if (spawn.Player.HasInfinite(spawn.Player.ChooseAmmo(spawn.Item) ?? spawn.Item, 1, Ammo.Instance)) return true;
            if (spawn.Item.type == ItemID.DirtRod && spawn.Player.GetModPlayer<DetectionPlayer>().aimedAtInfiniteTile) return true;
            return false;
        }

        private static void Explode(Projectile proj) {
            if (proj.owner < 0 || !Configs.InfinitySettings.Instance.detectMissingCategories || !_explodedProjTypes.Add(proj.type)) return;

            Item item = DetectionPlayer.FindAmmo(Main.player[proj.owner], proj.type);

            AmmoCategory ammo = InfinityManager.GetCategory(item, Ammo.Instance);
            if (ammo != AmmoCategory.None && ammo != AmmoCategory.Special) {
                if (Ammo.Instance.SaveDetectedCategory(item, AmmoCategory.Special)) DetectionPlayer.RefillExplosive(Main.player[proj.owner], proj.type, item);
            }
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
        private static readonly System.Collections.Generic.HashSet<int> _explodedProjTypes = [];
    }

}
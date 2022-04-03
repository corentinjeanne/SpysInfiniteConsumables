//using Terraria.ModLoader;

//namespace SPIC.Globals {

//	public class NoProjectileDup : GlobalProjectile {
//		public override bool InstancePerEntity => true;
//        public override bool CloneNewInstances => true;

//        public static KeyValuePair<Player,  bool> lastSpawnedProjectiles = new ();

//        private KeyValuePair<Player,  bool> potentialOwner = new ();
//        public override void SetDefaults(Projectile projectile){
//            potentialOwner = lastSpawnedProjectiles; // Need to be done because projectile.owner is not set
//        }

//        public override bool PreKill(Projectile projectile, int timeLeft){
//            if(projectile.noDropItem || !Config.ConsumableConfig.Instance.PreventItemDupication || potentialOwner.Key == null)
//                return true;
//            if(Main.player[projectile.owner] == potentialOwner.Key)
//                projectile.noDropItem = potentialOwner.Value;

//            return true;
//        }
//	}

//}
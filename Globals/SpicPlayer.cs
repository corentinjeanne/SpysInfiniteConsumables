using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using SPIC.Systems;
using SPIC.Categories;


namespace SPIC.Globals {
	public class SpicPlayer : ModPlayer {

		public int preUseMaxLife, preUseMaxMana;
		public int preUseExtraAccessories;
		public bool preUseDemonHeart;


		public override void Load() {
			On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
		}
		public override void Unload() {
			On.Terraria.Player.PutItemInInventoryFromItemUsage -= HookPutItemInInventory;
		}

		public void PreUseItem() {
			preUseMaxLife = Player.statLifeMax2;
			preUseMaxMana = Player.statManaMax2;
			preUseExtraAccessories = Player.extraAccessorySlots;
			preUseDemonHeart = Player.extraAccessory;
		}

		private void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
			if(selItem >= 0 && self.inventory[selItem].GetConsumableCategory() == Consumable.Category.Bucket) {
				self.inventory[selItem].stack++;
				if (self.HasInfinite(self.inventory[selItem].type, Consumable.Category.Bucket)) {
					if(ModContent.GetInstance<Config.ConsumableConfig>().PreventItemDupication) return;
				}else {
					self.inventory[selItem].stack--;
				}
			}
			orig(self, type, selItem);
		}
	}
}
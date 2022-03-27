using Terraria;
using Terraria.ModLoader;

using SPIC.Categories;
using SPIC.Systems;
using System;

namespace SPIC.Globals {
	public class SpicPlayer : ModPlayer {

		public int preUseMaxLife, preUseMaxMana;
		public int preUseExtraAccessories;
		public Microsoft.Xna.Framework.Vector2 preUsePosition;
		public bool preUseDemonHeart;
		private int m_CheckingForCategory;
		public bool CheckingForCategory => m_CheckingForCategory != Terraria.ID.ItemID.None;
		public bool InItemCheck { get; private set; }


		public override void Load() {
			On.Terraria.Player.PutItemInInventoryFromItemUsage += HookPutItemInInventory;
		}

		public override bool PreItemCheck() {
			InItemCheck = true;

			if (CheckingForCategory) SavePreUseItemStats();

			return true;
		}
		public override void PostItemCheck() {
			InItemCheck = false;
			if (CheckingForCategory) {
				Consumable.Category? cat = CheckForCategory();
				if (cat.HasValue || Player.itemTime <= 1) StopDetectingCategory();
			}
		}

		public void StartDetectingCategory(int type) {
			m_CheckingForCategory = type;
			SavePreUseItemStats();
		}
		public void StopDetectingCategory(Consumable.Category? detectedCategory = null) {

			Consumable.AddToCache(m_CheckingForCategory, detectedCategory ?? CheckForCategory() ?? Consumable.Category.PlayerBooster);
			m_CheckingForCategory = Terraria.ID.ItemID.None;
		}

		private void SavePreUseItemStats() {
			preUseMaxLife = Player.statLifeMax2;
			preUseMaxMana = Player.statManaMax2;
			preUseExtraAccessories = Player.extraAccessorySlots;
			preUseDemonHeart = Player.extraAccessory;
			preUsePosition = Player.position;

			SpicWorld.SavePreUseItemStats();
		}
		public Consumable.Category? CheckForCategory() {

			NPCStats stats = Utility.GetNPCStats();
			if (SpicWorld.preUseNPCStats.boss != stats.boss || SpicWorld.preUseInvasion != Main.invasionType)
				return Consumable.Category.Summoner;

			if (SpicWorld.preUseNPCStats.total != stats.total)
				return Consumable.Category.Critter;

			// Player Boosters
			if (preUseMaxLife != Player.statLifeMax2 || preUseMaxMana != Player.statManaMax2
					|| preUseExtraAccessories != Player.extraAccessorySlots || preUseDemonHeart != Player.extraAccessory)
				return Consumable.Category.PlayerBooster;

			// World boosters
			if (SpicWorld.preUseDifficulty != Utility.WorldDifficulty())
				return Consumable.Category.WorldBooster;

			// Some tools
			if (Player.position != preUsePosition)
				return Consumable.Category.Tool;

			// No new category detected
			return null;
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
using System;

using Terraria;
using Terraria.ModLoader;

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
				Categories.Consumable? cat = CheckForCategory();
				if (cat.HasValue || Player.itemTime <= 1) StopDetectingCategory();
			}
		}

		public void StartDetectingCategory(int type) {
			m_CheckingForCategory = type;
			SavePreUseItemStats();
		}
		public void StopDetectingCategory(Categories.Consumable? detectedCategory = null) {

			ConsumableExtension.AddToCache(m_CheckingForCategory, detectedCategory ?? CheckForCategory() ?? Categories.Consumable.PlayerBooster);
			m_CheckingForCategory = Terraria.ID.ItemID.None;
		}

		private void SavePreUseItemStats() {
			preUseMaxLife = Player.statLifeMax2;
			preUseMaxMana = Player.statManaMax2;
			preUseExtraAccessories = Player.extraAccessorySlots;
			preUseDemonHeart = Player.extraAccessory;
			preUsePosition = Player.position;

			Systems.SpicWorld.SavePreUseItemStats();
		}
		public Categories.Consumable? CheckForCategory() {

			NPCStats stats = Utility.GetNPCStats();
			if (Systems.SpicWorld.preUseNPCStats.boss != stats.boss || Systems.SpicWorld.preUseInvasion != Main.invasionType)
				return Categories.Consumable.Summoner;

			if (Systems.SpicWorld.preUseNPCStats.total != stats.total)
				return Categories.Consumable.Critter;

			// Player Boosters
			if (preUseMaxLife != Player.statLifeMax2 || preUseMaxMana != Player.statManaMax2
					|| preUseExtraAccessories != Player.extraAccessorySlots || preUseDemonHeart != Player.extraAccessory)
				return Categories.Consumable.PlayerBooster;

			// World boosters
			if (Systems.SpicWorld.preUseDifficulty != Utility.WorldDifficulty())
				return Categories.Consumable.WorldBooster;

			// Some tools
			if (Player.position != preUsePosition)
				return Categories.Consumable.Tool;

			// No new category detected
			return null;
		}
		private void HookPutItemInInventory(On.Terraria.Player.orig_PutItemInInventoryFromItemUsage orig, Player self, int type, int selItem) {
			if (selItem > -1) {
				if (!ConsumableExtension.IsInCache(self.inventory[selItem].type)) ConsumableExtension.AddToCache(self.inventory[selItem].type, Categories.Consumable.Bucket);
				
				self.inventory[selItem].stack++;
				if (ModContent.GetInstance<Configs.ConsumableConfig>().PreventItemDupication && self.HasInfinite(self.inventory[selItem].type, Categories.Consumable.Bucket)) {
					return;
				}
				self.inventory[selItem].stack--;
			}
			orig(self, type, selItem);
		}

	}
}
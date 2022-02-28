using System;
using System.Collections.Generic;


using System.Reflection;
using MonoMod.Cil;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using SPIC.Categories;
using SPIC.Systems;



namespace SPIC.Globals {
	
	public class SpicItem : GlobalItem {

		private static int[] m_ItemMaxStack;
		public static int MaxStack(int type) => SetDefaultsHook ? m_ItemMaxStack[type] : new Item(type).maxStack;
		public static bool SetDefaultsHook { get; private set; }

		public override void Load() {
			m_ItemMaxStack = new int[ItemID.Count];
			IL.Terraria.Item.SetDefaults_int_bool += HookItemSetDefaults;

		}
		public override void Unload() {
			IL.Terraria.Item.SetDefaults_int_bool -= HookItemSetDefaults;
			SetDefaultsHook = false;
		}

		public override void SetDefaults(Item item) {
			if(item.tileWand != -1){
				if(!WandAmmo.wandAmmoTypes.Contains(item.tileWand)) WandAmmo.wandAmmoTypes.Add(item.tileWand);
			}
		}
		public override void SetStaticDefaults() {
			if (!SetDefaultsHook) return;

			Array.Resize(ref m_ItemMaxStack, ItemLoader.ItemCount);
			for (int type = ItemID.Count; type < ItemLoader.ItemCount; type++) {
				ModItem item = ItemLoader.GetItem(type);
				ModItem modItem = item.Clone(new Item());
				modItem.SetDefaults();
				m_ItemMaxStack[type] = modItem.Item.maxStack != 0 ? modItem.Item.maxStack : 1;
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (SpysInfiniteConsumables.ShowConsumableCategory.Current) {
				Config.ConsumableConfig config = ModContent.GetInstance<Config.ConsumableConfig>();
				if(config.HasCustomInfinity(item.type, out int req)) {
					tooltips.Add(new TooltipLine(Mod, "Category", $"Custom: {config.InfinityRequirement(req, item.type)} items"));
					return;
				}
				string category = "";
				Consumable.Category? consumable = item.GetConsumableCategory();
				if (consumable.HasValue) {
					if (consumable.Value != Consumable.Category.NotConsumable) {
						category += (Consumable.IsTileCategory(consumable.Value) ?"P" : "C") + $":{consumable} ";
					}
				}else category += $"?Consumable ";

				Ammo.Category ammo = item.GetAmmoCategory();
				if (ammo != Ammo.Category.NotAmmo) category += $"A:{ammo} ";

				GrabBag.Category? bag = item.GetBagCategory();
				if (bag.HasValue) {
					if (bag.Value != GrabBag.Category.NotaBag) category += $"B:{bag} ";
				}
				WandAmmo.Category? wand = item.GetWandAmmoCategory();
				if (wand.HasValue) {
					if (wand.Value != WandAmmo.Category.NotWandAmmo) category += $"W:{wand} ";
				}
				tooltips.Add(new TooltipLine(Mod, "Category", category));
			}
		}

		public override bool? UseItem(Item item, Player player) {
			Consumable.Category? category = item.GetConsumableCategory();
			if (!category.HasValue) {
				player.GetModPlayer<SpicPlayer>().PreUseItem();
			   SpicWorld.PreUseItem();
			}
			return null;
		}
		public override bool ConsumeItem(Item item, Player player) {

			// Bags
			if (Main.playerInventory && player.HeldItem != item) {
				// Bags
				if (Main.mouseRight && Main.mouseRightRelease) {
					GrabBag.Category? bagCategory = item.GetBagCategory();
					return !player.HasInfinite(item.type, bagCategory ?? GrabBag.Category.GrabBag);
				}
				// Wands
				return player.HasInfinite(item.type, item.GetWandAmmoCategory() ?? WandAmmo.Category.WandAmmo);
			}

			// Consumables
			Consumable.Category? consumableCategory = item.GetConsumableCategory();
			if (consumableCategory.HasValue) return !player.HasInfinite(item.type, consumableCategory.Value);


			SpicPlayer modPlayer = player.GetModPlayer<SpicPlayer>();
			// Boss / event summoner
			if (SpicWorld.preUseBossCount != Utility.BossCount() || SpicWorld.preUseInvasion != Main.invasionType) {
				return !player.HasInfinite(item.type, Consumable.Category.Summoner);
			}
			// Player Boosters
			if (modPlayer.preUseMaxLife != player.statLifeMax2 || modPlayer.preUseMaxMana != player.statManaMax2
				|| modPlayer.preUseExtraAccessories != player.extraAccessorySlots || modPlayer.preUseDemonHeart != player.extraAccessory) {
				return !player.HasInfinite(item.type, Consumable.Category.PlayerBooster);
			}
			// World boosters
			if (SpicWorld.preUseDifficulty != Utility.WorldDifficulty || item.type == ItemID.LicenseBunny || item.type == ItemID.LicenseCat || item.type == ItemID.LicenseDog || item.type == ItemID.CombatBook) {
				return !player.HasInfinite(item.type, Consumable.Category.WorldBooster);
			}
			
			// Some boosters may go through
			return !player.HasInfinite(item.type, Consumable.Category.Tool);
			
		}
		public override bool CanBeConsumedAsAmmo(Item item, Player player) {
			return !player.HasInfiniteAmmo(item);
		}

		private void HookItemSetDefaults(ILContext il) {

			Type[] args = new Type[] { typeof(Item), typeof(bool) };
			MethodBase setdefault_item_bool = typeof(ItemLoader).GetMethod("SetDefaults", BindingFlags.Static | BindingFlags.NonPublic, args);

			// IL code editing
			ILCursor c = new ILCursor(il);

			if (setdefault_item_bool == null || !c.TryGotoNext(i => i.MatchCall(setdefault_item_bool))) {
				Mod.Logger.Error("Set default hook could not be aplied");
				return; // Patch unable to be applied
			}

			c.Index -= args.Length;
			c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0); // item
			c.EmitDelegate<Action<Item>>((Item item) => {
				if (item.type < ItemID.Count) {
					m_ItemMaxStack[item.type] = item.maxStack;
				}
			});
			SetDefaultsHook = true;
		}
	}
}
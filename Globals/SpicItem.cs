using System;
using System.Collections.Generic;

using System.Reflection;
using MonoMod.Cil;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using SPIC.Categories;

namespace SPIC.Globals {
	
	public class SpicItem : GlobalItem {

		private static int[] s_ItemMaxStack;
		public static int MaxStack(int type) => SetDefaultsHook ? s_ItemMaxStack[type] : new Item(type).maxStack;
		public static bool SetDefaultsHook { get; private set; }

		public override void Load() {
			s_ItemMaxStack = new int[ItemID.Count];
			IL.Terraria.Item.SetDefaults_int_bool += HookItemSetDefaults;

		}
		public override void Unload() {
			SetDefaultsHook = false;
			s_ItemMaxStack = null;
			Consumable.ClearCache();
		}

		public override void SetDefaults(Item item) {
			if(item.tileWand != -1){
				if(!WandAmmo.wandAmmoTypes.Contains(item.tileWand)) WandAmmo.wandAmmoTypes.Add(item.tileWand);
			}
			switch (item.type) {
			case ItemID.PlatinumAxe:
				item.useTime = 0;
				item.useAnimation = 0;
				break;
			}
		}
		public override void SetStaticDefaults() {
			if (!SetDefaultsHook) return;

			Array.Resize(ref s_ItemMaxStack, ItemLoader.ItemCount);
			for (int type = ItemID.Count; type < ItemLoader.ItemCount; type++) {
				ModItem item = ItemLoader.GetItem(type);
				ModItem modItem = item.Clone(new Item());
				modItem.SetDefaults();
				s_ItemMaxStack[type] = modItem.Item.maxStack != 0 ? modItem.Item.maxStack : 1;
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
				}else category += "?Consumable ";

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

			if (!category.HasValue) player.GetModPlayer<SpicPlayer>().StartDetectingCategory(item.type);

			return null;
		}
		public override bool ConsumeItem(Item item, Player player) {
			Config.ConsumableConfig config = ModContent.GetInstance<Config.ConsumableConfig>();
			SpicPlayer modPlayer = player.GetModPlayer<SpicPlayer>();

			if (modPlayer.InItemCheck) {
				// Wands
				if (item != player.HeldItem) {
					if (!config.InfiniteTiles) return true;
					return player.HasInfinite(item.type, item.GetWandAmmoCategory() ?? WandAmmo.Category.WandAmmo);
				}

				// Consumable used
				if(modPlayer.CheckingForCategory) modPlayer.StopDetectingCategory();
			}
			// Bags
			else if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease) {
				return !config.InfiniteConsumables || !player.HasInfinite(item.type, item.GetBagCategory() ?? GrabBag.Category.GrabBag);
			}
			Consumable.Category consumableCategory = item.GetConsumableCategory() ?? Consumable.Category.Buff;

			// Consumables
			if (Consumable.IsTileCategory(consumableCategory) ? !config.InfiniteTiles : !config.InfiniteConsumables)
				return true;

			return !player.HasInfinite(item.type, consumableCategory);

			
		}
		public override bool CanBeConsumedAsAmmo(Item item, Player player) {
			if (!ModContent.GetInstance<Config.ConsumableConfig>().InfiniteConsumables) return true;
			return !player.HasInfiniteAmmo(item);
		}

		private void HookItemSetDefaults(ILContext il) {

			Type[] args = new Type[] { typeof(Item), typeof(bool) };
			MethodBase setdefault_item_bool = typeof(ItemLoader).GetMethod("SetDefaults", BindingFlags.Static | BindingFlags.NonPublic, args);

			// IL code editing
			ILCursor c = new ILCursor(il);

			if (setdefault_item_bool == null || !c.TryGotoNext(i => i.MatchCall(setdefault_item_bool))) {
				Mod.Logger.Error("Unable to apply patch!");
				return;
			}

			c.Index -= args.Length;
			c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0); // item
			c.EmitDelegate((Item item) => {
				if (item.type < ItemID.Count) s_ItemMaxStack[item.type] = item.maxStack;
			});

			SetDefaultsHook = true;
		}

	}
}
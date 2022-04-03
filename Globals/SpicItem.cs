using System;
using System.Collections.Generic;

using System.Reflection;
using MonoMod.Cil;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			ConsumableExtension.ClearCache();
		}

		public override void SetDefaults(Item item) {
			if(item.tileWand != -1 && !WandAmmoExtension.IsInCache(item.type)) WandAmmoExtension.AddToCache(item.type);
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
			if (!SpysInfiniteConsumables.ShowConsumableCategory.Current) return;
			Configs.Custom custom = Configs.ConsumableConfig.Instance.GetCustom(item.type);
				
			string categoryTooltip = "";
			string toAdd = "";
			Categories.Consumable? consumable = item.GetConsumableCategory();
			if (custom?.Consumable?.Category == Categories.Consumable.None) toAdd = custom.Consumable.Infinity.ToString();
			else if (consumable.HasValue) {
				if (consumable.Value != Categories.Consumable.None) toAdd = consumable.Value.ToString();
			}else toAdd = "?";

			if (toAdd != "") {
				categoryTooltip += "C:" + toAdd + " ";
				toAdd = "";
			}

			Categories.Ammo ammo = item.GetAmmoCategory();
			if (custom?.Ammo?.Category == Categories.Ammo.None) categoryTooltip += $"A:{custom.Ammo.Infinity} ";
			else if (ammo != Categories.Ammo.None) categoryTooltip += $"A:{ammo} ";

			if(toAdd != "") {
				categoryTooltip += "A:" + toAdd + " ";
				toAdd = "";
			}
			Categories.GrabBag? grabBag = item.GetGrabBagCategory();
			if (custom?.GrabBag?.Category == Categories.GrabBag.None) toAdd = custom.GrabBag.Infinity.ToString();
			else if (grabBag.HasValue && grabBag.Value != Categories.GrabBag.None)
				toAdd = grabBag.Value.ToString();

			if (toAdd != "") {
				categoryTooltip += "B:" + toAdd + " ";
				toAdd = "";
			}

			Categories.WandAmmo? wand = item.GetWandAmmoCategory();
			if (custom?.WandAmmo?.Category == Categories.WandAmmo.None) toAdd = custom.WandAmmo.Infinity.ToString();
			else if (wand.HasValue && wand.Value != Categories.WandAmmo.None)
				toAdd = wand.Value.ToString();

			if (toAdd != "") categoryTooltip += "W:" + toAdd + " ";

			tooltips.Add(new TooltipLine(Mod, "Category", categoryTooltip));
			
		}

		public override bool? UseItem(Item item, Player player) {
			Categories.Consumable? category = item.GetConsumableCategory();

			if (!category.HasValue) player.GetModPlayer<SpicPlayer>().StartDetectingCategory(item.type);

			return null;
		}
		public override bool ConsumeItem(Item item, Player player) {
			Configs.ConsumableConfig config = ModContent.GetInstance<Configs.ConsumableConfig>();
			SpicPlayer modPlayer = player.GetModPlayer<SpicPlayer>();

			if (modPlayer.InItemCheck) {
				// Wands
				if (item != player.HeldItem) {
					if (!config.InfiniteTiles) return true;
					return player.HasInfinite(item.type, item.GetWandAmmoCategory() ?? Categories.WandAmmo.Block);
				}

				// Consumable used
				if(modPlayer.CheckingForCategory) modPlayer.StopDetectingCategory();
			}
			// Bags
			else if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease) {
				return !config.InfiniteConsumables || !player.HasInfinite(item.type, item.GetGrabBagCategory() ?? Categories.GrabBag.Crate);
			}

			Categories.Consumable consumableCategory = item.GetConsumableCategory() ?? Categories.Consumable.Buff;
			// Consumables
			if (consumableCategory.IsTile() ? !config.InfiniteTiles : !config.InfiniteConsumables) return true;

			return !player.HasInfinite(item.type, consumableCategory);

			
		}
		public override bool CanBeConsumedAsAmmo(Item item, Player player) {
			if (!ModContent.GetInstance<Configs.ConsumableConfig>().InfiniteConsumables) return true;
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
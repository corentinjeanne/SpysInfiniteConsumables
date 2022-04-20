using System;
using System.Collections.Generic;

using System.Reflection;
using MonoMod.Cil;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
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
			WandAmmoExtension.ClearCache();
		}

		public override void SetDefaults(Item item) {
			if(item.tileWand != -1 && !WandAmmoExtension.IsInCache(item.tileWand)) WandAmmoExtension.AddToCache(item.tileWand);
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
			if (item.playerIndexTheItemIsReservedFor == -1) return;

			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;
			Player player = Main.player[item.playerIndexTheItemIsReservedFor];
			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
			Categories.ItemCategories categories = spicPlayer.UpdateCategories(item);

			bool isTile = categories.Consumable?.IsTile() == true;
			if (categories.Consumable != Categories.Consumable.None) {

				TooltipLine consumable = isTile ?
					tooltips.FindorAddLine("Placeable", TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "Placeable", Lang.tip[33].Value, new(150, 150, 150))):
					tooltips.FindorAddLine("Consumable", TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "Consumable", Lang.tip[35].Value, new(150, 150, 150)));
				
				if (player.HasInfinite(item.type, categories.Consumable ?? Categories.Consumable.None, true)) {
					if (isTile) {
						if (config.InfiniteTiles) {
							consumable.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ consumable.Text;
							consumable.OverrideColor = new(0, 255, 0);
						}
					}
					else if (config.InfiniteConsumables) {
						consumable.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ consumable.Text;
						consumable.OverrideColor = new(255, 0, 0);
					}
				}
				if (config.ShowCategories) consumable.Text += $" ({Language.GetTextValue(categories.Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{categories.Consumable}" : "Mods.SPIC.ItemTooltip.Unknown")})";
			}
			if (!isTile && categories.WandAmmo.HasValue && categories.WandAmmo != Categories.WandAmmo.None) {
				TooltipLine wand = tooltips.AddLine(TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "WandAmmo", Language.GetTextValue("Mods.SPIC.ItemTooltip.WandAmmo"), new(150, 150, 150)), "Ammo");
				
				if (player.HasInfinite(item.type, categories.WandAmmo.Value) && config.InfiniteConsumables) {
					wand.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ wand.Text;
					wand.OverrideColor = new(0, 255, 0);
				}
				if (config.ShowCategories) wand.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.WandAmmo.{categories.WandAmmo}")})";
			}

			if (categories.Ammo != Categories.Ammo.None) {
				TooltipLine ammo = tooltips.FindorAddLine("Ammo", TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "Ammo", Lang.tip[34].Value, new(150, 150, 150)));
				if (player.HasInfinite(item.type, categories.Ammo) && config.InfiniteConsumables) {
					ammo.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ ammo.Text;
					ammo.OverrideColor = new(255, 0, 0);
				}
				if (config.ShowCategories) ammo.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Ammo.{categories.Ammo}")})";
			}

			if (categories.GrabBag.HasValue && categories.GrabBag != Categories.GrabBag.None) {
				TooltipLine bag = tooltips.AddLine(TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "GrabBag", Language.GetTextValue("Mods.SPIC.ItemTooltip.GrabBag"), new(150, 150, 150)), "Consumable");
				if (player.HasInfinite(item.type, categories.GrabBag.Value) && config.InfiniteConsumables){
					bag.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ bag.Text;
					bag.OverrideColor = new(255, 0, 0);
				}
				if (config.ShowCategories) bag.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.GrabBag.{categories.GrabBag}")})";
			}

				
			if (categories.Material != Categories.Material.None){
				TooltipLine material = tooltips.FindLine("Material");
				if (player.HasInfinite(item.type, categories.Material) && config.InfiniteCrafting) {
					material.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ material.Text;
					material.OverrideColor = new(255, 0, 255);
				}
				if (config.ShowCategories) material.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Material.{categories.Material}")})";
			}
		}

		public override bool? UseItem(Item item, Player player) {
			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();

			if (!spicPlayer.UpdateCategories(item).Consumable.HasValue) spicPlayer.StartDetectingCategory(item.type);

			return null;
		}
		public override bool ConsumeItem(Item item, Player player) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;
			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();

			Categories.ItemCategories categories = spicPlayer.UpdateCategories(item);
			// Item used
			if (spicPlayer.InItemCheck) {
				// Wands
				if (item != player.HeldItem) {
					if (!config.InfiniteTiles) return true;
					return player.HasInfinite(item.type, categories.WandAmmo ?? Categories.WandAmmo.Block);
				}

				// Consumable used
				if (spicPlayer.CheckingForCategory) {
					spicPlayer.TryStopDetectingCategory();
					categories = spicPlayer.UpdateCategories();
				}
			}
			// Bags
			else if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease) {
				return !config.InfiniteConsumables || !player.HasInfinite(item.type, categories.GrabBag ?? Categories.GrabBag.Crate);
			}

			// Consumables
			if (categories.Consumable?.IsTile() == true ? !config.InfiniteTiles : !config.InfiniteConsumables) return true;

			return !player.HasInfinite(item.type, categories.Consumable ?? Categories.Consumable.Buff);

			
		}
		public override bool CanBeConsumedAsAmmo(Item item, Player player) {
			Categories.Ammo ammo = player.GetModPlayer<SpicPlayer>().UpdateCategories(item).Ammo;
			
			if (!Configs.ConsumableConfig.Instance.InfiniteConsumables) return true;
			return !player.HasInfinite(item.type, ammo);
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
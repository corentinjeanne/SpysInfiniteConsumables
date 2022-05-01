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
        public override bool InstancePerEntity => true;
		public override GlobalItem Clone(Item item, Item itemClone){
			SpicItem inst = base.Clone(item, itemClone) as SpicItem;
            return inst;
        }
		public Categories.Consumable? Consumable { get; private set; }
		public Categories.Ammo Ammo { get; private set; }
		public Categories.GrabBag? GrabBag { get; private set; }
		public Categories.WandAmmo? WandAmmo { get; private set; }
		public Categories.Material Material { get; private set; }

		public override void Load() {
			s_ItemMaxStack = new int[ItemID.Count];
			IL.Terraria.Item.SetDefaults_int_bool += HookItemSetDefaults;

		}
        public override void Unload() {
			SetDefaultsHook = false;
			s_ItemMaxStack = null;
            WandAmmoExtension.ClearWandAmmos();
        }

		public override void SetDefaults(Item item) {
			if(item.tileWand != -1) WandAmmoExtension.SaveWandAmmo(item.tileWand);
			if(SpysInfiniteConsumables.Instance.ContentSetup) BuildCategories(item);
        }

		public void BuildCategories(Item item){
            Consumable = item.GetConsumableCategory();
            Ammo = item.GetAmmoCategory();
            WandAmmo = item.GetWandAmmoCategory();
            GrabBag = item.GetGrabBagCategory();
            Material = item.GetMaterialCategory();
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
			SpicPlayer spicPlayer = Main.player[item.playerIndexTheItemIsReservedFor].GetModPlayer<SpicPlayer>();
			bool isTile = Consumable?.IsTile() == true;
			if (Consumable != Categories.Consumable.None) {

				TooltipLine consumable = isTile ?
					tooltips.FindorAddLine("Placeable", TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "Placeable", Lang.tip[33].Value, new(150, 150, 150))):
					tooltips.FindorAddLine("Consumable", TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "Consumable", Lang.tip[35].Value, new(150, 150, 150)));
				
				if (spicPlayer.HasInfiniteConsumable(item.type)) {
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
				if (config.ShowCategories) consumable.Text += $" ({Language.GetTextValue(Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{Consumable}" : "Mods.SPIC.ItemTooltip.Unknown")})";
			}
			if (!isTile && WandAmmo.HasValue && WandAmmo != Categories.WandAmmo.None) {
				TooltipLine wand = tooltips.AddLine(TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "WandAmmo", Language.GetTextValue("Mods.SPIC.ItemTooltip.WandAmmo"), new(150, 150, 150)), "Ammo");
				
				if (spicPlayer.HasInfiniteWandAmmo(item.type) && config.InfiniteConsumables) {
					wand.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ wand.Text;
					wand.OverrideColor = new(0, 255, 0);
				}
				if (config.ShowCategories) wand.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.WandAmmo.{WandAmmo}")})";
			}

			if (Ammo != Categories.Ammo.None) {
				TooltipLine ammo = tooltips.FindorAddLine("Ammo", TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "Ammo", Lang.tip[34].Value, new(150, 150, 150)));
				if (spicPlayer.HasInfiniteAmmo(item.type) && config.InfiniteConsumables) {
					ammo.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ ammo.Text;
					ammo.OverrideColor = new(255, 0, 0);
				}
				if (config.ShowCategories) ammo.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Ammo.{Ammo}")})";
			}

			if (GrabBag.HasValue && GrabBag != Categories.GrabBag.None) {
				TooltipLine bag = tooltips.AddLine(TooltipHelper.NewLine(SpysInfiniteConsumables.Instance, "GrabBag", Language.GetTextValue("Mods.SPIC.ItemTooltip.GrabBag"), new(150, 150, 150)), "Consumable");
				if (spicPlayer.HasInfiniteGrabBag(item.type) && config.InfiniteConsumables){
					bag.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ bag.Text;
					bag.OverrideColor = new(255, 0, 0);
				}
				if (config.ShowCategories) bag.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.GrabBag.{GrabBag}")})";
			}

				
			if (Material != Categories.Material.None){
				TooltipLine material = tooltips.FindLine("Material");
				if (spicPlayer.HasInfiniteMaterial(item.type) && config.InfiniteCrafting) {
					material.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ material.Text;
					material.OverrideColor = new(255, 0, 255);
				}
				if (config.ShowCategories) material.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Material.{Material}")})";
			}
		}

        public override bool? UseItem(Item item, Player player) {
			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
			if (!Consumable.HasValue) spicPlayer.StartDetectingCategory(item.type);
			return null;
		}
		public override bool ConsumeItem(Item item, Player player) {
			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;
			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();

			// Item used
			if (spicPlayer.InItemCheck) {
                // Wands
                if (item != player.HeldItem) {
                    if (!WandAmmo.HasValue && Consumable?.IsTile() != true) {
                        Configs.ConsumableConfig.Instance.SaveWandAmmoCategory(item.type);
                        spicPlayer.RebuildCategories(item.type);
                    }
                    return !(config.InfiniteTiles && spicPlayer.HasInfiniteWandAmmo(item));
				}

				// Consumable used
				if (spicPlayer.CheckingForCategory) spicPlayer.TryStopDetectingCategory();
			}

			else {
				// Bags
				if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease){
					if(!GrabBag.HasValue) {
                        config.SaveGrabBagCategory(item.type);
                        spicPlayer.RebuildCategories(item.type);
                    }
                    return !(config.InfiniteConsumables && spicPlayer.HasInfiniteGrabBag(item.type));
				}

				// Hotkeys
			}

			// Consumables
			if (Consumable?.IsTile() == true ? !config.InfiniteTiles : !config.InfiniteConsumables) return true;

			return !spicPlayer.HasInfiniteConsumable(item.type);
		}
		public override bool CanBeConsumedAsAmmo(Item ammo, Player player) {
			if (!Configs.ConsumableConfig.Instance.InfiniteConsumables) return true;
			
			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
			return !spicPlayer.HasInfiniteAmmo(ammo.type);
		}
		public override bool? CanConsumeBait(Player player, Item bait) {
			if (!Configs.ConsumableConfig.Instance.InfiniteConsumables) return null;

			SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
			return spicPlayer.HasInfiniteConsumable(bait.type) ? false : null;
		}

		private void HookItemSetDefaults(ILContext il) {
			
			Type[] args = { typeof(Item), typeof(bool) };
			MethodBase setdefault_item_bool = typeof(ItemLoader).GetMethod("SetDefaults", BindingFlags.Static | BindingFlags.NonPublic, args);

			// IL code editing
			ILCursor c = new (il);

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
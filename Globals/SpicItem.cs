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
            WandAmmoExtension.ClearWandAmmos();
        }

        public override void SetDefaults(Item item) {
            if(item.tileWand != -1) WandAmmoExtension.SaveWandAmmo(item.tileWand);
        }

        public override void SetStaticDefaults() {
            if (!SetDefaultsHook) return;

            // FIXME probably not working for loaded items before or after the load
            Array.Resize(ref s_ItemMaxStack, ItemLoader.ItemCount);
            for (int type = ItemID.Count; type < ItemLoader.ItemCount; type++) {
                ModItem modItem = ItemLoader.GetItem(type).Clone(new());
                modItem.SetDefaults();
                s_ItemMaxStack[type] = modItem.Item.maxStack != 0 ? modItem.Item.maxStack : 1;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            
            Configs.Infinities config = Configs.Infinities.Instance;
            Configs.CategorySettings autos = Configs.CategorySettings.Instance;

            Categories.Categories categories = item.GetCategories();
            bool isTile = categories.Consumable?.IsTile() == true;

            TooltipLine ammo = null, consumable = null, bag = null, wand = null, material = null;

            static TooltipLine AddedLine(string name, string value) => new(SpysInfiniteConsumables.Instance, name, value) {
                OverrideColor = new(150, 150, 150)
            };
            // ? MAke static
            TooltipLine consumableLine = isTile ? AddedLine("Placeable",Lang.tip[33].Value) : AddedLine("Consumable", Lang.tip[35].Value);
            TooltipLine wandLine = AddedLine("WandAmmo", Language.GetTextValue("Mods.SPIC.ItemTooltip.WandAmmo"));
            TooltipLine ammoLine = AddedLine("Ammo", Lang.tip[34].Value);
            TooltipLine bagLine = AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.ItemTooltip.GrabBag"));
            string materialLine = "Material";

            if(autos.ShowCategories){
                Configs.CustomInfinities infs = config.GetCustomInfinities(item.type);

                if(infs.Consumable.HasValue) {
                    consumable ??= tooltips.FindorAddLine(consumableLine);
                    consumable.Text += $" ({infs.Consumable} items)";
                }
                else if (categories.Consumable != Categories.Consumable.None){
                    consumable ??= tooltips.FindorAddLine(consumableLine);
                    consumable.Text += $" ({Language.GetTextValue(categories.Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{categories.Consumable}" : "Mods.SPIC.Categories.Unknown")})";
                }
                if(!isTile) {
                    if(infs.WandAmmo.HasValue) {
                        wand ??= tooltips.FindorAddLine(wandLine, "Ammo");
                        wand.Text += $" ({infs.Consumable} items)";
                    }
                    else if (categories.WandAmmo.HasValue && categories.WandAmmo != Categories.WandAmmo.None) {
                        wand ??= tooltips.FindorAddLine(wandLine, "Ammo");
                        wand.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.WandAmmo.{categories.WandAmmo}")})";
                    }
                }

                if(infs.GrabBag.HasValue) {
                    bag ??= tooltips.FindorAddLine(bagLine, "Consumable");
                    bag.Text += $" ({infs.Consumable} items)";
                }
                else if (categories.GrabBag.HasValue && categories.GrabBag != Categories.GrabBag.None) {
                    bag ??= tooltips.FindorAddLine(bagLine, "Consumable");
                    bag.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.GrabBag.{categories.GrabBag}")})";
                }

                if(infs.Ammo.HasValue) {
                    ammo ??= tooltips.FindorAddLine(ammoLine);
                    ammo.Text += $" ({infs.Ammo} items)";
                }
                else if (categories.Ammo != Categories.Ammo.None) {
                    ammo ??= tooltips.FindorAddLine(ammoLine);
                    ammo.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Ammo.{categories.Ammo}")})";
                }

                if (categories.Material != Categories.Material.None) {
                    material ??= tooltips.FindLine(materialLine);
                    material.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Material.{categories.Material}")})";
                }
            }

            if (autos.ShowInfinites && item.playerIndexTheItemIsReservedFor >= 0) {
                SpicPlayer spicPlayer = Main.player[item.playerIndexTheItemIsReservedFor].GetModPlayer<SpicPlayer>();

                if(config.InfiniteConsumables){
                    if (!isTile && spicPlayer.HasInfiniteConsumable(item.type)) {
                        consumable ??= tooltips.FindorAddLine(consumableLine);
                        consumable.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + consumable.Text;
                        consumable.OverrideColor = new(255, 0, 0);
                    }
                    if(spicPlayer.HasInfiniteAmmo(item.type)){
                        ammo ??= tooltips.FindorAddLine(ammoLine, "Ammo");
                        ammo.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + ammo.Text;
                        ammo.OverrideColor = new(255, 0, 0);
                    }
                    if (spicPlayer.HasInfiniteGrabBag(item.type)) {
                        bag ??= tooltips.FindorAddLine(bagLine, "Consumable");
                        bag.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + bag.Text;
                        bag.OverrideColor = new(255, 0, 0);
                    }
                }
                if(config.InfiniteTiles){
                    if (isTile){
                        if (spicPlayer.HasInfiniteConsumable(item.type)) {
                            consumable ??= tooltips.FindorAddLine(consumableLine);
                            consumable.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + consumable.Text;
                            consumable.OverrideColor = new(255, 0, 0);
                        }
                    }else if(spicPlayer.HasInfiniteWandAmmo(item.type)){
                        wand ??= tooltips.FindorAddLine(wandLine, "Ammo");
                        wand.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + wand.Text;
                        wand.OverrideColor = new(255, 0, 0);
                    }
                }
                if (config.InfiniteCrafting && spicPlayer.HasInfiniteMaterial(item.type)) {
                    material ??= tooltips.FindLine(materialLine);
                    material.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + material.Text;
                    material.OverrideColor = new(255, 0, 0);
                }
            }
        }

        public override bool? UseItem(Item item, Player player) {
            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
            if (!item.GetCategories().Consumable.HasValue) spicPlayer.StartDetectingCategory(item.type);
            return null;
        }
        
        public override bool ConsumeItem(Item item, Player player) {
            Configs.Infinities infinities = Configs.Infinities.Instance;
            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();

            Categories.Categories categories = item.GetCategories();
            // Item used
            if (spicPlayer.InItemCheck) {
                // Wands
                if (item != player.HeldItem) {
                    if (!categories.WandAmmo.HasValue && categories.Consumable?.IsTile() != true) {
                        Configs.CategorySettings.Instance.SaveWandAmmoCategory(item.type);
                    }
                    return !(infinities.InfiniteTiles && spicPlayer.HasInfiniteWandAmmo(item));
                }

                // Consumable used
                if (spicPlayer.CheckingForCategory) spicPlayer.TryStopDetectingCategory();
            }

            else {
                // Bags
                if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease){
                    if(!categories.GrabBag.HasValue) {
                        Configs.CategorySettings.Instance.SaveGrabBagCategory(item.type);
                    }
                    return !(infinities.InfiniteConsumables && spicPlayer.HasInfiniteGrabBag(item.type));
                }

                // Hotkeys
            }

            // Consumables
            if (categories.Consumable?.IsTile() == true ? !infinities.InfiniteTiles : !infinities.InfiniteConsumables) return true;

            return !spicPlayer.HasInfiniteConsumable(item.type);
        }
        
        public override bool CanBeConsumedAsAmmo(Item ammo, Player player) {
            if (!Configs.Infinities.Instance.InfiniteConsumables) return true;
            
            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
            return !spicPlayer.HasInfiniteAmmo(ammo.type);
        }
        
        public override bool? CanConsumeBait(Player player, Item bait) {
            if (!Configs.Infinities.Instance.InfiniteConsumables) return null;

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
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

            static TooltipLine AddedLine(string name, string value) => new(SpysInfiniteConsumables.Instance, name, value) {
                OverrideColor = new(150, 150, 150)
            };

            Configs.Infinities config = Configs.Infinities.Instance;
            Configs.CategorySettings autos = Configs.CategorySettings.Instance;

            SpicPlayer spicPlayer = Main.player[item.playerIndexTheItemIsReservedFor].GetModPlayer<SpicPlayer>();

            bool isTile = Consumable?.IsTile() == true;
            if (Consumable != Categories.Consumable.None) {

                TooltipLine consumableLine = AddedLine(isTile ? "Placeable" : "Consumable", isTile ? Lang.tip[33].Value : Lang.tip[35].Value);
                TooltipLine consumable = null;

                if (spicPlayer.HasInfiniteConsumable(item.type)) {
                    if (isTile) {
                        if (config.InfiniteTiles) {
                            consumable ??= tooltips.FindorAddLine(consumableLine);
                            consumable.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ consumable.Text;
                            consumable.OverrideColor = new(0, 255, 0);
                        }
                    }
                    else if (config.InfiniteConsumables) {
                        consumable ??= tooltips.FindorAddLine(consumableLine);
                        consumable.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ consumable.Text;
                        consumable.OverrideColor = new(255, 0, 0);
                    }
                }
                if (autos.ShowCategories) {
                    consumable ??= tooltips.FindorAddLine(consumableLine);
                    consumable.Text += $" ({Language.GetTextValue(Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{Consumable}" : "Mods.SPIC.ItemTooltip.Unknown")})";
                }
            }

            if (!isTile && WandAmmo.HasValue && WandAmmo != Categories.WandAmmo.None) {
                TooltipLine wandLine = AddedLine("WandAmmo", Language.GetTextValue("Mods.SPIC.ItemTooltip.WandAmmo"));
                TooltipLine wand = null;
                if (spicPlayer.HasInfiniteWandAmmo(item.type) && config.InfiniteTiles) {
                    wand ??= tooltips.FindorAddLine(wandLine, "Ammo");
                    wand.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ wand.Text;
                    wand.OverrideColor = new(0, 255, 0);
                    tooltips.AddLine("Ammo", wand);
                }
                if (autos.ShowCategories) {
                    wand ??= tooltips.FindorAddLine(wandLine, "Ammo");
                    wand.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.WandAmmo.{WandAmmo}")})";
                }
            }

            if (Ammo != Categories.Ammo.None) {
                TooltipLine ammoLine = AddedLine("Ammo", Lang.tip[34].Value);
                TooltipLine ammo = null;
                if (spicPlayer.HasInfiniteAmmo(item.type) && config.InfiniteConsumables) {
                    ammo ??= tooltips.FindorAddLine(ammoLine);
                    ammo.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ ammo.Text;
                    ammo.OverrideColor = new(255, 0, 0);
                }
                if (autos.ShowCategories) {
                    ammo ??= tooltips.FindorAddLine(ammoLine);
                    ammo.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Ammo.{Ammo}")})";
                }
            }

            if (GrabBag.HasValue && GrabBag != Categories.GrabBag.None) {
                TooltipLine bagLine = AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.ItemTooltip.GrabBag"));
                TooltipLine bag = null;
                if (spicPlayer.HasInfiniteGrabBag(item.type) && config.InfiniteConsumables){
                    bag ??= tooltips.FindorAddLine(bagLine);
                    tooltips.AddLine("Consumable", bag);
                    bag.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ bag.Text;
                    bag.OverrideColor = new(255, 0, 0);
                }
                if (autos.ShowCategories){
                    bag ??= tooltips.FindorAddLine(bagLine);
                    bag.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.GrabBag.{GrabBag}")})";
                }
            }
            
            if (Material != Categories.Material.None){
                TooltipLine material = tooltips.FindLine("Material");
                if (spicPlayer.HasInfiniteMaterial(item.type) && config.InfiniteCrafting) {
                    material.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite")+" "+ material.Text;
                    material.OverrideColor = new(255, 0, 255);
                }
                if (autos.ShowCategories) material.Text += $" ({Language.GetTextValue($"Mods.SPIC.Categories.Material.{Material}")})";
            }
        }

        public override bool? UseItem(Item item, Player player) {
            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
            if (!Consumable.HasValue) spicPlayer.StartDetectingCategory(item.type);
            return null;
        }
        
        public override bool ConsumeItem(Item item, Player player) {
            Configs.Infinities infinities = Configs.Infinities.Instance;
            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();

            // Item used
            if (spicPlayer.InItemCheck) {
                // Wands
                if (item != player.HeldItem) {
                    if (!WandAmmo.HasValue && Consumable?.IsTile() != true) {
                        Configs.CategorySettings.Instance.SaveWandAmmoCategory(item.type);
                        spicPlayer.RebuildCategories(item.type);
                    }
                    return !(infinities.InfiniteTiles && spicPlayer.HasInfiniteWandAmmo(item));
                }

                // Consumable used
                if (spicPlayer.CheckingForCategory) spicPlayer.TryStopDetectingCategory();
            }

            else {
                // Bags
                if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease){
                    if(!GrabBag.HasValue) {
                        Configs.CategorySettings.Instance.SaveGrabBagCategory(item.type);
                        spicPlayer.RebuildCategories(item.type);
                    }
                    return !(infinities.InfiniteConsumables && spicPlayer.HasInfiniteGrabBag(item.type));
                }

                // Hotkeys
            }

            // Consumables
            if (Consumable?.IsTile() == true ? !infinities.InfiniteTiles : !infinities.InfiniteConsumables) return true;

            return !spicPlayer.HasInfiniteConsumable(item.type);
        }
        
        public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) {
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
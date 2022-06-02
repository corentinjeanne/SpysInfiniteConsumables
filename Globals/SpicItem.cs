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
            Categories.Infinities infinities = item.GetInfinities();
            bool isTile = categories.Consumable?.IsTile() == true;

            SpicPlayer spicPlayer = Main.player[item.playerIndexTheItemIsReservedFor].GetModPlayer<SpicPlayer>();
            
            static TooltipLine AddedLine(string name, string value) => new(SpysInfiniteConsumables.Instance, name, value) {
                OverrideColor = new(150, 150, 150)
            };
            TooltipLine consumable = isTile ? AddedLine("Placeable",Lang.tip[33].Value) : AddedLine("Consumable", Lang.tip[35].Value);
            TooltipLine wand = AddedLine("WandAmmo", Language.GetTextValue("Mods.SPIC.ItemTooltip.WandAmmo"));
            TooltipLine ammo = AddedLine("Ammo", Lang.tip[34].Value);
            TooltipLine bag = AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.ItemTooltip.GrabBag"));
            TooltipLine material = AddedLine("Material", null);

            bool ModifyLine(TooltipLine toEdit, string missing, bool displayCategory, string categoryKey, int requirement, bool infinite, Microsoft.Xna.Framework.Color color){ // category, req, inf
                TooltipLine line = null;
                TooltipLine Setline() => line ??= tooltips.FindorAddLine(toEdit, missing);
                if(autos.ShowInfinities && infinite) {
                    Setline();
                    line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite") + " " + line.Text;
                    line.OverrideColor = color;
                }

                int total = 0;
                string Separator() => total++ == 0 ? " (" : ", ";
                
                if(autos.ShowCategories && displayCategory) Setline().Text += Separator() + Language.GetTextValue(categoryKey);
                if(autos.ShowRequirement && requirement != 0) Setline().Text += Separator() + Language.GetTextValue("Mods.SPIC.ItemTooltip.RequirementTooltip", requirement);

                if(total != 0) line.Text += ")";
                return line != null;
            }

            ModifyLine(ammo, null,
                categories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{categories.Ammo}",
                infinities.Ammo, config.InfiniteConsumables && spicPlayer.HasInfiniteAmmo(item.type), new(0,255,0)
            );
            if (!(ModifyLine(consumable, null,
                    categories.Consumable != Categories.Consumable.None, $"Mods.SPIC.Categories.Consumable.{categories.Consumable}",
                    infinities.Consumable, spicPlayer.HasInfiniteConsumable(item.type), new(0, 0, 255)) && isTile)){
                ModifyLine(wand, "Placeable",
                    categories.WandAmmo.HasValue && categories.WandAmmo != Categories.WandAmmo.None, $"Mods.SPIC.Categories.WandAmmo.{categories.WandAmmo}",
                    infinities.WandAmmo, config.InfiniteTiles && spicPlayer.HasInfiniteWandAmmo(item.type), new(255, 0, 0)
                );
            }
            ModifyLine(bag, "Consumable",
                categories.GrabBag.HasValue && categories.GrabBag != Categories.GrabBag.None, $"Mods.SPIC.Categories.GrabBag.{categories.GrabBag}",
                infinities.GrabBag, config.InfiniteGrabBags && spicPlayer.HasInfiniteGrabBag(item.type), new(255, 0, 255)
            );
            ModifyLine(material, null,
                categories.Material != Categories.Material.None, $"Mods.SPIC.Categories.Material.{categories.Material}",
                infinities.Material, config.InfiniteCrafting && spicPlayer.HasInfiniteMaterial(item.type), new(255, 150, 150)
            );
        }

        public override bool? UseItem(Item item, Player player) {
            if (Configs.CategorySettings.Instance.AutoCategories && !item.GetCategories().Consumable.HasValue)
                player.GetModPlayer<SpicPlayer>().StartDetectingCategory(item.type);
            return null;
        }
        
        public override bool ConsumeItem(Item item, Player player) {
            Configs.Infinities infinities = Configs.Infinities.Instance;
            Configs.CategorySettings autos = Configs.CategorySettings.Instance;

            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
            Categories.Categories categories = item.GetCategories();

            // Item used
            if (spicPlayer.InItemCheck) {
                // Wands
                if (item != player.HeldItem) {
                    if (autos.AutoCategories && !categories.WandAmmo.HasValue && categories.Consumable?.IsTile() != true)
                        Configs.CategorySettings.Instance.SaveWandAmmoCategory(item.type);

                    return !(infinities.InfiniteTiles && spicPlayer.HasInfiniteWandAmmo(item));
                }

                // Consumable used
                spicPlayer.TryStopDetectingCategory();
            }

            else {
                // Bags
                if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease){
                    if(autos.AutoCategories && !categories.GrabBag.HasValue)
                        Configs.CategorySettings.Instance.SaveGrabBagCategory(item.type);
                    return !(infinities.InfiniteGrabBags && spicPlayer.HasInfiniteGrabBag(item.type));
                }

                // ? Hotkeys detect buff
            }

            // Consumables
            return !((categories.Consumable?.IsTile() == true ? infinities.InfiniteTiles : infinities.InfiniteConsumables)
                && spicPlayer.HasInfiniteConsumable(item.type));
        }

        public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) {
            return !(Configs.Infinities.Instance.InfiniteConsumables
                && player.GetModPlayer<SpicPlayer>().HasInfiniteAmmo(ammo.type));
        }

        public override bool? CanConsumeBait(Player player, Item bait) {
            return Configs.Infinities.Instance.InfiniteConsumables
                && player.GetModPlayer<SpicPlayer>().HasInfiniteConsumable(bait.type) ?
                    false : null;
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
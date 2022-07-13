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
        public static int MaxStack(int type) => s_ItemMaxStack[type];
        public static bool SetDefaultsHook { get; private set; }


        public override void Load() {
            s_ItemMaxStack = new int[ItemID.Count];
            IL.Terraria.Item.SetDefaults_int_bool += Hook_ItemSetDefaults;
        }
        public override void Unload() {
            SetDefaultsHook = false;
            s_ItemMaxStack = null;
        }

        public override void SetDefaults(Item item) {
            if(item.tileWand != -1) PlaceableExtension.RegisterWandAmmo(item);
            if (item.FitsAmmoSlot() && item.mech) PlaceableExtension.RegisterWandAmmo(item.type, Categories.Placeable.Wiring);
        }

        public override void SetStaticDefaults() {
            if (!SetDefaultsHook){
                for (int type = 0; type < ItemID.Count; type++){
                    Item item = new(type);
                    s_ItemMaxStack[type] = item.maxStack != 0 ? item.maxStack : 1;
                }
            }
            
            Array.Resize(ref s_ItemMaxStack, ItemLoader.ItemCount);
            for (int type = ItemID.Count; type < ItemLoader.ItemCount; type++) {
                ModItem modItem = ItemLoader.GetItem(type).Clone(new());
                modItem.SetDefaults();
                s_ItemMaxStack[type] = modItem.Item.maxStack != 0 ? modItem.Item.maxStack : 1;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            
            Configs.Requirements config = Configs.Requirements.Instance;
            Configs.IntemInfiDisplay visuals = Configs.IntemInfiDisplay.Instance;
            
            Categories.ItemCategories categories = item.GetCategories();
            Categories.ItemRequirements infinities = item.GetRequirements();

            SpicPlayer spicPlayer = Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>();

            static TooltipLine AddedLine(string name, string value) => new(SpysInfiniteConsumables.Instance, name, value) {
                OverrideColor = new(150, 150, 150)
            };
            TooltipLine consumable = AddedLine("Consumable", Lang.tip[35].Value);
            TooltipLine placeable = AddedLine("Placeable",Lang.tip[33].Value);
            TooltipLine wand = AddedLine("Placeable", Language.GetTextValue("Mods.SPIC.ItemTooltip.wandAmmo"));
            TooltipLine ammo = AddedLine("Ammo", Lang.tip[34].Value);
            TooltipLine bag = AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
            TooltipLine material = AddedLine("Material", null);
            TooltipLine currency = AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.Categories.Currency.name"));

            void ModifyLine(bool active, TooltipLine toEdit, string missing, bool displayCategory, string categoryKey, int requirement, bool infinite, Microsoft.Xna.Framework.Color color, object infinity = null){ // category, req, inf
                if(!active) return;
                
                TooltipLine line = null;
                TooltipLine Line() => line ??= tooltips.FindorAddLine(toEdit, missing);

                if(visuals.ShowInfinities && infinite) {
                    Line().Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.Infinite", line.Text);
                    line.OverrideColor = color;

                    if(infinity != null){
                        line.Text += " " + Language.GetTextValue("Mods.SPIC.ItemTooltip.infiniteMid", infinity) + " ";
                        if(infinity is int or long)
                            infinity = new List<KeyValuePair<int, long>> {new(item.type, (long)infinity) };

                        if(infinity is List<KeyValuePair<int,long>> l && l.Count > 0){
                            for (int i = 0; i < l.Count; i++) {
                                if(i != l.Count-1) line.Text += ' ';
                                line.Text += Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteSprite", l[i].Key, l[i].Value);
                            }
                        } 
                    }
                }

                int total = 0;
                string Separator() => total++ == 0 ? " (" : ", ";
                
                if(visuals.ShowCategories && displayCategory) Line().Text += Separator() + Language.GetTextValue(categoryKey);
                if(visuals.ShowRequirement && requirement != 0 && !infinite) Line().Text += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.Requirement", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.RequirementStacks", -requirement));
                if(total != 0) line.Text += ")";
            }
            ModifyLine(config.InfiniteConsumables, ammo, null,
                categories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{categories.Ammo}",
                infinities.Ammo, spicPlayer.HasInfiniteAmmo(item.type), visuals.color_Ammo
            );
            ModifyLine(config.InfiniteConsumables, consumable, null,
                categories.Consumable != Categories.Consumable.None, categories.Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{categories.Consumable}" : $"Mods.SPIC.Categories.Unknown",
                infinities.Consumable, spicPlayer.HasInfiniteConsumable(item.type), visuals.color_Consumables
            );
            ModifyLine(config.InfinitePlaceables, item.Placeable() || !PlaceableExtension.IsWandAmmo(item.type) ? placeable : wand, "Placeable",
                categories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{categories.Placeable}",
                infinities.Placeable, spicPlayer.HasInfinitePlaceable(item.type, true), visuals.color_Placeable
            );
            ModifyLine(config.InfiniteGrabBags, bag, "Consumable",
                categories.GrabBag.HasValue && categories.GrabBag != Categories.GrabBag.None, $"Mods.SPIC.Categories.GrabBag.{categories.GrabBag}",
                infinities.GrabBag, spicPlayer.HasInfiniteGrabBag(item.type), visuals.color_Bags
            );
            ModifyLine(config.InfiniteMaterials, material, null,
                categories.Material != Categories.Material.None, $"Mods.SPIC.Categories.Material.{categories.Material}",
                infinities.Material, spicPlayer.HasInfiniteMaterial(item.type, out long inf), visuals.color_Materials, inf
            );

            int currencyType = item.CurrencyType();
            if (currencyType != -2) {
                ModifyLine(config.InfiniteCurrencies, currency, "Material",
                    categories.Currency != Categories.Currency.None, $"Mods.SPIC.Categories.Currency.{categories.Currency}",
                    infinities.Currency, spicPlayer.HasInfiniteCurrency(currencyType, out inf), visuals.color_Currencies, CurrencyExtension.CurrencyCountToItems(currencyType, inf)
                );
            }
        }

        public override bool? UseItem(Item item, Player player) {

            if (Configs.CategoryDetection.Instance.AutoCategories && !item.GetCategories().Consumable.HasValue)
                player.GetModPlayer<SpicPlayer>().StartDetectingCategory(item);
            return null;
        }

        public override bool ConsumeItem(Item item, Player player) {
            Configs.Requirements infinities = Configs.Requirements.Instance;
            Configs.CategoryDetection autos = Configs.CategoryDetection.Instance;

            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
            

            // Item used
            if (spicPlayer.InItemCheck) {
                // Wands
                if (item != player.HeldItem) {
                    if (autos.AutoCategories && item.GetCategories().Placeable == Categories.Placeable.None)
                        Configs.CategoryDetection.Instance.DetectedPlaceable(item, Categories.Placeable.Block);

                    return !(infinities.InfinitePlaceables && spicPlayer.HasInfinitePlaceable(item.type));
                }

                // Consumable used
                spicPlayer.TryDetectCategory();
            }

            else {
                // Bags
                if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease){
                    if(autos.AutoCategories && !item.GetCategories().GrabBag.HasValue)
                        Configs.CategoryDetection.Instance.DetectedGrabBag(item);
                    return !(infinities.InfiniteGrabBags && spicPlayer.HasInfiniteGrabBag(item.type));
                }

                // ? Hotkeys detect buff
            }

            // Consumables
            Categories.ItemCategories categories = item.GetCategories();
            if(categories.Consumable != Categories.Consumable.None)
                return !(infinities.InfiniteConsumables && spicPlayer.HasInfiniteConsumable(item.type));
            if(item.Placeable())
                return !(infinities.InfinitePlaceables && spicPlayer.HasInfinitePlaceable(item.type));
            return !(infinities.InfiniteGrabBags && spicPlayer.HasInfiniteGrabBag(item.type));
        }

        public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
            => !(Configs.Requirements.Instance.InfiniteConsumables && player.GetModPlayer<SpicPlayer>().HasInfiniteAmmo(ammo.type));

        public override bool? CanConsumeBait(Player player, Item bait) 
            => !(Configs.Requirements.Instance.InfiniteConsumables && player.GetModPlayer<SpicPlayer>().HasInfiniteConsumable(bait.type)) ?
                null : false;

        private void Hook_ItemSetDefaults(ILContext il) {
            SetDefaultsHook = false;
            Type[] args = { typeof(Item), typeof(bool) };
            MethodBase setdefault_item_bool = typeof(ItemLoader).GetMethod(
                nameof(Item.SetDefaults),
                BindingFlags.Static | BindingFlags.NonPublic,
                args
            );

            // IL code editing
            ILCursor c = new (il);

            if (setdefault_item_bool == null || !c.TryGotoNext(i => i.MatchCall(setdefault_item_bool))) {
                Mod.Logger.Error("Unable to apply patch!");
                return;
            }

            c.Index -= args.Length;
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            c.EmitDelegate((Item item) => {
                if (item.type < ItemID.Count) s_ItemMaxStack[item.type] = item.maxStack;
            });

            SetDefaultsHook = true;
        }

        public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount){
            SpicPlayer spicPlayer = Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>();
            if(spicPlayer.HasInfiniteCurrency(-1, reforgePrice)){
                reforgePrice = 0;
                return true;
            }
            return false;
        }
    }
}
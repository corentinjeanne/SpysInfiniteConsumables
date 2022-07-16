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
            
            System.Array.Resize(ref s_ItemMaxStack, ItemLoader.ItemCount);
            for (int type = ItemID.Count; type < ItemLoader.ItemCount; type++) {
                ModItem modItem = ItemLoader.GetItem(type).Clone(new());
                modItem.SetDefaults();
                s_ItemMaxStack[type] = modItem.Item.maxStack != 0 ? modItem.Item.maxStack : 1;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            
            Configs.Requirements settings = Configs.Requirements.Instance;
            Configs.IntemInfiDisplay visuals = Configs.IntemInfiDisplay.Instance;
            
            Player player = Main.player[Main.myPlayer];
            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();

            Categories.ItemCategories categories = item.GetCategories();
            Categories.ItemRequirements requirements = item.GetRequirements();
            Categories.ItemInfinities infinities = spicPlayer.GetInfinities(item);
            int currencyType = item.CurrencyType();
            Categories.Currency c_category = CategoryHelper.GetCategory(currencyType);
            int c_requirement = CategoryHelper.GetRequirement(currencyType);
            long c_infinity = spicPlayer.GetCurrencyInfinity(currencyType);

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
            void ModifyLine(bool active, TooltipLine toEdit, string missing, bool displayCategory, string categoryKey, int requirement, bool infinite, Microsoft.Xna.Framework.Color color, params KeyValuePair<int, long>[] infinities){ // category, req, inf
                if(!active) return;
                
                TooltipLine line = null;
                TooltipLine Line() => line ??= tooltips.FindorAddLine(toEdit, missing);

                if(visuals.ShowInfinities && infinite) {
                    Line().OverrideColor = color;
                    if(infinities.Length == 0){
                        line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line.Text);
                    }else {
                        string items = "";
                        for (int i = 0; i < infinities.Length; i++) {
                            if(i != infinities.Length-1) line.Text += ' ';
                            items += Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteSprite", infinities[i].Key, infinities[i].Value);
                        }
                        line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.infiniteFull", line.Text, items);
                    }
                }

                int total = 0;
                string Separator() => total++ == 0 ? " (" : ", ";
                
                if(visuals.ShowCategories && displayCategory) Line().Text += Separator() + Language.GetTextValue(categoryKey);
                if(visuals.ShowRequirement && requirement > 0 && !infinite) Line().Text += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.Requirement", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.RequirementStacks", -requirement));
                if(total > 0) line.Text += ")";
            }
            ModifyLine(settings.InfiniteConsumables, ammo, null,
                categories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{categories.Ammo}",
                requirements.Ammo, infinities.Ammo > -1, visuals.color_Ammo
            );
            if(item.useAmmo != AmmoID.None && player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)){
                Item ammoItem = System.Array.Find(player.inventory, i => i.type == ammoType);
                if (ammoItem is not null) {
                    Categories.ItemCategories ammoCategories = ammoItem.GetCategories();
                    Categories.ItemRequirements ammoRequirements = ammoItem.GetRequirements();
                    Categories.ItemInfinities ammoInfinities = spicPlayer.GetInfinities(ammoItem);

                    TooltipLine ammo2 = AddedLine("Ammo", Lang.tip[34].Value + $" [i:{ammoType}]");
                        ModifyLine(settings.InfiniteConsumables, ammo2, null,
                        ammoCategories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{ammoCategories.Ammo}",
                        ammoRequirements.Ammo, ammoInfinities.Ammo > -1, visuals.color_Ammo
                    );
                }
            }
            ModifyLine(settings.InfiniteConsumables, consumable, null,
                categories.Consumable != Categories.Consumable.None, categories.Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{categories.Consumable}" : $"Mods.SPIC.Categories.Unknown",
                requirements.Consumable, infinities.Consumable > -1, visuals.color_Consumables
            );
            ModifyLine(settings.InfinitePlaceables, item.Placeable() || !PlaceableExtension.IsWandAmmo(item.type) ? placeable : wand, "Placeable",
                categories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{categories.Placeable}",
                requirements.Placeable, infinities.Placeable > -1, visuals.color_Placeable
            );
            if (item.tileWand != -1) {
                Item wandItem = System.Array.Find(player.inventory, i => i.type == item.tileWand);
                if(wandItem is not null){
                    TooltipLine wand2 = AddedLine("WandConsumes", null);
                    Categories.ItemCategories wandCategories = wandItem.GetCategories();
                    Categories.ItemRequirements wandRequirements = wandItem.GetRequirements();
                    Categories.ItemInfinities wandInfinities = spicPlayer.GetInfinities(wandItem);
                    ModifyLine(settings.InfinitePlaceables, wand2, null,
                        wandCategories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{wandCategories.Placeable}",
                        wandRequirements.Placeable, wandInfinities.Placeable > -1, visuals.color_Placeable
                    );
                }
            }
            ModifyLine(settings.InfiniteGrabBags, bag, "Consumable",
                categories.GrabBag.HasValue && categories.GrabBag != Categories.GrabBag.None, $"Mods.SPIC.Categories.GrabBag.{categories.GrabBag}",
                requirements.GrabBag, infinities.GrabBag > -1, visuals.color_Bags
            );
            ModifyLine(settings.InfiniteMaterials, material, null,
                categories.Material != Categories.Material.None, $"Mods.SPIC.Categories.Material.{categories.Material}",
                requirements.Material, infinities.Material > -1, visuals.color_Materials, new KeyValuePair<int, long>(item.type, infinities.Material)
            );
            ModifyLine(settings.InfiniteCurrencies, currency, "Material",
                c_category != Categories.Currency.None, $"Mods.SPIC.Categories.Currency.{c_category}",
                c_requirement, c_infinity > -1, visuals.color_Currencies, CurrencyExtension.CurrencyCountToItems(currencyType, c_infinity).ToArray()
            );
        }

        public override bool ConsumeItem(Item item, Player player) {
            Configs.Requirements settings = Configs.Requirements.Instance;
            Configs.CategoryDetection detected = Configs.CategoryDetection.Instance;

            SpicPlayer spicPlayer = player.GetModPlayer<SpicPlayer>();
            Categories.ItemCategories categories;
            Categories.ItemInfinities infinities = spicPlayer.GetInfinities(item);
            // LeftClick
            if (spicPlayer.InItemCheck) {
                // Consumed by other item
                if (item != player.HeldItem) {
                    if (detected.DetectMissing && item.GetCategories().Placeable == Categories.Placeable.None)
                        Configs.CategoryDetection.Instance.DetectedPlaceable(item, Categories.Placeable.Block);

                    return !(settings.InfinitePlaceables && infinities.Placeable > 0);
                }

                spicPlayer.TryDetectCategory();
            }

            else {
                // RightClick
                if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease){
                    categories = item.GetCategories();

                    if (!categories.GrabBag.HasValue) {
                        if (categories.Consumable == Categories.Consumable.Tool)
                            return !(settings.InfiniteConsumables && 1 <= infinities.Consumable);


                        if (detected.DetectMissing)
                            Configs.CategoryDetection.Instance.DetectedGrabBag(item);
                    }
                    return !(settings.InfiniteGrabBags && 1 <= infinities.GrabBag);
                    
                }

                // Hotkey
                // ? Hotkeys detect buff

            }

            // LeftClick
            categories = item.GetCategories();
            if(categories.Consumable != Categories.Consumable.None)
                return !(settings.InfiniteConsumables && 1 <= infinities.Consumable);
            if(item.Placeable())
                return !(settings.InfinitePlaceables && 1 <= infinities.Placeable);
            return !(settings.InfiniteGrabBags && 1 <= infinities.GrabBag);
        }

        public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
            => !(Configs.Requirements.Instance.InfiniteConsumables && 1 <= player.GetModPlayer<SpicPlayer>().GetInfinities(ammo).Ammo);

        public override bool? CanConsumeBait(Player player, Item bait) 
            => !(Configs.Requirements.Instance.InfiniteConsumables && 1 <= player.GetModPlayer<SpicPlayer>().GetInfinities(bait).Consumable) ?
                null : false;

        private void Hook_ItemSetDefaults(ILContext il) {
            SetDefaultsHook = false;
            System.Type[] args = { typeof(Item), typeof(bool) };
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

        public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
            SpicPlayer spicPlayer = Main.player[Main.myPlayer].GetModPlayer<SpicPlayer>();
            if (reforgePrice > spicPlayer.GetCurrencyInfinity(-1)) return false;
            reforgePrice = 0;
            return true;
        }
    }
}
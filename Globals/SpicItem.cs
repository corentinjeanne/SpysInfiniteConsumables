using System.Collections.Generic;

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace SPIC.Globals {
    
    public class SpicItem : GlobalItem {

        private static int[] s_itemMaxStack;
        public static int MaxStack(int type) => s_itemMaxStack[type];
        public static bool SetDefaultsHook { get; private set; }


        public override void Load() {
            s_itemMaxStack = new int[ItemID.Count];
            IL.Terraria.Item.SetDefaults_int_bool += Hook_ItemSetDefaults;
            CategoryHelper.ClearAll();
        }
        public override void Unload() {
            SetDefaultsHook = false;
            s_itemMaxStack = null;
            CategoryHelper.ClearAll();
        }

        public override void SetDefaults(Item item) {
            if (item.tileWand != -1) PlaceableExtension.RegisterWandAmmo(item);
            if (item.FitsAmmoSlot() && item.mech) PlaceableExtension.RegisterWandAmmo(item.type, Categories.Placeable.Wiring);
        }

        public override void SetStaticDefaults() {
            if (!SetDefaultsHook){
                for (int type = 0; type < ItemID.Count; type++){
                    Item item = new(type);
                    s_itemMaxStack[type] = item.maxStack != 0 ? item.maxStack : 1;
                }
            }
            
            System.Array.Resize(ref s_itemMaxStack, ItemLoader.ItemCount);
            for (int type = ItemID.Count; type < ItemLoader.ItemCount; type++) {
                ModItem modItem = ItemLoader.GetItem(type);
                s_itemMaxStack[type] = modItem.Item.maxStack != 0 ? modItem.Item.maxStack : 1;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            if(!item.HasAnInfinity() && item.useAmmo <= AmmoID.None && item.tileWand == -1) return;
            bool noInfinities = !item.CanDisplayInfinities(true);
            Configs.Requirements settings = Configs.Requirements.Instance;
            Configs.InfinityDisplay visuals = Configs.InfinityDisplay.Instance;

            Player player = Main.LocalPlayer;
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
            TooltipLine material = AddedLine("Material", Lang.tip[36].Value);
            TooltipLine currency = AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.Categories.Currency.name"));
            
            void ModifyLine(TooltipLine toEdit, string missing, bool displayCategory, string categoryKey, int requirement, bool infinite, Microsoft.Xna.Framework.Color color, params KeyValuePair<int, long>[] infinities){ // category, req, inf                
                TooltipLine line = null;
                TooltipLine Line() => line ??= tooltips.FindorAddLine(toEdit, missing);
                if(!visuals.toopltip_ShowMissingLines && tooltips.FindLine(toEdit.Name) == null) return;
                infinite &= !noInfinities;
                if(visuals.toopltip_ShowInfinities && infinite) {
                    Line().OverrideColor = color;
                    if(infinities.Length == 0){
                        line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line.Text);
                    }else {
                        string items = "";
                        for (int i = 0; i < infinities.Length; i++) {
                            if(i != infinities.Length-1) line.Text += ' ';

                            items += visuals.tooltip_UseItemName ? Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteName", Lang.GetItemNameValue(infinities[i].Key), infinities[i].Value) :
                                Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteSprite", infinities[i].Key, infinities[i].Value);
                        }
                        line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.infiniteFull", line.Text, items);
                    }
                }

                int total = 0;
                string Separator() => total++ == 0 ? " (" : ", ";
                
                if(visuals.toopltip_ShowCategories && displayCategory) Line().Text += Separator() + Language.GetTextValue(categoryKey);
                if(visuals.toopltip_ShowRequirement && requirement != 0 && !infinite) Line().Text += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.Requirement", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.RequirementStacks", -requirement));
                if(total > 0) line.Text += ")";
            }

            if (settings.InfinitePlaceables) {
                ModifyLine(item.Placeable() || !PlaceableExtension.IsWandAmmo(item.type) ? placeable : wand, "Placeable",
                    categories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{categories.Placeable}",
                    requirements.Placeable, infinities.Placeable > -1, visuals.color_Placeables
                );
                if (settings.InfinitePlaceables) {
                    if (item.tileWand != -1) {
                        Item wandItem = System.Array.Find(player.inventory, i => i.type == item.tileWand);
                        if (wandItem is not null) {
                            Categories.ItemCategories wandCategories = wandItem.GetCategories();
                            ModifyLine(AddedLine("WandConsumes", null), "Placeable",
                                wandCategories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{wandCategories.Placeable}",
                                wandItem.GetRequirements().Placeable, spicPlayer.GetInfinities(wandItem).Placeable > -1, visuals.color_Placeables
                            );
                        }
                    }
                }
            }

            if (settings.InfiniteConsumables) {
                ModifyLine(ammo, null,
                    categories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{categories.Ammo}",
                    requirements.Ammo, infinities.Ammo > -1, visuals.color_Ammo
                );
                if (item.useAmmo > AmmoID.None && player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)) {
                    Item ammoItem = System.Array.Find(player.inventory, i => i.type == ammoType);
                    if (ammoItem is not null) {
                        Categories.ItemCategories ammoCategories = ammoItem.GetCategories();
                        ModifyLine(AddedLine("WeaponConsumes", Lang.tip[34].Value + $" [i:{ammoType}]"), "WandConsumes",
                        ammoCategories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{ammoCategories.Ammo}",
                        ammoItem.GetRequirements().Ammo, spicPlayer.GetInfinities(ammoItem).Ammo > -1, visuals.color_Ammo
                    );
                    }
                }
                ModifyLine(consumable, null,
                    categories.Consumable != Categories.Consumable.None, categories.Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{categories.Consumable}" : $"Mods.SPIC.Categories.Unknown",
                    requirements.Consumable, infinities.Consumable > -1, visuals.color_Consumables
                );
            }
            if(settings.InfiniteGrabBags) ModifyLine(bag, "Consumable",
                categories.GrabBag.HasValue && categories.GrabBag != Categories.GrabBag.None, $"Mods.SPIC.Categories.GrabBag.{categories.GrabBag}",
                requirements.GrabBag, infinities.GrabBag > -1, visuals.color_Bags
            );
            if(settings.InfiniteCurrencies) ModifyLine(currency, "Consumable",
                c_category != Categories.Currency.None, $"Mods.SPIC.Categories.Currency.{c_category}",
                c_requirement, c_infinity > -1, visuals.color_Currencies, CurrencyExtension.CurrencyCountToItems(currencyType, c_infinity).ToArray()
            );
            if(settings.InfiniteMaterials) ModifyLine(material, null,
                categories.Material != Categories.Material.None, $"Mods.SPIC.Categories.Material.{categories.Material}",
                requirements.Material, infinities.Material > -1, visuals.color_Materials, new KeyValuePair<int, long>(item.type, infinities.Material)
            );
            if(!noInfinities && spicPlayer.HasFullyInfinite(item)){
                TooltipLine name = tooltips.FindLine("ItemName");
                name.Text = Language.GetTextValue($"Mods.SPIC.ItemTooltip.infinite", name.Text);
            }
        }

        private static int dotTime;
        private static int dot;
        private static readonly int[] pages = new int[(int)Categories.Category.Currency];
        private static readonly int[] glows = new int[(int)Categories.Category.Currency];

        public static void IncrementCounters() {
            Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
            dotTime++;
            if (dotTime < display.glow_PulseTime) return;
            dotTime = 0;
            for (int i = 0; i < glows.Length; i++) glows[i] = (glows[i] + 1) % (i+1);
            dot++;
            if (dot < Configs.InfinityDisplay.Instance.dots_Count) return;
            dot = 0;
            for (int i = 0; i < pages.Length; i++) pages[i] = (pages[i] + 1) % (i+1);
        }
        private static readonly Asset<Texture2D> s_smallDot = ModContent.Request<Texture2D>("SPIC/Textures/small_dot");
        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if (!item.CanDisplayInfinities(false) || (!item.HasAnInfinity() && item.useAmmo <= AmmoID.None && item.tileWand == -1)) return;
            Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
            if(!display.dots_ShowDots && !display.glow_ShowGlow) return;
            Player player = Main.LocalPlayer;
            if(!player.TryGetModPlayer(out SpicPlayer spicPlayer)) return;

            // ? add randomness in the timing
            Configs.Requirements settings = Configs.Requirements.Instance;
            Categories.ItemInfinities infinities = spicPlayer.GetInfinities(item);

            List<Color> activeInfinities = new();
            if(settings.InfinitePlaceables){
                if (item.tileWand != -1) {
                    Item wandItem = System.Array.Find(player.inventory, i => i.type == item.tileWand);
                    if (wandItem is not null && 0 <= spicPlayer.GetInfinities(wandItem).Placeable) activeInfinities.Add(display.color_Placeables);
                }
                if (0 <=infinities.Placeable) activeInfinities.Add(display.color_Placeables);
            }
            if (settings.InfiniteConsumables) {
                if (0 <= infinities.Ammo) activeInfinities.Add(display.color_Ammo);
                if (item.useAmmo > AmmoID.None && player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)) {
                    Item ammoItem = System.Array.Find(player.inventory, i => i.type == ammoType);
                    if (ammoItem is not null && 0 <= spicPlayer.GetInfinities(ammoItem).Ammo) activeInfinities.Add(display.color_Ammo);
                }
                if (0 <= infinities.Consumable) activeInfinities.Add(display.color_Consumables);
            }
            if(settings.InfiniteGrabBags && 0 <= infinities.GrabBag) activeInfinities.Add(display.color_Bags);
            if(settings.InfiniteCurrencies && 0 <= spicPlayer.GetCurrencyInfinity(item.CurrencyType())) activeInfinities.Add(display.color_Currencies);
            if(settings.InfiniteMaterials && 0 <= infinities.Material) activeInfinities.Add(display.color_Materials);
            if (activeInfinities.Count == 0) return;

            activeInfinities.Reverse();
            if (display.glow_ShowGlow) {
                float progress = 1 - System.MathF.Abs(display.glow_PulseTime/2 - dotTime % display.glow_PulseTime) / display.glow_PulseTime * 2;
                float maxScale = (frame.Size().X + 4) / frame.Size().X - 1; // 0.2f
                spriteBatch.Draw(TextureAssets.Item[item.type].Value, position + frame.Size()/2f * scale, frame, activeInfinities[glows[activeInfinities.Count - 1]] * display.glow_Intensity * progress, 0, origin + frame.Size()/2f, scale * (progress * maxScale + 1f), SpriteEffects.None, 0f);
            }
            if (display.dots_ShowDots) {
                Vector2 dotFrameSize = TextureAssets.InventoryBack.Value.Size() * Main.inventoryScale;
                Vector2 pos = position + frame.Size() / 2f * scale - dotFrameSize / 2 + Vector2.One;
                dotFrameSize -= s_smallDot.Size() * Main.inventoryScale;
                Vector2 start = display.dots_Start;
                Vector2 diff = display.dots_Count == 1 ? Vector2.Zero : (display.dots_Start - display.dots_End) / (display.dots_Count - 1);
                int dotOffset = pages[activeInfinities.Count / (display.dots_Count + 1)] * display.dots_Count;
                for (int i = 0; i + dotOffset < activeInfinities.Count && i < display.dots_Count; i++)
                    spriteBatch.Draw(s_smallDot.Value, pos + dotFrameSize * (start - diff * i), null, activeInfinities[i + dotOffset], 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0);
            }
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
                if (item.type < ItemID.Count) s_itemMaxStack[item.type] = item.maxStack;
            });

            SetDefaultsHook = true;
        }

        public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
            SpicPlayer spicPlayer = Main.LocalPlayer.GetModPlayer<SpicPlayer>();
            if (reforgePrice > spicPlayer.GetCurrencyInfinity(-1)) return false;
            reforgePrice = 0;
            return true;
        }
    }
}
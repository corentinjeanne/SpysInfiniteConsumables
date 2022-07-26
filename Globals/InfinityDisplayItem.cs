using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace SPIC.Globals;

public class InfinityDisplayItem : GlobalItem {

    // TODO only display material infininity on mat for selected recipe (& if inf mat)
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!item.HasAnInfinity() && item.useAmmo <= AmmoID.None && item.tileWand == -1) return;
        bool noInfinities = !item.CanDisplayInfinities(true);
        Configs.Requirements settings = Configs.Requirements.Instance;
        Configs.InfinityDisplay visuals = Configs.InfinityDisplay.Instance;

        Player player = Main.LocalPlayer;
        InfinityPlayer spicPlayer = player.GetModPlayer<InfinityPlayer>();
        Categories.TypeCategories typeCategories = item.GetTypeCategories();
        Categories.TypeRequirements typeRequirements = item.GetTypeRequirements();
        Categories.TypeInfinities typeInfinities = spicPlayer.GetTypeInfinities(item);

        int currencyType = item.CurrencyType();
        Categories.Currency currencyCategories = CategoryManager.GetCurrencyCategory(currencyType);
        int currencyRequirements = CategoryManager.GetCurrencyRequirement(currencyType);
        long currencyInfinities = spicPlayer.GetCurrencyInfinity(currencyType);

        static TooltipLine AddedLine(string name, string value) => new(SpysInfiniteConsumables.Instance, name, value) {
            OverrideColor = new(150, 150, 150)
        };
        TooltipLine consumable = AddedLine("Consumable", Lang.tip[35].Value);
        TooltipLine placeable = AddedLine("Placeable", Lang.tip[33].Value);
        TooltipLine wand = AddedLine("Placeable", Language.GetTextValue("Mods.SPIC.ItemTooltip.wandAmmo"));
        TooltipLine ammo = AddedLine("Ammo", Lang.tip[34].Value);
        TooltipLine bag = AddedLine("GrabBag", Language.GetTextValue("Mods.SPIC.Categories.GrabBag.name"));
        TooltipLine material = AddedLine("Material", Lang.tip[36].Value);
        TooltipLine currency = AddedLine("Currencycat", Language.GetTextValue("Mods.SPIC.Categories.Currency.name"));

        void ModifyLine(TooltipLine toEdit, string missing, bool displayCategory, string categoryKey, int requirement, bool infinite, Microsoft.Xna.Framework.Color color, params KeyValuePair<int, long>[] infinities) { // category, req, inf                
            TooltipLine line = null;
            TooltipLine Line() => line ??= tooltips.FindorAddLine(toEdit, missing);
            if (!visuals.toopltip_ShowMissingLines && tooltips.FindLine(toEdit.Name) == null) return;
            infinite &= !noInfinities;
            if (visuals.toopltip_ShowInfinities && infinite) {
                Line().OverrideColor = color;
                if (infinities.Length == 0) {
                    line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line.Text);
                } else {
                    string items = "";
                    for (int i = 0; i < infinities.Length; i++) {
                        if (i != infinities.Length - 1) line.Text += ' ';
                        items += visuals.tooltip_UseItemName ? Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteName", Lang.GetItemNameValue(infinities[i].Key), infinities[i].Value) :
                            Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteSprite", infinities[i].Key, infinities[i].Value);
                    }
                    line.Text = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite", line.Text, items);
                }
            }

            int total = 0;
            string Separator() => total++ == 0 ? " (" : ", ";

            if (visuals.toopltip_ShowCategories && displayCategory) Line().Text += Separator() + Language.GetTextValue(categoryKey);
            if (visuals.toopltip_ShowRequirement && requirement != 0 && !infinite) Line().Text += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.Requirement", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.RequirementStacks", -requirement));
            if (total > 0) line.Text += ")";
        }

        if (!noInfinities && spicPlayer.HasFullyInfinite(item)) {
            TooltipLine name = tooltips.FindLine("ItemName");
            name.Text = Language.GetTextValue($"Mods.SPIC.ItemTooltip.infinite", name.Text);
        }

        if (settings.InfinitePlaceables) {
            ModifyLine(item.Placeable() || !PlaceableExtension.IsWandAmmo(item.type) ? placeable : wand, "Placeable",
                typeCategories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{typeCategories.Placeable}",
                typeRequirements.Placeable, typeInfinities.Placeable > -1, visuals.color_Placeables
            );
            if (item.tileWand != -1) {
                Item wandItem = System.Array.Find(player.inventory, i => i.type == item.tileWand);
                if (wandItem is not null) {
                    Categories.TypeCategories wandCategories = wandItem.GetTypeCategories();
                    ModifyLine(AddedLine("WandConsumes", null), "Placeable",
                        wandCategories.Placeable != Categories.Placeable.None, $"Mods.SPIC.Categories.Placeable.{wandCategories.Placeable}",
                        wandItem.GetTypeRequirements().Placeable, spicPlayer.GetTypeInfinities(wandItem).Placeable > -1, visuals.color_Placeables
                    );
                }
            }
        }

        if (settings.InfiniteConsumables) {
            ModifyLine(ammo, null,
                typeCategories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{typeCategories.Ammo}",
                typeRequirements.Ammo, typeInfinities.Ammo > -1, visuals.color_Ammo
            );
            if (item.useAmmo > AmmoID.None) {
                if (player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)) {
                    Item ammoItem = System.Array.Find(player.inventory, i => i.type == ammoType);
                    Categories.TypeCategories ammoCategories = ammoItem.GetTypeCategories();
                    ModifyLine(AddedLine("WeaponConsumes", Language.GetTextValue($"Mods.SPIC.ItemTooltip.weaponAmmo", ammoItem.Name)), "WandConsumes",
                        ammoCategories.Ammo != Categories.Ammo.None, $"Mods.SPIC.Categories.Ammo.{ammoCategories.Ammo}",
                        ammoItem.GetTypeRequirements().Ammo, spicPlayer.GetTypeInfinities(ammoItem).Ammo > -1, visuals.color_Ammo
                    );
                }else {
                    tooltips.AddLine("WandConsumes", AddedLine("WeaponConsumes", Language.GetTextValue($"Mods.SPIC.ItemTooltip.noAmmo")));
                }
            }
            ModifyLine(consumable, null,
                typeCategories.Consumable != Categories.Consumable.None, typeCategories.Consumable.HasValue ? $"Mods.SPIC.Categories.Consumable.{typeCategories.Consumable}" : $"Mods.SPIC.Categories.Unknown",
                typeRequirements.Consumable, typeInfinities.Consumable > -1, visuals.color_Consumables
            );
        }
        if (settings.InfiniteGrabBags) ModifyLine(bag, "Consumable",
            typeCategories.GrabBag.HasValue && typeCategories.GrabBag != Categories.GrabBag.None, $"Mods.SPIC.Categories.GrabBag.{typeCategories.GrabBag}",
            typeRequirements.GrabBag, typeInfinities.GrabBag > -1, visuals.color_Bags
        );
        if (settings.InfiniteCurrencies) {
            KeyValuePair<int, long>[] c = spicPlayer.HasFullyInfiniteCurrency(currencyType) ? System.Array.Empty<KeyValuePair<int, long>>() : CurrencyExtension.CurrencyCountToItems(item.CurrencyType(), currencyInfinities).ToArray();
            ModifyLine(currency, "Consumable",
                currencyCategories != Categories.Currency.None, $"Mods.SPIC.Categories.Currency.{currencyCategories}",
                currencyRequirements, currencyInfinities > -1, visuals.color_Currencies, c
            );
        }
        if (settings.InfiniteMaterials) {
            KeyValuePair<int, long>[] p = spicPlayer.HasFullyInfiniteMaterial(item) ? System.Array.Empty<KeyValuePair<int, long>>() : new[] { new KeyValuePair<int, long>(item.type, typeInfinities.Material) };
            ModifyLine(material, null,
                typeCategories.Material != Categories.Material.None, $"Mods.SPIC.Categories.Material.{typeCategories.Material}",
                typeRequirements.Material, typeInfinities.Material > -1, visuals.color_Materials, p
            );
        }
    }

    private static int dotFrame;
    private static int dot;
    private static readonly int[] pages = new int[CategoryManager.CategoryCount];
    private static readonly int[] glows = new int[CategoryManager.CategoryCount];
    private static readonly Asset<Texture2D> s_smallDot = ModContent.Request<Texture2D>("SPIC/Textures/Small_Dot");

    public static void IncrementCounters() {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        dotFrame++;
        if (dotFrame < display.glow_PulseTime) return;
        dotFrame = 0;
        for (int i = 0; i < glows.Length; i++) glows[i] = (glows[i] + 1) % (i + 1);
        dot++;
        if (dot < Configs.InfinityDisplay.Instance.dots_Count) return;
        dot = 0;
        for (int i = 0; i < pages.Length; i++) pages[i] = (pages[i] + 1) % (i + 1);
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!item.CanDisplayInfinities(false) || (!item.HasAnInfinity() && item.useAmmo <= AmmoID.None && item.tileWand == -1)) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        if (!display.dots_ShowDots && !display.glow_ShowGlow) return;
        Player player = Main.LocalPlayer;
        if (!player.TryGetModPlayer(out InfinityPlayer spicPlayer)) return;

        // ? add randomness in the timing
        Configs.Requirements settings = Configs.Requirements.Instance;
        Categories.TypeInfinities infinities = spicPlayer.GetTypeInfinities(item);

        List<Color> activeInfinities = new();
        if (settings.InfinitePlaceables) {
            if (item.tileWand != -1) {
                Item wandItem = System.Array.Find(player.inventory, i => i.type == item.tileWand);
                if (wandItem is not null && 0 <= spicPlayer.GetTypeInfinities(wandItem).Placeable) activeInfinities.Add(display.color_Placeables);
            }
            if (0 <= infinities.Placeable) activeInfinities.Add(display.color_Placeables);
        }
        if (settings.InfiniteConsumables) {
            if (0 <= infinities.Ammo) activeInfinities.Add(display.color_Ammo);
            if (item.useAmmo > AmmoID.None && player.PickAmmo(item, out int _, out _, out _, out _, out int ammoType, true)) {
                Item ammoItem = System.Array.Find(player.inventory, i => i.type == ammoType);
                if (ammoItem is not null && 0 <= spicPlayer.GetTypeInfinities(ammoItem).Ammo) activeInfinities.Add(display.color_Ammo);
            }
            if (0 <= infinities.Consumable) activeInfinities.Add(display.color_Consumables);
        }
        if (settings.InfiniteGrabBags && 0 <= infinities.GrabBag) activeInfinities.Add(display.color_Bags);
        if (settings.InfiniteCurrencies && 0 <= spicPlayer.GetCurrencyInfinity(item.CurrencyType())) activeInfinities.Add(display.color_Currencies * (spicPlayer.HasFullyInfiniteCurrency(item.CurrencyType()) ? 1 : 0.5f));
        if (settings.InfiniteMaterials && 0 <= infinities.Material) activeInfinities.Add(display.color_Materials * (spicPlayer.HasFullyInfiniteMaterial(item) ? 1 : 0.5f));
        if (activeInfinities.Count == 0) return;

        activeInfinities.Reverse();
        if (display.glow_ShowGlow) {
            float progress = 1 - System.MathF.Abs(display.glow_PulseTime / 2 - dotFrame % display.glow_PulseTime) / display.glow_PulseTime * 2;
            float maxScale = (frame.Size().X + 4) / frame.Size().X - 1; // 0.2f
            spriteBatch.Draw(TextureAssets.Item[item.type].Value, position + frame.Size() / 2f * scale, frame, activeInfinities[glows[activeInfinities.Count - 1]] * display.glow_Intensity * progress, 0, origin + frame.Size() / 2f, scale * (progress * maxScale + 1f), SpriteEffects.None, 0f);
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
}

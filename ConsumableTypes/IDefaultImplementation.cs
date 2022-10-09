using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace SPIC.ConsumableTypes;

public interface IDefaultInfinity : IConsumableType {
    [NoJIT] int IConsumableType.GetRequirement(Item item) => GetRequirement(item);
    [NoJIT] long IConsumableType.GetInfinity(Player player, Item item) => GetInfinity(item, CountItems(player, item));
    
    long CountItems(Player player, Item item) => player.CountItems(item.type, true);
    int MaxStack(Category category);
    int Requirement(Category category);

    new int GetRequirement(Item item) => Requirement(item.GetCategory(UID));
    long GetInfinity(Item item, long count) => InfinityManager.CalculateInfinity(
        (int)System.MathF.Min(item.maxStack, MaxStack(item.GetCategory(UID))),
        count,
        item.GetRequirement(UID),
        1
    );
}

// ? remove
public interface IDefaultInfinity<TCategory> : IDefaultInfinity, IConsumableType<TCategory>, IConsumableType where TCategory : System.Enum {
    [NoJIT] int IDefaultInfinity.MaxStack(Category category) => MaxStack((TCategory)category);
    [NoJIT] int IDefaultInfinity.Requirement(Category category) => Requirement((TCategory)category);

    int MaxStack(TCategory category);
    int Requirement(TCategory category);
}


public enum DisplayFlags {
    Category = 0b0001,
    Requirement = 0b0010,
    Infinity = 0b0100,
    All = Category | Requirement | Infinity
}

public static class DefaultImplementation {

    public static DisplayFlags GetFlags(Category category, int requirement, long infinity, long maxInfinity) {
        DisplayFlags flags = 0;
        if (requirement != IConsumableType.NoRequirement || maxInfinity <= infinity)
            flags |= DisplayFlags.Category | DisplayFlags.Requirement;
        if (infinity > IConsumableType.NotInfinite) flags |= DisplayFlags.Infinity;
        return flags;
    }
    public static DisplayFlags GetInfinityDisplayFlags(Item item, bool isACopy) {
        Player player = Main.LocalPlayer;
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;

        if (item.playerIndexTheItemIsReservedFor != Main.myPlayer) return DisplayFlags.All & ~DisplayFlags.Infinity;
        if (System.Array.Find(player.armor, i => AreSameItems(i, item)) is not null
                || System.Array.Find(player.miscEquips, i => AreSameItems(i, item)) is not null)
            return 0;

        if (AreSameItems(Main.mouseItem, item)
                || System.Array.Find(player.inventory, i => AreSameItems(i, item)) is not null
                || (player.InChest(out var chest) && System.Array.Find(chest, i => AreSameItems(i, item)) is not null)
                || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item)))
            return DisplayFlags.All;

        return DisplayFlags.All & ~DisplayFlags.Infinity;
    }
    
    public static void ModifyTooltip(IDefaultDisplay type, List<TooltipLine> tooltips, TooltipLine affectedLine, string position, Item item, Item values) {
        Category category = values.GetCategory(type.UID);
        int requirement = values.GetRequirement(type.UID);
        long infinity = Main.LocalPlayer.GetInfinity(values, type.UID);
        long maxInfinity = type.GetMaxInfinity(Main.LocalPlayer, values);

        DisplayFlags displayFlags = GetFlags(category, requirement, infinity, maxInfinity) & type.GetInfinityDisplayFlags(values, true) & type.GetInfinityDisplayFlags(item, true);
        if ((displayFlags & OnLineDisplayFlags) == 0) return;

        TooltipLine line = tooltips.FindorAddLine(affectedLine, position, out bool addedLine);
        DisplayOnLine(line, type.Color, displayFlags, values, category.ToString(), requirement, infinity, maxInfinity);
        if (addedLine) line.OverrideColor *= 0.75f;
    }
    public static DisplayFlags OnLineDisplayFlags => DisplayFlags.All;
    public static void DisplayOnLine(TooltipLine line, Color color, DisplayFlags displayFlags, Item values, string category, int requirement, long infinity, long maxInfinity) => DisplayOnLine(ref line.Text, ref line.OverrideColor, color, displayFlags, values, category, requirement, infinity, maxInfinity);
    public static void DisplayOnLine(ref string line, ref Color? lineColor, Color color, DisplayFlags displayFlags, Item values, string category, int requirement, long infinity, long maxInfinity) {
        Configs.InfinityDisplay visuals = Configs.InfinityDisplay.Instance;

        if (displayFlags.HasFlag(DisplayFlags.Infinity)) {
            lineColor = color;
            if (maxInfinity <= infinity) line = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line);
            else line = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite",
                    line,
                    visuals.tooltip_UseItemName ? Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteName", values.Name, infinity) : Language.GetTextValue("Mods.SPIC.ItemTooltip.InfiniteSprite", values.type, infinity)
                );
        }

        int total = 0;
        string Separator() => total++ == 0 ? " (" : ", ";

        if (displayFlags.HasFlag(DisplayFlags.Category)) line += Separator() + category;

        if (displayFlags.HasFlag(DisplayFlags.Requirement)){
            if(!displayFlags.HasFlag(DisplayFlags.Infinity))
                line += Separator() + (requirement > 0 ? Language.GetTextValue("Mods.SPIC.ItemTooltip.items", requirement) : Language.GetTextValue("Mods.SPIC.ItemTooltip.stacks", -requirement));
        }
        if (total > 0) line += ")";
    }

    public static readonly Asset<Texture2D> SmallDot = ModContent.Request<Texture2D>("SPIC/Textures/Small_Dot");
    public static readonly Asset<Texture2D> TinyDot = ModContent.Request<Texture2D>("SPIC/Textures/Tiny_Dot");
    public static void DrawInInventorySlot(IDefaultDisplay type, Item item, Item values) {
        long infinity = Main.LocalPlayer.GetInfinity(values, type.UID);
        long maxInfinity = type.GetMaxInfinity(Main.LocalPlayer, values);

        DisplayFlags displayFlags = GetFlags(Category.None, IConsumableType.NoRequirement, infinity, maxInfinity) & type.GetInfinityDisplayFlags(values, false) & type.GetInfinityDisplayFlags(item, false);
        if ((displayFlags & DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayDot.Add(new(type.Color, displayFlags, infinity, maxInfinity));
    }
    public static DisplayFlags DotsDisplayFlags => DisplayFlags.Infinity;
    public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, Color color, DisplayFlags displayFlags, long infinity, long maxInfinity) {
        if (displayFlags.HasFlag(DisplayFlags.Infinity)) {
            spriteBatch.Draw(
                maxInfinity <= infinity ? SmallDot.Value : TinyDot.Value,
                position,
                null,
                color,
                0f,
                Vector2.Zero,
                Main.inventoryScale,
                SpriteEffects.None,
                0
            );
        }
    }

    public static void DrawOnItemSprite(IDefaultDisplay type, Item item, Item values) {

        long infinity = Main.LocalPlayer.GetInfinity(values, type.UID);
        long maxInfinity = type.GetMaxInfinity(Main.LocalPlayer, values);

        DisplayFlags displayFlags = GetFlags(Category.None, IConsumableType.NoRequirement, infinity, maxInfinity) & type.GetInfinityDisplayFlags(values, false) & type.GetInfinityDisplayFlags(item, false);
        if ((displayFlags & GlowDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayGlow.Add(new(type.Color, displayFlags, infinity, maxInfinity));
    }
    public static DisplayFlags GlowDisplayFlags => DisplayFlags.Infinity;
    public static void DisplayGlow(SpriteBatch spriteBatch, Item item, Vector2 position, Vector2 origin, Rectangle frame, float scale, Color color, float ratio, DisplayFlags displayFlags, long infinity, long maxInfinity) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;

        float increase = (ratio < 0.5f ? ratio : 1 - ratio)*2;
        float maxScale = maxInfinity <= infinity ? ((frame.Size().X + 4) / frame.Size().X - 1) : 0;
        spriteBatch.Draw(
            TextureAssets.Item[item.type].Value,
            position - frame.Size() / 2 * increase * maxScale * scale * Main.inventoryScale,
            frame,
            color * display.glow_Intensity * increase,
            0,
            origin,
            scale * (increase * maxScale + 1f),
            SpriteEffects.None,
            0f
        );
    }
}

public interface IDefaultDisplay : IConsumableType{
    [NoJIT] void IConsumableType.ModifyTooltip(Item item, List<TooltipLine> tooltips) => DefaultImplementation.ModifyTooltip(this, tooltips, TooltipLine, LinePosition, item, item);
    [NoJIT] void IConsumableType.DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => DefaultImplementation.DrawOnItemSprite(this, item, item);
    [NoJIT] void IConsumableType.DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => DefaultImplementation.DrawInInventorySlot(this, item, item);

    DisplayFlags GetInfinityDisplayFlags(Item item, bool isACopy) => DefaultImplementation.GetInfinityDisplayFlags(item, isACopy);

    long GetMaxInfinity(Player player, Item item) => NotInfinite;

    string LinePosition => TooltipLine.Name;
    TooltipLine TooltipLine { get; }
}

public interface IDefaultAmmunition : IDefaultDisplay, IAmmunition{
    [NoJIT] void IAmmunition.ModifyTooltip(Item weapon, Item ammo, List<TooltipLine> tooltips) => DefaultImplementation.ModifyTooltip(this, tooltips, WeaponTooltipLine(ammo), WeaponLinePosition, weapon, ammo);
    [NoJIT] void IAmmunition.DrawOnItemSprite(Item weapon, Item ammo, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => DefaultImplementation.DrawOnItemSprite(this, weapon, ammo);
    [NoJIT] void IAmmunition.DrawInInventorySlot(Item weapon, Item ammo, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => DefaultImplementation.DrawInInventorySlot(this, weapon, ammo);

    string WeaponLinePosition => "WandConsumes";
    TooltipLine WeaponTooltipLine(Item ammo) => new(Mod, $"{Name}Consumes", Language.GetTextValue("Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name));
}
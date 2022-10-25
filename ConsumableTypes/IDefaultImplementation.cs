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
    [NoJIT] IRequirement IConsumableType.GetRequirement(Item item) => GetRequirement(item);
    [NoJIT] long IConsumableType.CountItems(Player player, Item item) => CountItems(player, item);

    IRequirement Requirement(Category category);

    new IRequirement GetRequirement(Item item) => Requirement(item.GetCategory(UID));
    new long CountItems(Player player, Item item) => player.CountItems(item.type, true);
}

public interface IDefaultInfinity<TCategory> : IDefaultInfinity, IConsumableType<TCategory>, IConsumableType where TCategory : System.Enum {
    [NoJIT] IRequirement IDefaultInfinity.Requirement(Category category) => Requirement((TCategory)category);

    IRequirement Requirement(TCategory category);

}


public enum DisplayFlags {
    Category = 0b0001,
    Requirement = 0b0010,
    Infinity = 0b0100,
    All = Category | Requirement | Infinity
}

public static class DefaultImplementation {

    public static DisplayFlags GetDisplayFlags(Category category, Infinity infinity, ItemCount next) {
        DisplayFlags flags = 0;
        if (!category.IsNone) flags |= DisplayFlags.Category;
        if (!next.IsNone) flags |= DisplayFlags.Requirement;
        if (infinity.Value != ItemCount.None) flags |= DisplayFlags.Infinity;
        return flags;
    }

    public static bool OwnsItem(Player player, Item item, bool isACopy) {
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;

        if (item.playerIndexTheItemIsReservedFor != Main.myPlayer) return false;
        if (System.Array.Find(player.armor, i => AreSameItems(i, item)) is not null
                || System.Array.Find(player.miscEquips, i => AreSameItems(i, item)) is not null)
            return false;

        if (AreSameItems(Main.mouseItem, item)
                || System.Array.Find(player.inventory, i => AreSameItems(i, item)) is not null
                || (player.InChest(out var chest) && System.Array.Find(chest, i => AreSameItems(i, item)) is not null)
                || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item)))
            return true;

        return false;
    }
    
    public static void ModifyTooltip(IDefaultDisplay type, List<TooltipLine> tooltips, TooltipLine affectedLine, string position, Item item, Item values) {
        Player player = Main.LocalPlayer;
        Category category = values.GetCategory(type.UID);
        IRequirement requirement = values.GetRequirement(type.UID);

        // ItemCount effective;
        Infinity infinity;
        ItemCount itemCount;

        if (type.OwnsItem(player, item, true) && type.OwnsItem(player, values, true)) {
            // effective = player.GetEffectiveRequirement(values, type.UID);
            infinity = InfinityManager.GetInfinity(player, values, type.UID);
            itemCount = new(type.CountItems(player, values), values.maxStack);
        }else {
            // effective = ItemCount.None;
            infinity = Infinity.None;
            itemCount = ItemCount.None;
        }

        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < type.GetMaxInfinity(player, item) ?
            requirement.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        DisplayFlags displayFlags = GetDisplayFlags(category, infinity, next) & type.DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & OnLineDisplayFlags) == 0) return;

        TooltipLine line = tooltips.FindorAddLine(affectedLine, position, out bool addedLine);
        DisplayOnLine(ref line.Text, ref line.OverrideColor, type.Color, displayFlags, category, infinity, next, itemCount);
        if (addedLine) line.OverrideColor *= 0.75f;
    }
    public static DisplayFlags OnLineDisplayFlags => DisplayFlags.All;
    public static void DisplayOnLine(ref string line, ref Color? lineColor, Color color, DisplayFlags displayFlags, Category category, Infinity infinity, ItemCount nextrequirement, ItemCount itemCount) {

        if (displayFlags.HasFlag(DisplayFlags.Infinity)) {
            lineColor = color;
            if (nextrequirement.IsNone) line = Language.GetTextValue("Mods.SPIC.ItemTooltip.infinite", line);
        else line = Language.GetTextValue("Mods.SPIC.ItemTooltip.partialyInfinite",line,
                Language.GetTextValue("Mods.SPIC.ItemTooltip.items",infinity.Value.IsNone)
            );
        }

        int total = 0;
        string Separator() => total++ == 0 ? " (" : ", ";

        if (displayFlags.HasFlag(DisplayFlags.Category) && !displayFlags.HasFlag(DisplayFlags.Infinity)) line += Separator() + category.Label();

        if (displayFlags.HasFlag(DisplayFlags.Requirement)){
            line += Separator() + (itemCount != ItemCount.None ?
                Language.GetTextValue("Mods.SPIC.ItemTooltip.items", $"{itemCount.Items}/{nextrequirement.Items}"):
                Language.GetTextValue("Mods.SPIC.ItemTooltip.items", nextrequirement.Items));
        }
        if (total > 0) line += ")";
    }

    public static readonly Asset<Texture2D> SmallDot = ModContent.Request<Texture2D>("SPIC/Textures/Small_Dot");
    public static readonly Asset<Texture2D> TinyDot = ModContent.Request<Texture2D>("SPIC/Textures/Tiny_Dot");
    public static void DrawInInventorySlot(IDefaultDisplay type, Item item, Item values) {
        Player player = Main.LocalPlayer;
        IRequirement root = values.GetRequirement(type.UID);

        // ItemCount effective;
        Infinity infinity;
        ItemCount itemCount;

        if (type.OwnsItem(player, item, true) && type.OwnsItem(player, values, true)) {
            // effective = player.GetEffectiveRequirement(values, type.UID);
            infinity = InfinityManager.GetInfinity(player, values, type.UID);
            itemCount = new(type.CountItems(player, values), values.maxStack);
        } else {
            // effective = ItemCount.None;
            infinity = Infinity.None;
            itemCount = ItemCount.None;
        }
        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < type.GetMaxInfinity(player, item) ?
            root.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        DisplayFlags displayFlags = GetDisplayFlags(Category.None, infinity, next) & type.DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & DotsDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayDot.Add(new(type.Color, displayFlags, infinity, next));
    }
    public static DisplayFlags DotsDisplayFlags => DisplayFlags.Infinity;
    public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, Color color, DisplayFlags displayFlags, Infinity infinity, ItemCount next) {
        if (displayFlags.HasFlag(DisplayFlags.Infinity)) {
            if(infinity.Value.IsNone) return;
            spriteBatch.Draw(
                next.IsNone ? SmallDot.Value : TinyDot.Value,
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
        Player player = Main.LocalPlayer;
        IRequirement root = values.GetRequirement(type.UID);
        
        // ItemCount effective;
        Infinity infinity;
        ItemCount itemCount;

        if (type.OwnsItem(player, item, true) && type.OwnsItem(player, values, true)) {
            // effective = player.GetEffectiveRequirement(values, type.UID);
            infinity = InfinityManager.GetInfinity(player, values, type.UID);
            itemCount = new(type.CountItems(player, values), item.maxStack);
        } else {
            // effective = ItemCount.None;
            infinity = Infinity.None;
            itemCount = ItemCount.None;
        }
        ItemCount next = infinity.Value.IsNone || infinity.Value.Items < type.GetMaxInfinity(player, item) ?
            root.NextRequirement(infinity.EffectiveRequirement) : ItemCount.None;

        DisplayFlags displayFlags = GetDisplayFlags(Category.None, infinity, next) & type.DisplayFlags & Configs.InfinityDisplay.Instance.DisplayFlags;
        if ((displayFlags & GlowDisplayFlags) != 0)
            Globals.InfinityDisplayItem.s_wouldDisplayGlow.Add(new(type.Color, displayFlags, infinity, next));
    }
    public static DisplayFlags GlowDisplayFlags => DisplayFlags.Infinity;
    public static void DisplayGlow(SpriteBatch spriteBatch, Item item, Vector2 position, Vector2 origin, Rectangle frame, float scale, Color color, float ratio, DisplayFlags displayFlags, Infinity infinity, ItemCount next) {
        if(!displayFlags.HasFlag(DisplayFlags.Infinity)) return;
        if(infinity.Value.IsNone) return;
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;
        float increase = (ratio < 0.5f ? ratio : 1 - ratio)*2;
        float maxScale = next.IsNone ? ((frame.Size().X + 4) / frame.Size().X - 1) : 0;
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

    DisplayFlags DisplayFlags => DisplayFlags.All;
    bool OwnsItem(Player player, Item item, bool isACopy) => DefaultImplementation.OwnsItem(player, item, isACopy);

    long GetMaxInfinity(Player player, Item item) => Infinity.None.Value.Items;

    string LinePosition => TooltipLine.Name;
    TooltipLine TooltipLine { get; }
}

public interface IDefaultAmmunition : IDefaultDisplay, IAmmunition{
    [NoJIT] void IAmmunition.ModifyTooltip(Item weapon, Item ammo, List<TooltipLine> tooltips) => DefaultImplementation.ModifyTooltip(this, tooltips, WeaponTooltipLine(weapon, ammo), WeaponLinePosition, weapon, ammo);
    [NoJIT] void IAmmunition.DrawOnItemSprite(Item weapon, Item ammo, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => DefaultImplementation.DrawOnItemSprite(this, weapon, ammo);
    [NoJIT] void IAmmunition.DrawInInventorySlot(Item weapon, Item ammo, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => DefaultImplementation.DrawInInventorySlot(this, weapon, ammo);

    string WeaponLinePosition => "WandConsumes";
    TooltipLine WeaponTooltipLine(Item weapon, Item ammo) => new(Mod, $"{Name}Consumes", Language.GetTextValue("Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name));
}
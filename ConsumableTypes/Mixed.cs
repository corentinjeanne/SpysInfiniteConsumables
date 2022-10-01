using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableTypes;

public enum MixedCategory : byte {
    AllNone = IConsumableType.NoCategory,
    Mixed
}

// ? add amunition
internal class Mixed : ConsumableType<Mixed>, IConsumableType<MixedCategory> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => Terraria.ID.ItemID.LunarHook;

    public Color Color => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
    public static Color PartialInfinityColor => new(255, (byte)(Main.masterColor * 200f), 0);

    public MixedCategory GetCategory(Item item) {
        foreach (IConsumableType type in item.UsedConsumableTypes()) {
            if (item.GetCategory(type.UID) > IConsumableType.NoCategory) return MixedCategory.Mixed;
        }
        return MixedCategory.AllNone;
    }

    public int GetRequirement(Item item) {
        int mixed = IConsumableType.NoRequirement;
        foreach (IConsumableType type in item.UsedConsumableTypes()) {
            int req = item.GetRequirement(type.UID);
            if (req > mixed) mixed = req;
        }
        return mixed;
    }

    public long GetInfinity(Player player, Item item) {
        long mixed = IConsumableType.NoInfinity;
        foreach (IConsumableType type in item.UsedConsumableTypes()) {
            long inf = player.GetInfinity(item, type.UID);
            if (mixed == IConsumableType.NoInfinity || inf < mixed) mixed = inf;
        }
        return mixed;
    }

    public static long GetMaxInfinity(Player player, Item item) {
        long mixed = IConsumableType.NoInfinity;
        foreach (IDefaultDisplay type in item.UsedConsumableTypes()) {
            long inf = type.GetMaxInfinity(player, item);
            if (inf > mixed) mixed = inf;
        }
        return mixed;
    }

    public static DisplayFlags GetInfinityDisplayFlags(Item item, bool isACopy){
        DisplayFlags flags = DisplayFlags.All;
        foreach (IDefaultDisplay type in item.UsedConsumableTypes()) flags &= type.GetInfinityDisplayFlags(item, isACopy);
        return flags;
    }

    public void ModifyTooltip(Item item, List<TooltipLine> tooltips) {
        Configs.InfinityDisplay display = Configs.InfinityDisplay.Instance;

        byte category = item.GetCategory(UID);
        int requirement = item.GetRequirement(UID);
        long infinity = Main.LocalPlayer.GetInfinity(item, UID);
        long maxInfinity = GetMaxInfinity(Main.LocalPlayer, item);

        DisplayFlags displayFlags = DefaultImplementation.GetFlags(category, requirement, infinity, maxInfinity) & GetInfinityDisplayFlags(item, true) & DisplayFlags.Infinity;
        if ((displayFlags & (DisplayFlags.Infinity)) == 0) return;

        TooltipLine line = tooltips.FindLine("ItemName");
        string text = "";
        Color? color = line.OverrideColor;
        bool names = display.tooltip_UseItemName;
        display.tooltip_UseItemName = true;
        DefaultImplementation.DisplayOnLine(ref text, ref color, maxInfinity <= infinity ? Color : PartialInfinityColor, displayFlags, item, ((IConsumableType<MixedCategory>)this).CategoryLabel(category), requirement, infinity, maxInfinity);
        display.tooltip_UseItemName = names;

        if(color == line.OverrideColor) return;
        text = text.Replace("  ", " ").Trim();
        if(item.stack > 0) line.Text = line.Text.Replace($"({item.stack})", $"[c/{color.Value.Hex3()}:({item.stack}, {text})]");
        else line.Text += $"[c/{color.Value.Hex3()}:({text})]";
    }

    // ? Implement
    public void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
    public void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {}
}
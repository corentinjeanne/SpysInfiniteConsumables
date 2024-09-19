using System.ComponentModel;
using Terraria.ModLoader.Config;
using System;
using SPIC.Configs;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System.Linq;

namespace SPIC.Default.Displays;

public sealed class GlowConfig {
    [DefaultValue(true)] public bool fancyGlow = true;
    [DefaultValue(true)] public bool offset = true;
    [DefaultValue(2f), Range(1f, 5f), Increment(0.1f)] public float animationLength = 2f;
    [DefaultValue(0.75f)] public float intensity = 0.75f;
    [DefaultValue(1.2f), Range(0f, 3f), Increment(0.1f)] public float scale = 1.2f;
}

public sealed class Glow : Display, IConfigProvider<GlowConfig> {
    public static Glow Instance = null!;

    public GlowConfig Config { get; set; } = null!;

    public static void PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale) {
        var displays = InfinityManager.GetDisplayedInfinities(item).SelectMany(displays => displays).Where(display => display.Value.Infinity > 0).ToArray();
        if (displays.Length == 0) return;
        var display = displays[s_index % displays.Length];
        DisplayGlow(spriteBatch, item, position, frame, origin, scale, display.Infinity.Color);
    }

    public static void DisplayGlow(SpriteBatch spriteBatch, Item item, Vector2 position, Rectangle frame, Vector2 origin, float scale, Color color) {
        Texture2D texture = TextureAssets.Item[item.type].Value;
        float offset = item.GetHashCode() % 16 / 16f;

        float progress = (s_nextIndexTime - Main.GlobalTimeWrappedHourly) / Instance.Config.animationLength; // 0>1
        if (Instance.Config.offset) progress = (progress + offset) % 1;
        float distance = (progress <= 0.5f ? progress : (1 - progress)) * 2; // 0>1>0
        color *= Instance.Config.intensity * distance;

        if (!Instance.Config.fancyGlow) {
            float scl = 1 + 8 * distance / frame.Width * Instance.Config.scale;
            spriteBatch.Draw(texture, position, frame, color, 0, origin, scale * scl, 0, 0f);
            return;
        }

        for (float f = 0f; f < 1f; f += 1 / 3f) spriteBatch.Draw(texture, position + new Vector2(0f, 1.5f + 1.5f * distance * Instance.Config.scale).RotatedBy((f * 2 + progress) * Math.PI), frame, color, 0, origin, scale, 0, 0f);
        color *= 0.67f;
        for (float f = 0f; f < 1f; f += 1 / 4f) spriteBatch.Draw(texture, position + new Vector2(0f, 4f * distance * Instance.Config.scale).RotatedBy((f + progress) * -2 * Math.PI), frame, color, 0, origin, scale, 0, 0f);
    }

    public static void PreUpdate() {
        if (Main.GlobalTimeWrappedHourly >= s_nextIndexTime) {
            s_nextIndexTime = (s_nextIndexTime + Instance.Config.animationLength) % 3600;
            s_index = (s_index + 1) % InfinityManager.InfinitiesLCM;
        }
    }

    private static float s_nextIndexTime = 0;
    private static int s_index = 0;

}

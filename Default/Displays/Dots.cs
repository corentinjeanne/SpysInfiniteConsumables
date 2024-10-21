using System.ComponentModel;
using Terraria.ModLoader.Config;
using System;
using SPIC.Configs;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.ModLoader;
using System.Linq;
using SpikysLib.Configs;
using Newtonsoft.Json;
using SPIC.Default.Globals;

namespace SPIC.Default.Displays;

public sealed class DotsConfig {
    [DefaultValue(true)] public bool displayRequirement = true;
    [DefaultValue(Corner.BottomRight)] public Corner start = Corner.BottomRight;
    [DefaultValue(Direction.Horizontal)] public Direction direction = Direction.Horizontal;
    [DefaultValue(5f), Range(1f, 10f), Increment(0.1f)] public float time = 5f;
    [DefaultValue(1f), Range(0f, 3f), Increment(0.1f)] public float scale = 1f;
    [DefaultValue(0.5f)] public float missingOpacity = 0.5f;

    // Compatibility version < v4.0
    [JsonProperty, DefaultValue(5f)] private float AnimationLength { set => ConfigHelper.MoveMember(value != 5f, _ => time = value); }
}

public sealed class Dots : Display, IConfigProvider<DotsConfig> {
    public static Dots Instance = null!;

    public DotsConfig Config { get; set; } = null!;

    public override void Load() {
        _dotsTexture = Mod.Assets.Request<Texture2D>($"Assets/Dots");
    }

    public static void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position) {
        var displays = InfinityManager.GetDisplayedInfinities(item, InfinityDisplayItem.drawItemIconParams.Context);
        if (displays.Count == 0) return;
        
        Vector2 cornerDirection = Instance.Config.start switch {
            Corner.TopLeft => new(-1, -1),
            Corner.TopRight => new(1, -1),
            Corner.BottomRight => new(1, 1),
            Corner.BottomLeft or _ => new(-1, 1),
        };

        float slotScale = InfinityDisplayItem.drawItemIconParams.Scale;
        float scale = Instance.Config.scale * slotScale;
        Vector2 dotSize = new Vector2(_dotsTexture.Height()) * scale;
        position += (TextureAssets.InventoryBack.Value.Size() / 2f * slotScale - dotSize*1.2f) * cornerDirection;
        Vector2 dotDelta = dotSize * (Instance.Config.direction == Direction.Vertical ? new Vector2(0, -cornerDirection.Y) : new Vector2(-cornerDirection.X, 0));

        foreach (var display in displays[s_index % displays.Count].Where(d => d.Value.Count > 0)) {
            DisplayDot(spriteBatch, position, display.Value, display.Infinity.Color, scale);
            position += dotDelta;
        }

    }

    public static void DisplayDot(SpriteBatch spriteBatch, Vector2 position, InfinityValue value, Color color, float scale) {
        int frame = Math.Min((int)(4 * value.Count / value.Requirement), 4)-1;
        float maxAlpha = Main.mouseTextColor / 255f;
        Vector2 origin = new(0.5f);
        if (frame != 3){
            if(!Instance.Config.displayRequirement) return;
            spriteBatch.Draw(
                _dotsTexture.Value,
                position,
                _dotsTexture.Frame(4, 1, frameX:2-frame),
                color * maxAlpha * Instance.Config.missingOpacity,
                0,
                origin,
                scale,
                SpriteEffects.FlipHorizontally,
                0
            );
        }
        spriteBatch.Draw(
            _dotsTexture.Value,
            position,
            _dotsTexture.Frame(4, 1, frameX:frame),
            color * maxAlpha,
            0,
            origin,
            scale,
            SpriteEffects.None,
            0
        );
    }

    public static void PreUpdate() {
        if (Main.GlobalTimeWrappedHourly >= s_nextIndexTime) {
            s_nextIndexTime = (s_nextIndexTime + Instance.Config.time) % 3600;
            s_index = (s_index + 1) % InfinityLoader.InfinitiesLCM;
        }
    }

    private static float s_nextIndexTime = 0;
    private static int s_index = 0;

    private static Asset<Texture2D> _dotsTexture = null!;
}

public enum Direction { Vertical, Horizontal }
public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }
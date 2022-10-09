using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableTypes;

public interface IConsumableType {
    public const int NoRequirement = 0;
    public const long NoInfinity = -2L;
    public const long NotInfinite = -1L;
    public const long MinInfinity = 0L;

    Mod Mod { get; }
    string Name { get; }
    int UID { get; }

    Category GetCategory(Item item);
    int GetRequirement(Item item);
    long GetInfinity(Player player, Item item);

    void ModifyTooltip(Item item, List<TooltipLine> tooltips);
    void DrawInInventorySlot(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    void DrawOnItemSprite(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);

    // ? Move to other interface
    int IconType { get; }
    Color Color { get; }
}

// ? remove
public interface IConsumableType<TCategory> : IConsumableType where TCategory : System.Enum {
    Category IConsumableType.GetCategory(Item item) => GetCategory(item);
    new TCategory GetCategory(Item item);
}

public interface IAmmunition : IConsumableType {
    bool ConsumesAmmo(Item item);
    Item GetAmmo(Player player, Item weapon);
    bool HasAmmo(Player player, Item weapon, out Item ammo) => (ammo = GetAmmo(player, weapon)) != null;

    void ModifyTooltip(Item weapon, Item ammo, List<TooltipLine> tooltips);
    void DrawInInventorySlot(Item weapon, Item ammo, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    void DrawOnItemSprite(Item weapon, Item ammo, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
}

public interface IToggleable : IConsumableType {
    bool DefaultsToOn { get; }
}

public interface IConfigurable : IConsumableType {
    object Settings { get; internal set; }
    System.Type SettingsType { get; }
}
public interface IConfigurable<TSettings> : IConfigurable where TSettings : new() {
    System.Type IConfigurable.SettingsType => typeof(TSettings);
    object IConfigurable.Settings { get => Settings; set => Settings = (TSettings)value; }
    new TSettings Settings { get; internal set; }
}

public interface IColorable : IConsumableType {
    Color DefaultColor { get; }
    Color IConsumableType.Color => Configs.InfinityDisplay.Instance.Colors[this.ToDefinition()];
}
public interface ICustomizable : IConsumableType { }

public interface IDetectable : IConsumableType { }


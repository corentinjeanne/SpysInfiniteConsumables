using System.Diagnostics.CodeAnalysis;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public interface ICategory<TConsumable, TCategory> : IConsumableGroup<TConsumable> where TConsumable : notnull where TCategory : System.Enum {
    TCategory GetCategory(TConsumable consumable);
}

public interface IAlternateDisplay<TConsumable> : IConsumableGroup<TConsumable> where TConsumable : notnull{
    TooltipLine AlternateTooltipLine(TConsumable consumable, TConsumable alternate);
    bool HasAlternate(Player player, TConsumable consumable, [MaybeNullWhen(false)] out TConsumable alt);
}

public interface IDetectable : IConsumableGroup { }

public interface IToggleable : IConsumableGroup {
    bool DefaultsToOn { get; }
}

public interface IColorable : IConsumableGroup {
    Color DefaultColor { get; }
}

public interface IConfigurable : IConsumableGroup {
    System.Type SettingsType { get; }
}
public interface IConfigurable<TSettings> : IConfigurable {
    System.Type IConfigurable.SettingsType => typeof(TSettings);
}
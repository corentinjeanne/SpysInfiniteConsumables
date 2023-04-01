using System.Diagnostics.CodeAnalysis;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public interface ICategory<TConsumable, TCategory> : IConsumableGroup<TConsumable> where TConsumable : notnull where TCategory : System.Enum {
    TCategory GetCategory(TConsumable consumable);
}

public interface IAmmunition<TConsumable> : IConsumableGroup<TConsumable> where TConsumable : notnull{
    bool HasAmmo(Player player, TConsumable weapon, [NotNullWhen(true)] out TConsumable? ammo);
}
public interface IStandardAmmunition<TConsumable> : IAmmunition<TConsumable>, IConsumableGroup<TConsumable> where TConsumable : notnull{
    TooltipLine WeaponLine(TConsumable consumable, TConsumable ammo);
}

public interface IDetectable : IConsumableGroup{
    bool IncludeUnknown { get; }
}

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
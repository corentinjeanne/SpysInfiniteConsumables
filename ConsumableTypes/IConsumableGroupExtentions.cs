using System.Diagnostics.CodeAnalysis;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.ConsumableGroup;

public interface ICategory<TConsumable, TCategory> : IConsumableGroup<TConsumable> where TConsumable : notnull where TCategory : System.Enum {
    TCategory GetCategory(TConsumable consumable);
    Requirement Requirement(TCategory category);
}

public interface IAlternateDisplay {
    bool HasAlternate(Player player, object consumable, [MaybeNullWhen(false)] out object alt);
    TooltipLine AlternateTooltipLine(object consumable, object alternate);

}
public interface IAlternateDisplay<TConsumable> : IAlternateDisplay where TConsumable : notnull{
    bool IAlternateDisplay.HasAlternate(Player player, object consumable, [MaybeNullWhen(false)] out object alt) {
        if(HasAlternate(player, (TConsumable)consumable, out TConsumable? t)){
            alt = t;
            return true;
        }
        alt = null;
        return false;
    }
    TooltipLine IAlternateDisplay.AlternateTooltipLine(object consumable, object alternate) => AlternateTooltipLine((TConsumable)consumable, (TConsumable)alternate);

    TooltipLine AlternateTooltipLine(TConsumable consumable, TConsumable alternate);
    bool HasAlternate(Player player, TConsumable consumable, [MaybeNullWhen(false)] out TConsumable alt);
}

public interface IToggleable : IConsumableGroup {
    bool DefaultsToOn { get; }
    bool Enabled => UID > 0 ? (bool)Config.RequirementSettings.Instance.EnabledGroups[new Config.ConsumableGroupDefinition(UID)]! : Config.RequirementSettings.Instance.EnabledGlobals[new(UID)];
}

public interface IColorable : IConsumableGroup {
    Color DefaultColor { get; }
    Color Color => Config.InfinityDisplay.Instance.Colors[this.ToDefinition()];
}

public interface IConfigurable : IConsumableGroup {
    object Settings { get; internal set; }
    System.Type SettingsType { get; }
}
public interface IConfigurable<TSettings> : IConfigurable where TSettings : notnull, new() {
    System.Type IConfigurable.SettingsType => typeof(TSettings);
    object IConfigurable.Settings { get => Settings; set => Settings = (TSettings)value; }
    new TSettings Settings { get; set; }
}

public interface IDetectable : IConsumableGroup { }
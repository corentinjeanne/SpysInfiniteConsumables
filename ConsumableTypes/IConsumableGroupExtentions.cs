using Microsoft.Xna.Framework;
using Terraria;

namespace SPIC.ConsumableGroup;

public interface IToggleable : IConsumableGroup {
    bool DefaultsToOn { get; }
    bool Enabled => UID > 0 ? (bool)Configs.RequirementSettings.Instance.EnabledTypes[new Configs.ConsumableTypeDefinition(UID)]! : Configs.RequirementSettings.Instance.EnabledGlobals[new(UID)];
}

public interface IColorable : IConsumableGroup {
    Color DefaultColor { get; }
    Color Color => Configs.InfinityDisplay.Instance.Colors[this.ToDefinition()];
}

public interface IConfigurable : IConsumableGroup {
    object Settings { get; internal set; }
    System.Type SettingsType { get; }
}
public interface IConfigurable<TSettings> : IConfigurable where TSettings : notnull, new() {
    System.Type IConfigurable.SettingsType => typeof(TSettings);
    object IConfigurable.Settings { get => Settings; set => Settings = (TSettings)value; } //? look into the config
    new TSettings Settings { get; internal set; }
}

public interface ICustomizable : IConsumableGroup { }

public interface IDetectable : IConsumableGroup { }


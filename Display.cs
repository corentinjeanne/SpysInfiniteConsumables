using System;
using System.ComponentModel;
using SpikysLib.Localization;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public interface IConfigurableDisplay : IDisplay {
    Type ConfigType { get; }
    void OnLoaded(object config) { }
}

public interface IConfigurableDisplay<TConfig> : IConfigurableDisplay where TConfig : new() {
    void OnLoaded(TConfig config) { }
    
    Type IConfigurableDisplay.ConfigType => typeof(TConfig);
    void IConfigurableDisplay.OnLoaded(object config) => OnLoaded((TConfig)config);
}

public interface IDisplay : ILocalizedModType {
    bool DefaultEnabled { get; }
    LocalizedText Label { get; }
}

public abstract class Display : ModType, IDisplay {

    [DefaultValue(true)]
    public virtual bool DefaultEnabled => true;

    public string LocalizationCategory => "Displays";
    public virtual LocalizedText Label => this.GetLocalization("Label");

    protected sealed override void Register() {
        ModTypeLookup<Display>.Register(this);
        DisplayLoader.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
        if (this is IConfigurableDisplay configurable) LanguageHelper.RegisterLocalizationKeysForMembers(configurable.ConfigType);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
}
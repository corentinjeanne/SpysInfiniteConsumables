using SPIC.Configs;
using SpikysLib.Localization;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public interface IDisplay : ILocalizedModType {
    bool DefaultEnabled { get; }
    LocalizedText Label { get; }
}

public abstract class Display : ModType, IDisplay {

    public bool Enabled { get; set; }
    public virtual bool DefaultEnabled => true;

    public string LocalizationCategory => "Displays";
    public virtual LocalizedText Label => this.GetLocalization("Label");

    protected sealed override void Register() {
        ModTypeLookup<Display>.Register(this);
        DisplayLoader.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("Label"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
        if (this is IConfigProvider configurable) LanguageHelper.RegisterLocalizationKeysForMembers(configurable.ConfigType);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
}
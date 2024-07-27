using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public abstract class Display : ModType, ILocalizedModType {

    [DefaultValue(true)]
    public virtual bool Enabled { get; internal set; } = true;
    public abstract int IconType { get; }

    public string LocalizationCategory => "Displays";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", new System.Func<string>(PrettyPrintName));

    protected sealed override void Register() {
        ModTypeLookup<Display>.Register(this);
        DisplayLoader.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public sealed override void SetupContent() => SetStaticDefaults();
}

using SPIC.Default.Components;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Configs;

public abstract class Preset : ModType, ILocalizedModType {

    public string LocalizationCategory => "Configs.Presets";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName");

    public sealed override void SetupContent() => SetStaticDefaults();

    protected sealed override void Register() {
        ModTypeLookup<Preset>.Register(this);
        PresetLoader.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public abstract int CriteriasCount { get; }

    public abstract bool MeetsCriterias(GroupConfig config);
    public abstract void ApplyCriterias(GroupConfig config);
    public virtual bool AppliesTo(IInfinityGroup group) => true;
}
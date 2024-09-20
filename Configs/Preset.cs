using SpikysLib.Configs;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Configs;

public abstract class Preset : ModType, ILocalizedModType {

    public string LocalizationCategory => "Configs.Presets";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName");

    protected override void InitTemplateInstance() => ConfigHelper.SetInstance(this);

    protected sealed override void Register() {
        ModTypeLookup<Preset>.Register(this);
        PresetLoader.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), PrettyPrintName);
        Language.GetOrRegister(this.GetLocalizationKey("Tooltip"), () => "");
    }

    public sealed override void SetupContent() => SetStaticDefaults();

    public override void Unload() => ConfigHelper.SetInstance(this, true);

    public abstract int CriteriasCount { get; }

    public abstract bool MeetsCriterias(ConsumableInfinities config);
    public abstract void ApplyCriterias(ConsumableInfinities config);
    public virtual bool AppliesTo(IConsumableInfinity infinity) => true;
}
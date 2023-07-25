using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.Configs.Presets;

public abstract class ModPreset : ModType, ILocalizedModType {

    public string LocalizationCategory => "Configs.Presets";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", new System.Func<string>(PrettyPrintName));

    public sealed override void SetupContent() => SetStaticDefaults();

    protected sealed override void Register() {
        ModTypeLookup<ModPreset>.Register(this);
        PresetLoader.Register(this);
    }

    public abstract int CriteriasCount { get; }

    public abstract bool MeetsCriterias(MetaConfig config);
    public abstract void ApplyCriterias(MetaConfig config);
}
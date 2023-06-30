using Terraria.ModLoader;

namespace SPIC.Configs.Presets;

// public class Preset
public abstract class StaticPreset<TImplementation> : Preset where TImplementation: StaticPreset<TImplementation> {
    public static TImplementation Instance => _instance ??= System.Activator.CreateInstance<TImplementation>();
    public static int ID => Instance.UID;
    private static TImplementation? _instance;
    static StaticPreset() { }
    protected StaticPreset() { }

    public static void Register() => PresetManager.Register(Instance);
}

public abstract class Preset { // TODO add ModType
    public abstract Mod Mod { get; }
    public int UID { get; internal set; }

    public string InternalName => GetType().Name;
    public virtual string DisplayName {
        get { // TODO
            return InternalName;
            // return Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), delegate () {
            //     LabelAttribute legacyLabelAttribute = ConfigManager.GetLegacyLabelAttribute(base.GetType());
            //     return ((legacyLabelAttribute != null) ? legacyLabelAttribute.LocalizationEntry : null) ?? Regex.Replace(this.Name, "([A-Z])", " $1").Trim();
            // });
        }
    }

    public abstract int CriteriasCount { get; }
    public abstract bool MeetsCriterias(GroupSettings config);
    public abstract void ApplyCriterias(GroupSettings config);

    public static NoPreset None => new();
}

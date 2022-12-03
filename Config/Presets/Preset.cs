using Terraria.ModLoader;

namespace SPIC.Config.Presets;

// public class Preset
public abstract class StaticPreset<TImplementation> : Preset where TImplementation: StaticPreset<TImplementation>, new() {
    public static TImplementation Instance => _instance ??= new TImplementation();
    public static int ID => Instance.UID;
    private static TImplementation? _instance;
    static StaticPreset() { }
    protected StaticPreset() { }

    public static void Register() => PresetManager.Register(Instance);
}

public abstract class Preset {
    public abstract Mod Mod { get; }
    public int UID { get; internal set; }

    public virtual string Name => GetType().Name;
    // public abstract int IconType { get; }

    public abstract int CriteriasCount { get; }
    public abstract bool MeetsCriterias(RequirementSettings config);
    public abstract void ApplyCriterias(RequirementSettings config);

    public static NoPreset None => new();
}
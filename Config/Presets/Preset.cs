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

public abstract class Preset {
    public abstract Mod Mod { get; }
    public int UID { get; internal set; }

    public virtual string Name => GetType().Name;
    // public abstract int IconType { get; }

    public abstract int CriteriasCount { get; }
    public abstract bool MeetsCriterias(GroupSettings config);
    public abstract void ApplyCriterias(GroupSettings config);

    public static NoPreset None => new();
}

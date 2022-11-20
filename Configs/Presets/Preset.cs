using SPIC.ConsumableGroup;
using Terraria.ModLoader;

using SPIC.VanillaConsumableTypes;
namespace SPIC.Configs.Presets;

// public class Preset
public abstract class StaticPreset<TImplementation> : Preset where TImplementation: StaticPreset<TImplementation>, new() {
    public static TImplementation Instance => _instance ??= new TImplementation();
    public static int ID => Instance.UID;
    private static TImplementation _instance;
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

public class NoPreset : Preset {
    public override Mod Mod => SpysInfiniteConsumables.Instance;

    public override int CriteriasCount => 0;


    public override void ApplyCriterias(RequirementSettings config) { }

    public override bool MeetsCriterias(RequirementSettings config) => true;
}


public class Defaults : StaticPreset<Defaults> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;

    public override int CriteriasCount => 3;

    public override void ApplyCriterias(RequirementSettings config) {
        config.EnabledTypes = new();
        config.EnabledGlobals = new();
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(RequirementSettings config) {

        foreach ((IToggleable type, bool state, bool _) in config.LoadedTypes) {
            if (state != type.DefaultsToOn) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class OneForAll : StaticPreset<OneForAll> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(RequirementSettings config) {
        bool foundEnabled = false;
        foreach ((IToggleable type, bool state, bool global) in config.LoadedTypes) {
            if(global) break;
            if (state){
                foundEnabled = true;
                break;
            }
        }
        if(!foundEnabled) {
            foreach(object key in config.EnabledTypes){
                if(!((ConsumableTypeDefinition)key).IsUnloaded) config.EnabledTypes[key] = true;
            }
        }
        config.MaxConsumableTypes = 1;
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        bool foundEnabled = false;
        foreach ((IToggleable type, bool state, bool global) in config.LoadedTypes) {
            if (global) break;
            if (state) {
                foundEnabled = true;
                break;
            }
        }
        return foundEnabled && config.MaxConsumableTypes == 1;
    }
}


public class AllEnabled : StaticPreset<AllEnabled> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 3;

    public override void ApplyCriterias(RequirementSettings config) {
        for (int i = 0; i < config.EnabledTypes.Count; i++){
            config.EnabledTypes[i] = true;
        }
        foreach (var key in config.EnabledGlobals.Keys) {
            config.EnabledGlobals[key] = true;
        }
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        foreach ((IToggleable _, bool state, bool _) in config.LoadedTypes) {
            if (!state) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class AllDisabled : StaticPreset<AllDisabled> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(RequirementSettings config) {
        for (int i = 0; i < config.EnabledTypes.Count; i++){
            config.EnabledTypes[i] = false;
        }
        foreach (var key in config.EnabledGlobals.Keys) {
            config.EnabledGlobals[key] = false;
        }
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        foreach ((IToggleable _, bool state, bool _) in config.LoadedTypes) {
            if (state) return false;
        }
        return true;
    }
}

public class JourneyCosts : StaticPreset<JourneyCosts> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 3;

    public override void ApplyCriterias(RequirementSettings config) {
        config.EnabledTypes.Move(JourneySacrifice.Instance.ToDefinition(), 0);
        config.EnabledTypes[0] = true;
        config.MaxConsumableTypes = 1;
    }

    public override bool MeetsCriterias(RequirementSettings config)
        => config.MaxConsumableTypes == 1
            && (bool)config.EnabledTypes[0]
            && ((ConsumableTypeDefinition)config.EnabledTypes.Keys.Index(0)).ConsumableType == JourneySacrifice.Instance;
    
}

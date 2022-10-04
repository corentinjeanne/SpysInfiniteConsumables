using System.Collections;
using SPIC.ConsumableTypes;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs.Presets;

// public class Preset
public abstract class Preset<TImplementation> : Preset where TImplementation: Preset<TImplementation>, new() {
    public static TImplementation Instance => _instance ??= new TImplementation();
    public static int ID => Instance.UID;
    private static TImplementation _instance;
    static Preset() { }
    protected Preset() { }

    public static void Register() => PresetManager.Register(Instance);


}
public abstract class Preset {
    public abstract Mod Mod { get; }
    public int UID { get; internal set; }

    public virtual string Name => GetType().Name;
    // public abstract int IconType { get; }

    public string Label() {
        LabelAttribute label = System.Attribute.GetCustomAttribute(GetType(), typeof(LabelAttribute), true) as LabelAttribute;
        return label is not null ? label.Label : Name;
    }

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


public class Defaults : Preset<Defaults> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;

    public override int CriteriasCount => 3;

    public override void ApplyCriterias(RequirementSettings config) {
        config.EnabledTypes = new();
        config.EnabledGlobals = new();
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        // ? order
        foreach (DictionaryEntry entry in config.EnabledTypes) {
            IToggleable inf = (IToggleable)((ConsumableTypeDefinition)entry.Key).ConsumableType;
            if ((bool)entry.Value != inf.DefaultsToOn) return false;
        }
        foreach ((ConsumableTypeDefinition def, bool state) in config.EnabledGlobals) {
            IToggleable inf = (IToggleable)def.ConsumableType;
            if (state != inf.DefaultsToOn) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class OneForAll : Preset<OneForAll> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(RequirementSettings config) {
        bool foundEnabled = false;
        for (int i = 0; i < config.EnabledTypes.Count; i++){
            if((bool)config.EnabledTypes[i])foundEnabled = true;
        }
        if(!foundEnabled) config.EnabledTypes[0] = true;
        config.MaxConsumableTypes = 1;
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        bool foundEnabled = false;
        for (int i = 0; i < config.EnabledTypes.Count; i++) {
            if ((bool)config.EnabledTypes[i]) foundEnabled = true;
        }
        return foundEnabled && config.MaxConsumableTypes == 1;
    }
}


public class AllEnabled : Preset<AllEnabled> {
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
        foreach(object state in config.EnabledTypes.Values){
            if(!(bool)state) return false;
        }
        foreach(bool state in config.EnabledGlobals.Values){
            if(!state) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class AllDisabled : Preset<AllDisabled> {
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
        foreach(object state in config.EnabledTypes.Values){
            if((bool)state) return false;
        }
        foreach(bool state in config.EnabledGlobals.Values){
            if(state) return false;
        }
        return true;
    }
}

public class JourneyCosts : Preset<JourneyCosts> {
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

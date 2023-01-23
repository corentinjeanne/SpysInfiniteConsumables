using SPIC.ConsumableGroup;
using Terraria.ModLoader;

using SPIC.VanillaGroups;
namespace SPIC.Configs.Presets;

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
        config.EnabledGroups = new();
        config.EnabledGlobals = new();
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(RequirementSettings config) {

        foreach ((IToggleable group, bool state, bool _) in config.LoadedToggleableGroups) {
            if (state != group.DefaultsToOn) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class OneForAll : StaticPreset<OneForAll> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(RequirementSettings config) {
        bool foundEnabled = false;
        foreach ((IToggleable group, bool state, bool global) in config.LoadedToggleableGroups) {
            if (global) break;
            if (state) {
                foundEnabled = true;
                break;
            }
        }
        if (!foundEnabled) {
            foreach (object key in config.EnabledGroups) {
                if (!((ConsumableGroupDefinition)key).IsUnloaded) config.EnabledGroups[key] = true;
            }
        }
        config.MaxConsumableTypes = 1;
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        bool foundEnabled = false;
        foreach ((IToggleable group, bool state, bool global) in config.LoadedToggleableGroups) {
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
        for (int i = 0; i < config.EnabledGroups.Count; i++) {
            config.EnabledGroups[i] = true;
        }
        foreach (var key in config.EnabledGlobals.Keys) {
            config.EnabledGlobals[key] = true;
        }
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        foreach ((IToggleable _, bool state, bool _) in config.LoadedToggleableGroups) {
            if (!state) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class AllDisabled : StaticPreset<AllDisabled> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(RequirementSettings config) {
        for (int i = 0; i < config.EnabledGroups.Count; i++) {
            config.EnabledGroups[i] = false;
        }
        foreach (var key in config.EnabledGlobals.Keys) {
            config.EnabledGlobals[key] = false;
        }
    }

    public override bool MeetsCriterias(RequirementSettings config) {
        foreach ((IToggleable _, bool state, bool _) in config.LoadedToggleableGroups) {
            if (state) return false;
        }
        return true;
    }
}

public class JourneyCosts : StaticPreset<JourneyCosts> {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int CriteriasCount => 3;

    public override void ApplyCriterias(RequirementSettings config) {
        config.EnabledGroups.Move(JourneySacrifice.Instance.ToDefinition(), 0);
        config.EnabledGroups[0] = true;
        config.MaxConsumableTypes = 1;
    }

    public override bool MeetsCriterias(RequirementSettings config)
        => config.MaxConsumableTypes == 1
            && (bool)config.EnabledGroups[0]!
            && ((ConsumableGroupDefinition)config.EnabledGroups.Keys.Index(0)).ConsumableType == JourneySacrifice.Instance;

}

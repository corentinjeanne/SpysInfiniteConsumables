using System;
using SPIC.Groups;

namespace SPIC.Configs.Presets;

public class Defaults : ModPreset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(ConsumableConfig config) {
        foreach ((ModGroupDefinition def, bool enable) in config.EnabledGroups.Items<ModGroupDefinition, bool>()) {
            if (enable != InfinityManager.GetModGroup(def.Mod, def.Name)!.DefaultsToOn) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
    public override void ApplyCriterias(ConsumableConfig config) {
        for(int i = 0; i < config.EnabledGroups.Count; i++) {
            ModGroupDefinition def = (ModGroupDefinition)config.EnabledGroups.Keys.Index(i);
            config.EnabledGroups[i] = InfinityManager.GetModGroup(def.Mod, def.Name)!.DefaultsToOn;
        }
        config.MaxConsumableTypes = 0;
    }

}

public class OneForMany : ModPreset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(ConsumableConfig config) {
        foreach (bool enable in config.EnabledGroups.Values) {
            if (enable) return config.MaxConsumableTypes == 1;
        }
        return false;
    }
    public override void ApplyCriterias(ConsumableConfig config) {
        config.MaxConsumableTypes = 1;
        if(!MeetsCriterias(config)) config.EnabledGroups[0] = true;
    }

}

public class AllEnabled : ModPreset {
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(ConsumableConfig config) {
        for(int i = 0; i < config.EnabledGroups.Count; i++) config.EnabledGroups[i] = true;
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(ConsumableConfig config) {
        foreach (bool enabled in config.EnabledGroups.Values) {
            if (!enabled) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class AllDisabled : ModPreset {
    public override int CriteriasCount => 1;

    public override void ApplyCriterias(ConsumableConfig config) {
        for(int i = 0; i < config.EnabledGroups.Count; i++) config.EnabledGroups[i] = false;
    }

    public override bool MeetsCriterias(ConsumableConfig config) {
        foreach (bool enabled in config.EnabledGroups.Values) {
            if (enabled) return false;
        }
        return true;
    }
}
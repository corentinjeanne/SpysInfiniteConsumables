namespace SPIC.Configs.Presets;

public class Defaults : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach ((InfinityDefinition def, bool enable) in config.EnabledInfinities.Items<InfinityDefinition, bool>()) {
            if (enable != InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultsToOn) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.EnabledInfinities.Count; i++) {
            InfinityDefinition def = (InfinityDefinition)config.EnabledInfinities.Keys.Index(i);
            config.EnabledInfinities[i] = InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultsToOn;
        }
        config.MaxConsumableTypes = 0;
    }

}

public class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enable in config.EnabledInfinities.Values) {
            if (enable) return config.MaxConsumableTypes == 1;
        }
        return false;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.MaxConsumableTypes = 1;
        if(!MeetsCriterias(config)) config.EnabledInfinities[0] = true;
    }

}

public class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.EnabledInfinities.Count; i++) config.EnabledInfinities[i] = true;
        config.MaxConsumableTypes = 0;
    }

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enabled in config.EnabledInfinities.Values) {
            if (!enabled) return false;
        }
        return config.MaxConsumableTypes == 0;
    }
}

public class AllDisabled : Preset {
    public override int CriteriasCount => 1;

    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.EnabledInfinities.Count; i++) config.EnabledInfinities[i] = false;
    }

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enabled in config.EnabledInfinities.Values) {
            if (enabled) return false;
        }
        return true;
    }
}
namespace SPIC.Configs.Presets;

public sealed class Defaults : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach ((InfinityDefinition def, bool enable) in config.Infinities.Items<InfinityDefinition, bool>()) {
            if (enable != InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultsToOn) return false;
        }
        return config.MaxUsedInfinities == 0;
    }
    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.Infinities.Count; i++) {
            InfinityDefinition def = (InfinityDefinition)config.Infinities.Keys.Index(i);
            config.Infinities[i] = InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultsToOn;
        }
        config.MaxUsedInfinities = 0;
    }

}

public sealed class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enable in config.Infinities.Values) {
            if (enable) return config.MaxUsedInfinities == 1;
        }
        return false;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.MaxUsedInfinities = 1;
        if(!MeetsCriterias(config)) config.Infinities[0] = true;
    }

}

public sealed class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.Infinities.Count; i++) config.Infinities[i] = true;
        config.MaxUsedInfinities = 0;
    }

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enabled in config.Infinities.Values) {
            if (!enabled) return false;
        }
        return config.MaxUsedInfinities == 0;
    }
}

public sealed class AllDisabled : Preset {
    public override int CriteriasCount => 1;

    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.Infinities.Count; i++) config.Infinities[i] = false;
    }

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enabled in config.Infinities.Values) {
            if (enabled) return false;
        }
        return true;
    }
}
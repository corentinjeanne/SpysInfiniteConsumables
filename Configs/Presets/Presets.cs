using SPIC.Infinities;

namespace SPIC.Configs.Presets;

public sealed class Defaults : Preset {
    public override int CriteriasCount => 3;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach ((InfinityDefinition def, bool enable) in config.Infinities.Items<InfinityDefinition, bool>()) {
            if (!enable == InfinityManager.GetInfinity(def.Mod, def.Name)?.DefaultState()) return false;
        }
        return config.UsedInfinities == 0;
    }
    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.Infinities.Count; i++) {
            InfinityDefinition def = (InfinityDefinition)config.Infinities.Keys.Index(i);
            if(!def.IsUnloaded) config.Infinities[i] = InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultState();
        }
        config.UsedInfinities = 0;
    }

}

public sealed class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool AppliesToGroup(IGroup group) => group.Infinities.Count > 1;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enable in config.Infinities.Values) {
            if (enable) return config.UsedInfinities == 1;
        }
        return false;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.UsedInfinities = 1;
        if(!MeetsCriterias(config)) config.Infinities[0] = true;
    }

}

public sealed class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override void ApplyCriterias(GroupConfig config) {
        for(int i = 0; i < config.Infinities.Count; i++) config.Infinities[i] = true;
        config.UsedInfinities = 0;
    }

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (bool enabled in config.Infinities.Values) {
            if (!enabled) return false;
        }
        return config.UsedInfinities == 0;
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

public sealed class Classic : Preset {

    public static IInfinity[] Order => new IInfinity[] { Usable.Instance, Ammo.Instance, GrabBag.Instance, Placeable.Instance };
    public override int CriteriasCount => 3;

    public override bool AppliesToGroup(IGroup group) => group is Items;

    public override bool MeetsCriterias(GroupConfig config) {
        int i = 0;
        IInfinity[] order = Order;
        foreach((InfinityDefinition def, bool value) in config.Infinities.Items<InfinityDefinition, bool>()){
            if(!value || !def.Equals(new InfinityDefinition(order[i]))) return false;
            if(++i == order.Length) break;
        }
        return config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.Infinities = new();
        foreach(IInfinity infinity in Order) config.Infinities.Add(new InfinityDefinition(infinity), infinity.DefaultState());
        foreach(IInfinity infinity in Items.Instance.Infinities) config.Infinities.TryAdd(new InfinityDefinition(infinity), infinity.DefaultState());
        config.UsedInfinities = 1;
    }
}

public sealed class JourneyRequirements : Preset {
    public override int CriteriasCount => 3;

    public override bool AppliesToGroup(IGroup group) => group is Items;

    public override bool MeetsCriterias(GroupConfig config) {
        if(!config.Infinities.Keys.Index(0).Equals(new InfinityDefinition(JourneySacrifice.Instance))) return false;
        if(!(bool)config.Infinities[0]!) return false;
        return config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.Infinities.Move(new InfinityDefinition(JourneySacrifice.Instance), 0);
        config.Infinities[0] = true;
        config.UsedInfinities = 1;
    }
}
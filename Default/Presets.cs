using SPIC.Configs;
using SpikysLib.Collections;
using SPIC.Components;
using SpikysLib.Configs;

namespace SPIC.Default.Presets;

public sealed class Defaults : Preset {
    public override int CriteriasCount => 3;

    public override bool MeetsCriterias(GroupConfig config) => !config.Infinities.Exist(kvp => kvp.Value.Key != kvp.Key.Entity?.DefaultEnabled) && config.UsedInfinities == 0;
    public override void ApplyCriterias(GroupConfig config) {
        foreach ((var def, var value) in config.Infinities) {
            if (!def.IsUnloaded) value.Key = def.Entity!.DefaultEnabled;
        }
        config.UsedInfinities = 0;
    }
}

public sealed class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool AppliesTo(IInfinityGroup group) => group.Infinities.Count > 1;

    public override bool MeetsCriterias(GroupConfig config) => config.Infinities.Values.Exist(v => v.Key) && config.UsedInfinities == 1;
    public override void ApplyCriterias(GroupConfig config) {
        config.UsedInfinities = 1;
        if (!MeetsCriterias(config)) config.Infinities[0].Key = true;
    }
}

public sealed class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) => !config.Infinities.Values.Exist(v => !v.Key) && config.UsedInfinities == 0;

    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((IToggle)config.Infinities[def]!).Key = true;
        config.UsedInfinities = 0;
    }
}

public sealed class AllDisabled : Preset {
    public override int CriteriasCount => 1;

    public override bool MeetsCriterias(GroupConfig config) => !config.Infinities.Values.Exist(v => v.Key);
    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((IToggle)config.Infinities[def]!).Key = false;
    }
}

public sealed class Classic : Preset {

    public static InfinityDefinition[] Order => [new(Infinities.Usable.Instance), new(Infinities.Ammo.Instance), new(Infinities.Placeable.Instance)];
    public override int CriteriasCount => 3;

    public override bool AppliesTo(IInfinityGroup group) => group.Infinity is Infinities.ConsumableItem;

    public override bool MeetsCriterias(GroupConfig config) {
        for (int i = 0; i < Order.Length; i++) {
            if (!config.Infinities.Keys[i].Equals(Order[i]) || !config.Infinities[i].Key) return false;
        }
        return config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        (var oldInfinities, config.Infinities) = (config.Infinities, []);
        for (int i = 0; i < Order.Length; i++) {
            InfinityDefinition def = Order[i];
            config.Infinities.Add(def, oldInfinities[def]);
            oldInfinities.Remove(def);
            config.Infinities[def].Key = true;
        }
        foreach ((var def, var value) in oldInfinities) {
            config.Infinities.Add(def, value);
            config.Infinities[def].Key = false;
        }
        config.UsedInfinities = 1;
    }
}

public sealed class JourneyRequirements : Preset {
    public override int CriteriasCount => 3;

    public override bool AppliesTo(IInfinityGroup group) => group.Infinity is Infinities.ConsumableItem;

    public override bool MeetsCriterias(GroupConfig config)
        => config.Infinities.Keys[0].Equals(new InfinityDefinition(Infinities.JourneySacrifice.Instance)) && config.Infinities[0].Key && config.UsedInfinities == 1;
    public override void ApplyCriterias(GroupConfig config) {
        InfinityDefinition def = new(Infinities.JourneySacrifice.Instance);
        config.Infinities.Move(def, 0);
        config.Infinities[0].Key = true;
        config.UsedInfinities = 1;
    }
}
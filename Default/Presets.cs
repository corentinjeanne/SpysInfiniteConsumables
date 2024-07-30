using System.Collections.Specialized;
using SPIC.Configs;
using SPIC.Configs.Presets;
using SpikysLib.Configs;
using SpikysLib.Collections;
using Terraria;

namespace SPIC.Default.Presets;

public sealed class Defaults : Preset {
    public override int CriteriasCount => 3;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach ((InfinityDefinition def, INestedValue value) in config.Infinities.Items<InfinityDefinition, INestedValue>()) {
            if ((bool)value.Key != InfinityManager.GetInfinity(def.Mod, def.Name)?.DefaultEnabled()) return false;
        }
        return config.UsedInfinities == 0;
    }
    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) {
            if(!def.IsUnloaded) ((INestedValue)config.Infinities[def]!).Key = InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultEnabled();
        }
        config.UsedInfinities = 0;
    }
}

public sealed class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool AppliesToGroup(IGroup group) => group.Infinities.Count > 1;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (INestedValue value in config.Infinities.Values) {
            if ((bool)value.Key) return config.UsedInfinities == 1;
        }
        return false;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.UsedInfinities = 1;
        if(!MeetsCriterias(config)) ((INestedValue)config.Infinities[0]!).Key = true;
    }
}

public sealed class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (INestedValue value in config.Infinities.Values) if (!(bool)value.Key) return false;
        return config.UsedInfinities == 0;
    }
    
    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((INestedValue)config.Infinities[def]!).Key = true;
        config.UsedInfinities = 0;
    }
}

public sealed class AllDisabled : Preset {
    public override int CriteriasCount => 1;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (INestedValue value in config.Infinities.Values) if ((bool)value.Key) return false;
        return true;
    }

    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((INestedValue)config.Infinities[def]!).Key = false;
    }
}

public sealed class Classic : Preset {

    public static Infinity<Item>[] Order => [Infinities.Usable.Instance, Infinities.Ammo.Instance, Infinities.Placeable.Instance];
    public override int CriteriasCount => 3;

    public override bool AppliesToGroup(IGroup group) => group is Infinities.Items;

    public override bool MeetsCriterias(GroupConfig config) {
        int i = 0;
        foreach ((InfinityDefinition def, INestedValue value) in config.Infinities.Items<InfinityDefinition, INestedValue>()){
            if (i < Order.Length ? (!def.Equals(new InfinityDefinition(Order[i])) || !(bool)value.Key) : (bool)value.Key) return false;
            i++;
        }
        return config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        (OrderedDictionary oldInfs, config.Infinities) = (config.Infinities, new());
        for (int i = 0; i < Order.Length; i++) {
            InfinityDefinition def = new(Order[i]);
            ((INestedValue)oldInfs[def]!).Key = true;
            config.Infinities.Add(def, oldInfs[def]);
            oldInfs.Remove(def);
        }
        foreach ((InfinityDefinition def, INestedValue value) in oldInfs.Items<InfinityDefinition, INestedValue>()) {
            ((INestedValue)oldInfs[def]!).Key = false;
            config.Infinities.Add(def, value);
        }
        config.UsedInfinities = 1;
    }
}

public sealed class JourneyRequirements : Preset {
    public override int CriteriasCount => 3;

    public override bool AppliesToGroup(IGroup group) => group is Infinities.Items;

    public override bool MeetsCriterias(GroupConfig config) {
        InfinityDefinition def = new(Infinities.JourneySacrifice.Instance);
        return config.Infinities[0] == def && ((bool)((INestedValue)config.Infinities[def]!).Key) && config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        InfinityDefinition def = new(Infinities.JourneySacrifice.Instance);
        config.Infinities.Move(def, 0);
        ((INestedValue)config.Infinities[def]!).Key = true;
        config.UsedInfinities = 1;
    }
}
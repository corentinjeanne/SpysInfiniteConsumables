using System.Collections.Specialized;
using SPIC.Configs;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using SpikysLib.Configs;
using SpikysLib.Extensions;
using Terraria;

namespace SPIC.Default.Presets;

public sealed class Defaults : Preset {
    public override int CriteriasCount => 3;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach ((InfinityDefinition def, INestedValue value) in config.Infinities.Items<InfinityDefinition, INestedValue>()) {
            if ((bool)value.Parent != InfinityManager.GetInfinity(def.Mod, def.Name)?.DefaultEnabled()) return false;
        }
        return config.UsedInfinities == 0;
    }
    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) {
            if(!def.IsUnloaded) ((INestedValue)config.Infinities[def]!).Parent = InfinityManager.GetInfinity(def.Mod, def.Name)!.DefaultEnabled();
        }
        config.UsedInfinities = 0;
    }
}

public sealed class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool AppliesToGroup(IGroup group) => group.Infinities.Count > 1;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (INestedValue value in config.Infinities.Values) {
            if ((bool)value.Parent) return config.UsedInfinities == 1;
        }
        return false;
    }
    public override void ApplyCriterias(GroupConfig config) {
        config.UsedInfinities = 1;
        if(!MeetsCriterias(config)) ((INestedValue)config.Infinities[0]!).Parent = true;
    }
}

public sealed class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (INestedValue value in config.Infinities.Values) if (!(bool)value.Parent) return false;
        return config.UsedInfinities == 0;
    }
    
    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((INestedValue)config.Infinities[def]!).Parent = true;
        config.UsedInfinities = 0;
    }
}

public sealed class AllDisabled : Preset {
    public override int CriteriasCount => 1;

    public override bool MeetsCriterias(GroupConfig config) {
        foreach (INestedValue value in config.Infinities.Values) if ((bool)value.Parent) return false;
        return true;
    }

    public override void ApplyCriterias(GroupConfig config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((INestedValue)config.Infinities[def]!).Parent = false;
    }
}

public sealed class Classic : Preset {

    public static Infinity<Item>[] Order => new Infinity<Item>[] { Infinities.Usable.Instance, Infinities.Ammo.Instance, Infinities.GrabBag.Instance, Infinities.Placeable.Instance };
    public override int CriteriasCount => 3;

    public override bool AppliesToGroup(IGroup group) => group is Infinities.Items;

    public override bool MeetsCriterias(GroupConfig config) {
        int i = 0;
        foreach((InfinityDefinition def, INestedValue value) in config.Infinities.Items<InfinityDefinition, INestedValue>()){
            if(def != new InfinityDefinition(Order[i]) || (bool)value.Parent != Order[i].DefaultEnabled()) return false;
            if(++i == Order.Length) break;
        }
        return config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        (OrderedDictionary oldInfs, config.Infinities) = (config.Infinities, new());
        for (int i = 0; i < Order.Length; i++) {
            InfinityDefinition def = new(Order[i]);
            config.Infinities.Add(def, oldInfs[def]);
            ((INestedValue)config.Infinities[i]!).Parent = Order[i].DefaultEnabled();
            oldInfs.Remove(def);
        }
        foreach ((InfinityDefinition def, INestedValue value) in oldInfs.Items<InfinityDefinition, INestedValue>()) config.Infinities.Add(def, value);
        config.UsedInfinities = 1;
    }
}

public sealed class JourneyRequirements : Preset {
    public override int CriteriasCount => 3;

    public override bool AppliesToGroup(IGroup group) => group is Infinities.Items;

    public override bool MeetsCriterias(GroupConfig config) {
        InfinityDefinition def = new(Infinities.JourneySacrifice.Instance);
        return config.Infinities[0] == def && ((bool)((INestedValue)config.Infinities[def]!).Parent) && config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(GroupConfig config) {
        InfinityDefinition def = new(Infinities.JourneySacrifice.Instance);
        config.Infinities.Move(def, 0);
        ((INestedValue)config.Infinities[def]!).Parent = true;
        config.UsedInfinities = 1;
    }
}
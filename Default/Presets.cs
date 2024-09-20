using SPIC.Configs;
using SpikysLib.Collections;
using SpikysLib.Configs;

namespace SPIC.Default.Presets;

public sealed class ConsumableDefaults : Preset {
    public static ConsumableDefaults Instance = null!;
    
    public override int CriteriasCount => 3;
    public static InfinityDefinition[] Order => [new(Infinities.Usable.Instance), new(Infinities.Ammo.Instance), new(Infinities.Placeable.Instance), new(Infinities.GrabBag.Instance), new(Infinities.Material.Instance), new(Infinities.JourneySacrifice.Instance)];

    public override bool MeetsCriterias(ConsumableInfinities config) => !config.Infinities.Exist(kvp => kvp.Value.Key != kvp.Key.Entity?.Defaults.Enabled) && config.UsedInfinities == 0;
    public override void ApplyCriterias(ConsumableInfinities config) {
        (var oldInfinities, config.Infinities) = (config.Infinities, []);
        foreach (InfinityDefinition def in Order) {
            config.Infinities.Add(def, oldInfinities[def]);
            oldInfinities.Remove(def);
            config.Infinities[def].Key = def.Entity!.Defaults.Enabled;
        }
        foreach ((var def, var value) in oldInfinities) {
            config.Infinities.Add(def, value);
            config.Infinities[def].Key = def.Entity!.Defaults.Enabled;
        }
        config.UsedInfinities = 0;
    }
    public override bool AppliesTo(IConsumableInfinity infinity) => infinity is Infinities.ConsumableItem;
}

public sealed class CurrencyDefaults : Preset {
    public static CurrencyDefaults Instance = null!;
    
    public override int CriteriasCount => 3;
    public static InfinityDefinition[] Order => [new(Infinities.Shop.Instance), new(Infinities.Reforging.Instance), new(Infinities.Nurse.Instance), new(Infinities.Purchase.Instance)];

    public override bool MeetsCriterias(ConsumableInfinities config) => !config.Infinities.Exist(kvp => kvp.Value.Key != kvp.Key.Entity?.Defaults.Enabled) && config.UsedInfinities == 0;
    public override void ApplyCriterias(ConsumableInfinities config) {
        (var oldInfinities, config.Infinities) = (config.Infinities, []);
        for (int i = 0; i < Order.Length; i++) {
            InfinityDefinition def = Order[i];
            config.Infinities.Add(def, oldInfinities[def]);
            oldInfinities.Remove(def);
            config.Infinities[def].Key = def.Entity!.Defaults.Enabled;
        }
        foreach ((var def, var value) in oldInfinities) {
            config.Infinities.Add(def, value);
            config.Infinities[def].Key = def.Entity!.Defaults.Enabled;
        }
        config.UsedInfinities = 0;
    }
    public override bool AppliesTo(IConsumableInfinity infinity) => infinity is Infinities.Currency;
}

public sealed class OneForMany : Preset {
    public override int CriteriasCount => 2;

    public override bool AppliesTo(IConsumableInfinity infinity) => infinity.Infinities.Count > 1;

    public override bool MeetsCriterias(ConsumableInfinities config) => config.Infinities.Values.Exist(v => v.Key) && config.UsedInfinities == 1;
    public override void ApplyCriterias(ConsumableInfinities config) {
        config.UsedInfinities = 1;
        if (!MeetsCriterias(config)) config.Infinities[0].Key = true;
    }
}

public sealed class AllEnabled : Preset {
    public override int CriteriasCount => 2;

    public override bool MeetsCriterias(ConsumableInfinities config) => !config.Infinities.Values.Exist(v => !v.Key) && config.UsedInfinities == 0;

    public override void ApplyCriterias(ConsumableInfinities config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((IToggle)config.Infinities[def]!).Key = true;
        config.UsedInfinities = 0;
    }
}

public sealed class AllDisabled : Preset {
    public override int CriteriasCount => 1;

    public override bool MeetsCriterias(ConsumableInfinities config) => !config.Infinities.Values.Exist(v => v.Key);
    public override void ApplyCriterias(ConsumableInfinities config) {
        foreach (InfinityDefinition def in config.Infinities.Keys) ((IToggle)config.Infinities[def]!).Key = false;
    }
}

public sealed class Classic : Preset {

    public static InfinityDefinition[] Order => [new(Infinities.Usable.Instance), new(Infinities.Ammo.Instance), new(Infinities.Placeable.Instance)];
    public override int CriteriasCount => 3;

    public override bool AppliesTo(IConsumableInfinity infinity) => infinity is Infinities.ConsumableItem;

    public override bool MeetsCriterias(ConsumableInfinities config) {
        for (int i = 0; i < Order.Length; i++) {
            if (!config.Infinities.Keys[i].Equals(Order[i]) || !config.Infinities[i].Key) return false;
        }
        return config.UsedInfinities == 1;
    }
    public override void ApplyCriterias(ConsumableInfinities config) {
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

    public override bool AppliesTo(IConsumableInfinity infinity) => infinity is Infinities.ConsumableItem;

    public override bool MeetsCriterias(ConsumableInfinities config)
        => config.Infinities.Keys[0].Equals(new InfinityDefinition(Infinities.JourneySacrifice.Instance)) && config.Infinities[0].Key && config.UsedInfinities == 1;
    public override void ApplyCriterias(ConsumableInfinities config) {
        InfinityDefinition def = new(Infinities.JourneySacrifice.Instance);
        config.Infinities.Move(def, 0);
        config.Infinities[0].Key = true;
        config.UsedInfinities = 1;
    }
}
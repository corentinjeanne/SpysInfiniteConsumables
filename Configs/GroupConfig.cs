using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using SpikysLib.DataStructures;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public sealed class UsedInfinities : MultiChoice<int> {
    public UsedInfinities() : base() { }
    public UsedInfinities(int value) : base(value) { }

    [Choice] public Text All { get; set; } = new();
    [Choice, Range(1, 9999)] public int Used { get; set; } = 1;

    public override int Value {
        get => Choice == nameof(All) ? 0 : Used;
        set {
            if (value != 0) {
                Choice = nameof(Used);
                Used = value;
            } else Choice = nameof(All);
        }
    }

    public static implicit operator UsedInfinities(int used) => new(used);
}

public sealed class GroupConfig {
    public UsedInfinities UsedInfinities { get; set; } = 0;

    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityConfigsWrapper))] public OrderedDictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> Infinities { get; set; } = new();
}

public sealed class ClientGroupConfig {
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(InfinityClientConfigsWrapper))] public OrderedDictionary<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>> Infinities { get; set; } = new();
}

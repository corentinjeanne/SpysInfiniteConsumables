using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using SpikysLib.DataStructures;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

// public class InfinityValueWrapper<TValue> : ValueWrapper<InfinityDefinition, Toggle<Dictionary<string, object>>> {
//     [ColorNoAlpha, ColorHSLSlider, ValueWrapper]
//     public override TValue Value { get; set; } = default!;

//     public override void OnBind(ConfigElement element) {
//         if (Key.IsUnloaded) return;
//         // SpikysLib.Reflection.ConfigElement.backgroundColor.SetValue(element, Key.Entity!.Color);
//         SpikysLib.Reflection.ConfigElement.TooltipFunction.SetValue(element, () => Key.Tooltip!);
//     }
// }

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

    [CustomModConfigItem(typeof(DictionaryValuesElement)), ValueWrapper(typeof(InfinityConfigsWrapper))] public OrderedDictionary<InfinityDefinition, Toggle<Dictionary<string, object>>> Infinities { get; set; } = new();
}

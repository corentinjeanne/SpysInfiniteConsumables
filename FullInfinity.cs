using System.Collections.Generic;
using SPIC.Configs;
using Terraria;
using System;

namespace SPIC;

public class FullInfinity {

    private FullInfinity() {}

    public Requirement Requirement { get; private set; }
    public long Count { get; private set; }
    public long Infinity { get; private set; }
    public List<object> Extras { get; private set; } = new();

    public static FullInfinity None => new();

    public static FullInfinity With(Requirement requirement, long count, long infinity, params object[] extras) => new() {
        Requirement = requirement,
        Count = count,
        Infinity = infinity,
        Extras = new(extras)
    };

    internal static FullInfinity WithRequirement<TConsumable>(TConsumable consumable, Infinity<TConsumable> infinity) where TConsumable : notnull {
        FullInfinity fullInfinity = new();
        Group<TConsumable> group = infinity.Group;

        List<object> extras = new();
        Requirement requirement = infinity.GetRequirement(consumable, extras);

        infinity.ModifyRequirement(consumable, ref requirement, extras);
        
        if (group.Config.HasCustomCount(consumable, infinity, out Count? custom)) {
            extras.Clear();
            extras.Add($"{Localization.Keys.CommonItemTooltips}.Custom");
            requirement = new Requirement(custom, requirement.Multiplier == 0 ? 1 : requirement.Multiplier);
        }

        fullInfinity.Requirement = requirement;
        fullInfinity.Extras = extras;
        return fullInfinity;
    }

    internal static FullInfinity WithInfinity<TConsumable>(Player player, TConsumable consumable, Infinity<TConsumable> infinity) where TConsumable : notnull {
        FullInfinity fullInfinity = WithRequirement(consumable, infinity);
        fullInfinity.Count = infinity.Group.CountConsumables(player, consumable);
        long infinityValue = fullInfinity.Requirement.Infinity(fullInfinity.Count);
        infinity.ModifyInfinity(player, consumable, fullInfinity.Requirement, fullInfinity.Count, ref infinityValue, fullInfinity.Extras);
        fullInfinity.Infinity = infinityValue;
        return fullInfinity;
    }

    public string LocalizeExtras(IInfinity infinity) {
        List<string> parts = new();
        foreach(object extra in Extras) {
            Type extraType = extra.GetType();
            if(_extraLocs.TryGetValue(extraType, out Func<IInfinity, object, string>? func)) parts.Add(func(infinity, extra));
            else {
                foreach((Type type, Func<IInfinity, object, string> conveter) in _extraLocs) {
                    if (!extraType.IsSubclassOf(type)) continue;
                    parts.Add(conveter(infinity, extra));
                    goto success;
                }
                parts.Add(extra.ToString()!);
            success:;
            }
        }
        return string.Join(", ", parts);
    }

    public static void RegisterExtraLocalization<TExtra>(Func<IInfinity, TExtra, string> localizer) => _extraLocs[typeof(TExtra)] = (inf, extra) => localizer(inf, (TExtra)extra);
    internal static void ClearExtraLocs() => _extraLocs.Clear();

    private static readonly Dictionary<Type, Func<IInfinity, object, string>> _extraLocs = new();

    internal static int EstimatedSize => sizeof(long) * 3 + sizeof(float) + IntPtr.Size * 3;
}
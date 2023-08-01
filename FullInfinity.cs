using System.Collections.Generic;
using SPIC.Configs;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Text.RegularExpressions;

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

    internal static FullInfinity WithRequirement<TGroup, TConsumable>(TConsumable consumable, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        FullInfinity fullInfinity = new();
        Group<TGroup, TConsumable> group = infinity.Group;

        List<object> extras = new();
        Requirement requirement = infinity.GetRequirement(consumable, extras);

        infinity.OverrideRequirement(consumable, ref requirement, extras);
        
        if (group.Config.HasCustomCount(consumable, infinity, out Count? custom)) {
            extras.Clear();
            extras.Add($"{Localization.Keys.CommonItemTooltips}.Custom");
            requirement = new Requirement(custom, requirement.Multiplier == 0 ? 1 : requirement.Multiplier);
        }

        fullInfinity.Requirement = requirement;
        fullInfinity.Extras = extras;
        return fullInfinity;
    }

    internal static FullInfinity WithInfinity<TGroup, TConsumable>(Player player, TConsumable consumable, Infinity<TGroup, TConsumable> infinity) where TGroup : Group<TGroup, TConsumable> where TConsumable : notnull {
        FullInfinity fullInfinity = WithRequirement(consumable, infinity);
        fullInfinity.Count = infinity.Group.CountConsumables(player, consumable);
        long infinityValue = fullInfinity.Requirement.Infinity(fullInfinity.Count);
        infinity.OverrideInfinity(player, consumable, fullInfinity.Requirement, fullInfinity.Count, ref infinityValue, fullInfinity.Extras);
        fullInfinity.Infinity = infinityValue;
        return fullInfinity;
    }

    public string LocalizeExtras(IInfinity infinity) {
        string GetValue(string s) => Language.GetOrRegister(s, () => Regex.Replace(s, "([A-Z])", " $1").Trim()).Value;
        List<string> parts = new();
        foreach(object extra in Extras) {
            string value = extra switch {
                System.Enum category => infinity.GetLocalization(category.ToString(), () => Regex.Replace(category.ToString(), "([A-Z])", " $1").Trim()).Value,
                LocalizedText text => text.Value,
                string s => GetValue(s),
                _ => GetValue(extra.ToString()!),
            };
            parts.Add(value);
        }
        return string.Join(", ", parts);
    }

    internal static int EstimatedSize => sizeof(long) * 3 + sizeof(float) + System.IntPtr.Size * 3;
}
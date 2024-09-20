using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SPIC;

public static class InfinityLoader {
    public static IInfinity? GetInfinity(string mod, string name) => s_infinities.Find(g => g.Mod.Name == mod && g.Name == name);

    internal static void Register<TConsumable>(Infinity<TConsumable> infinity) {
        s_infinities.Add(infinity);
        if (infinity is ConsumableInfinity<TConsumable> consumable) s_consumableInfinities.Add(consumable);
        InfinitiesLCM = s_infinities.Count * InfinitiesLCM / SpikysLib.MathHelper.GCD(InfinitiesLCM, s_infinities.Count);
    }
    public static void Unload() {
        s_infinities.Clear();
        s_consumableInfinities.Clear();
    }

    public static ReadOnlyCollection<IInfinity> Infinities => s_infinities.AsReadOnly();
    public static ReadOnlyCollection<IConsumableInfinity> ConsumableInfinities => s_consumableInfinities.AsReadOnly();

    public static int InfinitiesLCM { get; private set; } = 1;

    private static readonly List<IInfinity> s_infinities = [];
    private static readonly List<IConsumableInfinity> s_consumableInfinities = [];
}

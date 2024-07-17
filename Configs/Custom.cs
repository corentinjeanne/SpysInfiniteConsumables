using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SpikysLib.Configs;

namespace SPIC.Configs;



public sealed class Custom : MultiChoice {

    [Choice] public Count Global { get; set; } = new();

    [Choice] public Dictionary<InfinityDefinition, Count> Individual { get; set; } = new(); // Count | Count<TCategory>

    public bool TryGetIndividial(IInfinity infinity, [MaybeNullWhen(false)] out Count choice) {
        if (Choice == nameof(Individual)) return Individual.TryGetValue(new(infinity), out choice);
        choice = default;
        return false;
    }
    public bool TryGetGlobal([MaybeNullWhen(false)] out Count count) => TryGet(nameof(Global), out count);
}
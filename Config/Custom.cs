using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SPIC.Configs;



public class Custom : MultyChoice {

    [JsonIgnore]
    public IGroup Group {
        get => _group;
        internal set {
            _group = value;
            foreach (InfinityDefinition def in CustomRequirements.Keys)def.Filter = _group;
        }
    }

    [Choice]
    public Count GlobalValue { get; set; } = new();

    [Choice]
    public Dictionary<InfinityDefinition, Count> CustomRequirements { get; set; } = new();

    private IGroup _group = null!;

    public bool TryGetValue(IInfinity infinity, [MaybeNullWhen(false)] out Count count){
        if(Choice == nameof(Globals)){
            count = default;
            return false;
        }
        InfinityDefinition def = new(infinity);
        return CustomRequirements.TryGetValue(def, out count);
    }
    public bool TryGetGlobal([MaybeNullWhen(false)] out Count count){
        if(Choice == nameof(GlobalValue)){
            count = GlobalValue;
            return true;
        }
        count = default;
        return false;
    }
}
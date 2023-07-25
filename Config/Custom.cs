using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SPIC.Configs;



public class Custom : MultyChoice {


    [JsonIgnore]
    public IMetaGroup MetaGroup {
        get => _metaGroup;
        internal set {
            _metaGroup = value;
            foreach(ModGroupDefinition def in CustomRequirements.Keys) def.MetaGroup = MetaGroup;
        }
    }

    [Choice]
    public Count GlobalValue { get; set; } = new();

    [Choice]
    public Dictionary<ModGroupDefinition, Count> CustomRequirements { get; set; } = new();
    
    private IMetaGroup _metaGroup = null!;

    public bool TryGetValue(IModGroup group, [MaybeNullWhen(false)] out Count count){
        if(Choice.Name == nameof(Globals)){
            count = default;
            return false;
        }
        ModGroupDefinition def = new(group);
        return CustomRequirements.TryGetValue(def, out count);
    }
    public bool TryGetGlobal([MaybeNullWhen(false)] out Count count){
        if(Choice.Name == nameof(GlobalValue)){
            count = GlobalValue;
            return true;
        }
        count = default;
        return false;
    }
}
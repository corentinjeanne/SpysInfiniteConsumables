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

            Dictionary<GenericWrapper<ModGroupDefinition>, Count> dict = new();
            foreach (GenericWrapper<ModGroupDefinition> def in CustomRequirements.Keys) dict[GenericWrapper<ModGroupDefinition>.From(def.Value.MakeForMetagroup(_metaGroup))] = CustomRequirements[def];

            CustomRequirements.Clear();
            foreach((GenericWrapper<ModGroupDefinition> def, Count wrapper) in dict) CustomRequirements[def] = wrapper;
        }
    }

    [Choice]
    public Count GlobalValue { get; set; } = new();

    [Choice]
    public Dictionary<GenericWrapper<ModGroupDefinition>, Count> CustomRequirements { get; set; } = new();

    private IMetaGroup _metaGroup = null!;

    public bool TryGetValue(IModGroup group, [MaybeNullWhen(false)] out Count count){
        if(Choice == nameof(Globals)){
            count = default;
            return false;
        }
        ModGroupDefinition def = new(group);
        return CustomRequirements.TryGetValue(new GenericWrapper<ModGroupDefinition>(def), out count);
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
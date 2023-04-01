using System.Collections.Generic;
using Newtonsoft.Json;
using SPIC.ConsumableGroup;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;



public class Custom : MultyChoice {

    [Choice, Label("$Mods.SPIC.Configs.UI.Blacklisted.Name")]
    public object Blacklisted => new();

    [Choice]
    public Dictionary<ConsumableGroupDefinition, UniversalCountWrapper> CustomRequirements {
        get => _customRequirements;
        set {
            _customRequirements.Clear();
            foreach ((ConsumableGroupDefinition def, UniversalCountWrapper wrapper) in value) {
                if (def.IsUnloaded) {
                    if (!ModLoader.HasMod(def.Mod)) _customRequirements[def] = wrapper;
                    continue;
                }
                IConsumableGroup group = def.ConsumableGroup;
                if(!group.GetType().ImplementsInterface(typeof(IConsumableGroup<,>), out System.Type? iGroupGen)) continue;
                CustomWrapper? wrapperAttrib = (CustomWrapper?)System.Attribute.GetCustomAttribute(iGroupGen.GenericTypeArguments[1], typeof(CustomWrapper));
                UniversalCountWrapper count = wrapper;
                if (wrapperAttrib is not null) {
                    string json = JsonConvert.SerializeObject(wrapper);
                    count = (UniversalCountWrapper)JsonConvert.DeserializeObject(json, wrapperAttrib.WrapperType)!;
                }
                _customRequirements[def] = count;
            }
        }
    }
    private readonly Dictionary<ConsumableGroupDefinition, UniversalCountWrapper> _customRequirements = new();
}
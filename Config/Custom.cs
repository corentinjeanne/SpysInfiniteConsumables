using System.Collections.Generic;

namespace SPIC.Configs;

public class Custom : MultyChoice {

    public Custom() {
        GlobalRequirement = new();
        IndividualRequirements = new();
    }

    [Choice]
    public ItemCountWrapper GlobalRequirement = new();
    [Choice]
    public Dictionary<ConsumableGroupDefinition, ItemCountWrapper> IndividualRequirements = new();
}
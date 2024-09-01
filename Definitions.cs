using System.Linq;
using System.ComponentModel;
using SpikysLib;

namespace SPIC;

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class InfinityDefinition : EntityDefinition<InfinityDefinition, IInfinity> {
    public InfinityDefinition() : base() { }
    public InfinityDefinition(string fullName) : base(fullName) { }
    public InfinityDefinition(string mod, string name) : base(mod, name) { }
    public InfinityDefinition(IInfinity infinity) : this(infinity.Mod.Name, infinity.Name) { }

    public override IInfinity? Entity => InfinityManager.GetInfinity(Mod, Name);

    public override string DisplayName { get {
        IInfinity? infinity = Entity;
        return infinity is not null ? infinity.Label.Value : base.DisplayName;
    } }

    public override InfinityDefinition[] GetValues() => InfinityManager.Infinities.Select(infinity => new InfinityDefinition(infinity)).ToArray();
}
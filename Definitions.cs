using System.Linq;
using System.ComponentModel;
using SpikysLib;
using SPIC.Configs;
using Newtonsoft.Json;
using SPIC.Default.Components;

namespace SPIC;

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class PresetDefinition : EntityDefinition<PresetDefinition, Preset> {
    public PresetDefinition() : base() { }
    public PresetDefinition(string key) : base(key) { }
    public PresetDefinition(string mod, string name) : base(mod, name) { }

    public override Preset? Entity => PresetLoader.GetPreset(Mod, Name);

    public override bool AllowNull => true;

    [JsonIgnore] public IInfinityGroup? Filter { get; set; }

    public override PresetDefinition[] GetValues() => (Filter?.Presets ?? PresetLoader.Presets).Select(preset => new PresetDefinition(preset.Mod.Name, preset.Name)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class InfinityDefinition : EntityDefinition<InfinityDefinition, IInfinity> {
    public InfinityDefinition() : base() { }
    public InfinityDefinition(string fullName) : base(fullName) { }
    public InfinityDefinition(string mod, string name) : base(mod, name) { }
    public InfinityDefinition(IInfinity infinity) : this(infinity.Mod.Name, infinity.Name) { }

    public override IInfinity? Entity => InfinityManager.GetInfinity(Mod, Name);

    public override string DisplayName => Entity?.Label.Value ?? base.DisplayName;

    public override InfinityDefinition[] GetValues() => InfinityManager.Infinities.Select(infinity => new InfinityDefinition(infinity)).ToArray();
}

using System.Linq;
using System.ComponentModel;
using SpikysLib;
using Newtonsoft.Json;

namespace SPIC.Configs;

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class PresetDefinition : EntityDefinition<PresetDefinition, Preset> {
    public PresetDefinition() : base() { }
    public PresetDefinition(string key) : base(key) { }
    public PresetDefinition(string mod, string name) : base(mod, name) { }
    public PresetDefinition(Preset preset) : this(preset.Mod.Name, preset.Name) { }

    public override Preset? Entity => PresetLoader.GetPreset(Mod, Name);

    public override bool AllowNull => true;

    [JsonIgnore] public IConsumableInfinity? Consumable { get; set; }

    public override PresetDefinition[] GetValues() => (Consumable?.Presets ?? PresetLoader.Presets).Select(preset => new PresetDefinition(preset.Mod.Name, preset.Name)).ToArray();
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

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class DisplayDefinition : EntityDefinition<DisplayDefinition, Display> {
    public DisplayDefinition() : base() { }
    public DisplayDefinition(string fullName) : base(fullName) { }
    public DisplayDefinition(string mod, string name) : base(mod, name) { }
    public DisplayDefinition(Display entity) : this(entity.Mod.Name, entity.Name) { }

    public override Display? Entity => DisplayLoader.GetDisplay(Mod, Name);

    public override string DisplayName => Entity?.Label.Value ?? base.DisplayName;

    public override DisplayDefinition[] GetValues() => DisplayLoader.Displays.Select(display => new DisplayDefinition(display.Mod.Name, display.Name)).ToArray();
}
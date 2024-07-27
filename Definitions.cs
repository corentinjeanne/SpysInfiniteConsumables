using SPIC.Configs.Presets;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using SpikysLib;

namespace SPIC;

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class PresetDefinition : EntityDefinition<PresetDefinition, Preset> {
    public PresetDefinition() : base() { }
    public PresetDefinition(string key) : base(key) { }
    public PresetDefinition(string mod, string name) : base(mod, name) { }

    public override Preset? Entity => PresetLoader.GetPreset(Mod, Name);

    public override bool AllowNull => true;

    [JsonIgnore] public IGroup? Filter { get; set; }
    
    public override PresetDefinition[] GetValues() => (Filter?.Presets ?? PresetLoader.Presets).Select(preset => new PresetDefinition(preset.Mod.Name, preset.Name)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class GroupDefinition : EntityDefinition<GroupDefinition, IGroup> {
    public GroupDefinition() : base(){}
    public GroupDefinition(string fullName) : base(fullName) {}
    public GroupDefinition(IGroup group) : base(group.Mod.Name, group.Name) {}

    public override IGroup? Entity => InfinityManager.GetGroup(Mod, Name);

    public override GroupDefinition[] GetValues() => InfinityManager.Groups.Select(consumable => new GroupDefinition(consumable)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class InfinityDefinition : EntityDefinition<InfinityDefinition, IInfinity> {
    public InfinityDefinition() : base() { }
    public InfinityDefinition(string fullName) : base(fullName) { }
    public InfinityDefinition(string mod, string name) : base(mod, name) { }
    public InfinityDefinition(IInfinity infinity) : this(infinity.Mod.Name, infinity.Name) { }

    public override IInfinity? Entity => InfinityManager.GetInfinity(Mod, Name);

    [JsonIgnore] public IGroup? Filter { get; set; }

    public override string DisplayName { get {
        IInfinity? infinity = Entity;
        return infinity is not null ? $"[i:{infinity.IconType}] {infinity.DisplayName}" : base.DisplayName;
    } }

    public override InfinityDefinition[] GetValues() => (Filter?.Infinities ?? InfinityManager.Infinities).Select(intinity => new InfinityDefinition(intinity)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class DisplayDefinition : EntityDefinition<DisplayDefinition, Display> {
    public DisplayDefinition() : base() { }
    public DisplayDefinition(string fullName) : base(fullName) { }
    public DisplayDefinition(string mod, string name) : base(mod, name) { }

    public override Display? Entity => DisplayLoader.GetDisplay(Mod, Name);

    [JsonIgnore] public IGroup? Filter { get; set; }

    public override string DisplayName { get {
        Display? display = Entity;
        return display is not null ? $"[i:{display.IconType}] {display.DisplayName}" : base.DisplayName;
    } }

    public override DisplayDefinition[] GetValues() => DisplayLoader.Displays.Select(display => new DisplayDefinition(display.Mod.Name, display.Name)).ToArray();
}
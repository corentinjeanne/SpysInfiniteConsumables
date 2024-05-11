using SPIC.Configs.Presets;
using System.Linq;
using Newtonsoft.Json;
using Terraria.ModLoader;
using SpikysLib.Configs.UI;
using System.ComponentModel;

namespace SPIC.Configs.UI;

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class PresetDefinition : EntityDefinition<PresetDefinition> {
    public PresetDefinition() : base() { }
    public PresetDefinition(string key) : base(key) { }
    public PresetDefinition(string mod, string name) : base(mod, name) { }

    public override int Type => PresetLoader.GetPreset(Mod, Name) is null ? -1 : 1;
    public override bool AllowNull => true;

    public override string DisplayName => PresetLoader.GetPreset(Mod, Name)?.DisplayName.Value ?? base.DisplayName;
    public override string? Tooltip => PresetLoader.GetPreset(Mod, Name)?.GetLocalization("Tooltip").Value;

    [JsonIgnore] public IGroup? Filter { get; set; }
    
    public override PresetDefinition[] GetValues() => (Filter?.Presets ?? PresetLoader.Presets).Select(preset => new PresetDefinition(preset.Mod.Name, preset.Name)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class GroupDefinition : EntityDefinition<GroupDefinition> {
    public GroupDefinition() : base(){}
    public GroupDefinition(string fullName) : base(fullName) {}
    public GroupDefinition(IGroup group) : base(group.Mod.Name, group.Name) {}

    public override int Type => InfinityManager.GetGroup(Mod, Name) is null ? -1 : 1;

    public override string DisplayName => InfinityManager.GetGroup(Mod, Name)?.DisplayName.Value ?? base.DisplayName;

    public override GroupDefinition[] GetValues() => InfinityManager.Groups.Select(consumable => new GroupDefinition(consumable)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class InfinityDefinition : EntityDefinition<InfinityDefinition> {
    public InfinityDefinition() : base() { }
    public InfinityDefinition(string fullName) : base(fullName) { }
    public InfinityDefinition(string mod, string name) : base(mod, name) { }
    public InfinityDefinition(IInfinity infinity) : this(infinity.Mod.Name, infinity.Name) { }

    public override int Type => InfinityManager.GetInfinity(Mod, Name) is null ? -1 : 1;

    [JsonIgnore] public IGroup? Filter { get; set; }

    public override string DisplayName { get {
        IInfinity? infinity = InfinityManager.GetInfinity(Mod, Name);
        return infinity is not null ? $"[i:{infinity.IconType}] {infinity.DisplayName}" : base.DisplayName;
    } }

    public override InfinityDefinition[] GetValues() => (Filter?.Infinities ?? InfinityManager.Infinities).Select(intinity => new InfinityDefinition(intinity)).ToArray();
}

[TypeConverter("SPIC.IO.ToFromStringConverterFix")]
public sealed class DisplayDefinition : EntityDefinition<DisplayDefinition> {
    public DisplayDefinition() : base() { }
    public DisplayDefinition(string fullName) : base(fullName) { }
    public DisplayDefinition(string mod, string name) : base(mod, name) { }

    public override int Type => DisplayLoader.GetDisplay(Mod, Name) is null ? -1 : 1;

    [JsonIgnore] public IGroup? Filter { get; set; }

    public override string DisplayName { get {
        Display? display = DisplayLoader.GetDisplay(Mod, Name);
        return display is not null ? $"[i:{display.IconType}] {display.DisplayName}" : base.DisplayName;
    } }

    public override DisplayDefinition[] GetValues() => DisplayLoader.Displays.Select(display => new DisplayDefinition(display.Mod.Name, display.Name)).ToArray();
}
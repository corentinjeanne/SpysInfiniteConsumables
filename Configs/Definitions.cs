using System.ComponentModel;
using Terraria.ModLoader.Config;
using SPIC.Configs.Presets;
using SPIC.Configs.UI;
using System.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Reflection;

namespace SPIC.Configs;

public sealed class DefinitionConverter : TypeConverter {
    public DefinitionConverter(Type type) {
        if(!type.IsSubclassOfGeneric(typeof(Definition<>), out Type? gen)) throw new ArgumentException($"The type {type} does not derive from the type {typeof(DefinitionConverter)}.");
        GenericType = gen;
        MethodInfo? fromString = GenericType.GetMethod("FromString", BindingFlags.Static | BindingFlags.Public, new[] { typeof(string) });
        if(fromString is null || fromString.ReturnType != GenericType.GenericTypeArguments[0]) throw new ArgumentException($"The type {GenericType} does not have a public static FromString(string) method that returns a {GenericType.GenericTypeArguments[0]}");
        FromString = fromString;
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value) => value is string ? FromString.Invoke(null, new[] { value }) : base.ConvertFrom(context, culture, value);

    public Type GenericType { get; }
    public MethodInfo FromString { get; }
}

public interface IDefinition {
    bool AllowNull { get; }
    string DisplayName { get; }
    string? Tooltip { get; }
    IList<IDefinition> GetValues();
}

[CustomModConfigItem(typeof(DefinitionElement))]
[TypeConverter("SPIC.Configs.DefinitionConverter")]
public abstract class Definition<TDefinition> : EntityDefinition, IDefinition where TDefinition : Definition<TDefinition> {
    public Definition() : base() {}
    public Definition(string key) : base(key) { }
    public Definition(string mod, string name) : base(mod, name) {}

    [JsonIgnore] public virtual string DisplayName => $"{Name} [{Mod}]{(IsUnloaded ? $" ({Language.GetTextValue("Mods.ModLoader.Unloaded")})" : string.Empty)}";
    [JsonIgnore] public virtual string? Tooltip => null;

    [JsonIgnore] public virtual bool AllowNull => false;
    public abstract TDefinition[] GetValues();
    IList<IDefinition> IDefinition.GetValues() => GetValues();

    public static TDefinition FromString(string s) => (TDefinition)Activator.CreateInstance(typeof(TDefinition), s)!;
}

public sealed class PresetDefinition : Definition<PresetDefinition> {
    public PresetDefinition() : base() { }
    public PresetDefinition(string key) : base(key) { }
    public PresetDefinition(string mod, string name) : base(mod, name) { }

    public override int Type => PresetLoader.GetPreset(Mod, Name) is null ? -1 : 1;
    
    public override PresetDefinition[] GetValues() => PresetLoader.Presets.Select(preset => new PresetDefinition(preset.Mod.Name, preset.Name)).ToArray();

    public override bool AllowNull => true;

    public override string DisplayName => PresetLoader.GetPreset(Mod, Name)?.DisplayName.Value ?? base.DisplayName;
    public override string? Tooltip => PresetLoader.GetPreset(Mod, Name)?.GetLocalization("Tooltip").Value;
}

public sealed class GroupDefinition : Definition<GroupDefinition> {
    public GroupDefinition() : base(){}
    public GroupDefinition(string fullName) : base(fullName) {}
    public GroupDefinition(IGroup group) : base(group.Mod.Name, group.Name) {}

    public override int Type => InfinityManager.GetGroup(Mod, Name) is null ? -1 : 1;

    public override string DisplayName => InfinityManager.GetGroup(Mod, Name)?.DisplayName.Value ?? base.DisplayName;

    public override GroupDefinition[] GetValues() => InfinityManager.Groups.Select(consumable => new GroupDefinition(consumable)).ToArray();
}

public sealed class InfinityDefinition : Definition<InfinityDefinition> {
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

    public override InfinityDefinition[] GetValues() => (Filter is null ? InfinityManager.Infinities : Filter.Infinities).Select(intinity => new InfinityDefinition(intinity)).ToArray();
}
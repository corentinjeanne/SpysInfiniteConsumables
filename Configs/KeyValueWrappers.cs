using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public sealed class InfinityConfigsWrapper : KeyValueWrapper<InfinityDefinition, Toggle<Dictionary<ProviderDefinition, object>>>, IKeyValueWrapper {
    [KeyValueWrapper(typeof(InfinityConfigWrapper))] public override Toggle<Dictionary<ProviderDefinition, object>> Value { get => base.Value; set => base.Value = value; }
    public override void OnBind(ConfigElement element) => OnBind(Key, element);
    
    public static void OnBind(InfinityDefinition key, ConfigElement element) {
        if (key.IsUnloaded) return;
        SpikysLib.Reflection.ConfigElement.backgroundColor.SetValue(element, key.Entity!.Color);
        SpikysLib.Reflection.ConfigElement.TooltipFunction.SetValue(element, () => key.Tooltip!);
    }
}
public sealed class InfinityConfigWrapper : KeyValueWrapper<bool, Dictionary<ProviderDefinition, object>> {
    [ColorNoAlpha, ColorHSLSlider] public override bool Key { get => base.Key; set => base.Key = value; }
    [CustomModConfigItem(typeof(DictionaryValuesElement))] public override Dictionary<ProviderDefinition, object> Value { get => base.Value; set => base.Value = value; }
}

public sealed class InfinityClientConfigsWrapper : KeyValueWrapper<InfinityDefinition, NestedValue<Color, Dictionary<ProviderDefinition, object>>>, IKeyValueWrapper {
    [KeyValueWrapper(typeof(InfinityClientConfigWrapper))] public override NestedValue<Color, Dictionary<ProviderDefinition, object>> Value { get => base.Value; set => base.Value = value; }
    public override void OnBind(ConfigElement element) => InfinityConfigsWrapper.OnBind(Key, element);
}
public sealed class InfinityClientConfigWrapper: KeyValueWrapper<Color, Dictionary<ProviderDefinition, object>> {
    [ColorNoAlpha, ColorHSLSlider] public override Color Key { get => base.Key; set => base.Key = value; }
    [CustomModConfigItem(typeof(DictionaryValuesElement))] public override Dictionary<ProviderDefinition, object> Value { get => base.Value; set => base.Value = value; }
}

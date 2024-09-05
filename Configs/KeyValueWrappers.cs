using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpikysLib.Configs;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs;

public sealed class InfinityConfigsWrapper : KeyValueWrapper<InfinityDefinition, Toggle<Dictionary<string, object>>>, IKeyValueWrapper {
    [KeyValueWrapper(typeof(InfinityConfigWrapper))] public override Toggle<Dictionary<string, object>> Value { get => base.Value; set => base.Value = value; }
    public override void OnBind(ConfigElement element) => OnBind(Key, element);
    
    public static void OnBind(InfinityDefinition key, ConfigElement element) {
        if (key.IsUnloaded) return;
        SpikysLib.Reflection.ConfigElement.backgroundColor.SetValue(element, InfinityDisplays.GetColor(key.Entity!));
        SpikysLib.Reflection.ConfigElement.TooltipFunction.SetValue(element, () => key.Tooltip!);
    }
}
public sealed class InfinityConfigWrapper : KeyValueWrapper<bool, Dictionary<string, object>> {
    [ColorNoAlpha, ColorHSLSlider] public override bool Key { get => base.Key; set => base.Key = value; }
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(ComponentConfigWrapper<>))] public override Dictionary<string, object> Value { get => base.Value; set => base.Value = value; }
}

public sealed class InfinityClientConfigsWrapper : KeyValueWrapper<InfinityDefinition, NestedValue<Color, Dictionary<string, object>>>, IKeyValueWrapper {
    [KeyValueWrapper(typeof(InfinityClientConfigWrapper))] public override NestedValue<Color, Dictionary<string, object>> Value { get => base.Value; set => base.Value = value; }
    public override void OnBind(ConfigElement element) => InfinityConfigsWrapper.OnBind(Key, element);
}
public sealed class InfinityClientConfigWrapper: KeyValueWrapper<Color, Dictionary<string, object>> {
    [ColorNoAlpha, ColorHSLSlider] public override Color Key { get => base.Key; set => base.Key = value; }
    [CustomModConfigItem(typeof(DictionaryValuesElement)), KeyValueWrapper(typeof(ComponentConfigWrapper<>))] public override Dictionary<string, object> Value { get => base.Value; set => base.Value = value; }
}

public sealed class ComponentConfigWrapper<TValue> : KeyValueWrapper<string, TValue> {
    [CustomModConfigItem(typeof(ObjectMembersElement))] public override TValue Value { get => base.Value; set => base.Value = value; }
}
using System;

namespace SPIC.Configs;

public interface IConfigurableComponents : IComponent {
    Type ConfigType { get; }
    string ConfigKey => $"{ConfigType.Assembly.GetName().Name}/{ConfigType.Name}";
    void OnLoaded(object config) { }
}

public interface IConfigurableComponents<TConfig> : IConfigurableComponents where TConfig : new() {
    Type IConfigurableComponents.ConfigType => typeof(TConfig);
    void IConfigurableComponents.OnLoaded(object config) => OnLoaded((TConfig)config);
    void OnLoaded(TConfig config) { }
}

public interface IClientConfigurableComponents : IComponent {
    Type ConfigType { get; }
    string ConfigKey => $"{ConfigType.Assembly.GetName().Name}/{(ConfigType.Name.StartsWith("Client") ? ConfigType.Name[..] : ConfigType.Name)}";
    void OnLoaded(object config) { }
}

public interface IClientConfigurableComponents<TConfig> : IClientConfigurableComponents where TConfig : new() {
    Type IClientConfigurableComponents.ConfigType => typeof(TConfig);
    void IClientConfigurableComponents.OnLoaded(object config) => OnLoaded((TConfig)config);
    void OnLoaded(TConfig config) { }
}

using System;

namespace SPIC;

public interface IComponent {
    void Load(IInfinity infinity);
    void Unload();

    IInfinity Infinity { get; }
}

public class Component<TInfinity> : IComponent where TInfinity: IInfinity {
    public virtual void Load() { }
    public virtual void Unload() { }

    public TInfinity Infinity { get; private set; } = default!;

    IInfinity IComponent.Infinity => Infinity;

    void IComponent.Load(IInfinity infinity) {
        Infinity = (TInfinity)infinity;
        Load();
    }

    void IComponent.Unload() {
        Unload();
        Infinity = default!;
    }
}

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
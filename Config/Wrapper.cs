using System;

namespace SPIC.Configs;


public interface IWrapper {
    Type Type { get; }
    object Obj { get; internal set; }
}

public class Wrapper<T> : IWrapper where T : class, new() {
    Type IWrapper.Type => typeof(T);

    object IWrapper.Obj { get => Obj; set => Obj = (T)value; }
    public T Obj { get; private set; } = new();
}
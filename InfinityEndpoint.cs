using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SPIC;

public interface IInfinityEndpoint {
    object? GetValue(object arg);
    void Clear();
    
    void Unload();
}

public interface IInfinityEndpoint<TArg, TValue> : IInfinityEndpoint {
    TValue? GetValue(TArg args);

    void Register(GetValueFn provider);
    void Register(ModifyValueFn modifier);

    ReadOnlyCollection<GetValueFn> Providers { get; }
    ReadOnlyCollection<ModifyValueFn> Modifiers { get; }

    object? IInfinityEndpoint.GetValue(object arg) => GetValue((TArg)arg);

    delegate TValue? GetValueFn(TArg args);
    delegate void ModifyValueFn(TArg args, ref TValue value);
}

public sealed class InfinityEndpoint<TArg, TValue, TIndex> : IInfinityEndpoint<TArg, TValue> where TIndex : notnull {

    public InfinityEndpoint(IndexerFn indexer) {
        _indexer = indexer;
    }

    public bool TryGetValue(TIndex index, [MaybeNullWhen(false)] out TValue value) => _values.TryGetValue(index, out value);
    public TValue GetValue(TArg args) {
        TIndex index = _indexer(args);
        if (_values.TryGetValue(index, out TValue? value)) return value;

        foreach (var provider in _providers) {
            if ((value = provider(args)) is not null) break;
        }
        if (value is null) throw new NullReferenceException("No provider returned a value");
        foreach (var modifier in _modifiers) modifier(args, ref value);

        return _values[index] = value;
    }

    public void Register(IInfinityEndpoint<TArg, TValue>.GetValueFn provider) => _providers.Add(provider);
    public void Register(IInfinityEndpoint<TArg, TValue>.ModifyValueFn modifier) => _modifiers.Add(modifier);

    public void Clear() => _values.Clear();

    public void Unload() {
        Clear();
        _providers.Clear();
        _modifiers.Clear();
    }

    public ReadOnlyCollection<IInfinityEndpoint<TArg, TValue>.GetValueFn> Providers => _providers.AsReadOnly();
    public ReadOnlyCollection<IInfinityEndpoint<TArg, TValue>.ModifyValueFn> Modifiers => _modifiers.AsReadOnly();

    private readonly List<IInfinityEndpoint<TArg, TValue>.GetValueFn> _providers = [];
    private readonly List<IInfinityEndpoint<TArg, TValue>.ModifyValueFn> _modifiers = [];
    private readonly IndexerFn _indexer;
    private readonly Dictionary<TIndex, TValue> _values = [];

    public delegate TIndex IndexerFn(TArg args);
}

public sealed class SimpleInfinityEndpoint<TArg, TValue> : IInfinityEndpoint {

    public TValue GetValue(TArg args) => Provider(args)!;

    public void Unload() {
        Provider = null!;
    }

    object? IInfinityEndpoint.GetValue(object arg) => GetValue((TArg)arg);

    void IInfinityEndpoint.Clear() { }

    public IInfinityEndpoint<TArg, TValue>.GetValueFn Provider { get; set; } = null!;
}
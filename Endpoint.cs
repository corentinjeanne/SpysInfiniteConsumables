using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace SPIC;

public interface IEndpoint {
    object? GetValue(object? arg);

    void ClearCache();
    void Unload();
}

public interface IEndpoint<TArg, TValue> : IEndpoint {
    TValue GetValue(TArg args);

    void Register(GetValueFn provider);
    void Register(ModifyValueFn modifier);

    ReadOnlyCollection<GetValueFn> Providers { get; }
    ReadOnlyCollection<ModifyValueFn> Modifiers { get; }

    object? IEndpoint.GetValue(object? arg) => GetValue((TArg)arg!);

    delegate Optional<TValue> GetValueFn(TArg args);
    delegate void ModifyValueFn(TArg args, ref TValue value);
}

public sealed class SimpleEndpoint<TArg, TValue> : IEndpoint<TArg, TValue> {

    public TValue GetValue(TArg args) => _provider!(args).Value;

    public void Register(IEndpoint<TArg, TValue>.GetValueFn provider) => _provider ??= provider;
    public void Unload() => _provider = null;

    public ReadOnlyCollection<IEndpoint<TArg, TValue>.GetValueFn> Providers => new([_provider!]);
    
    private IEndpoint<TArg, TValue>.GetValueFn? _provider;

    void IEndpoint.ClearCache() {}
    void IEndpoint<TArg, TValue>.Register(IEndpoint<TArg, TValue>.ModifyValueFn modifier) => throw new NotSupportedException();
    ReadOnlyCollection<IEndpoint<TArg, TValue>.ModifyValueFn> IEndpoint<TArg, TValue>.Modifiers => throw new NotSupportedException();
}

public sealed class Endpoint<TArg, TValue, TIndex> : IEndpoint<TArg, TValue> where TIndex : notnull {

    public Endpoint(IndexerFn indexer) {
        _indexer = indexer;
    }

    public bool TryGetValue(TIndex index, [MaybeNullWhen(false)] out TValue value) => _values.TryGetValue(index, out value);
    public TValue GetValue(TArg args) {
        TIndex index = _indexer(args);
        if (_values.TryGetValue(index, out TValue? value)) return value;
        Optional<TValue> v = new();

        foreach (var provider in _providers) {
            if ((v = provider(args)).HasValue) break;
        }
        if (!v.HasValue) throw new NullReferenceException("No provider returned a value");

        value = v.Value;
        foreach (var modifier in _modifiers) modifier(args, ref value);
        return _values[index] = value;
    }

    public void Register(IEndpoint<TArg, TValue>.GetValueFn provider) => _providers.Add(provider);
    public void Register(IEndpoint<TArg, TValue>.ModifyValueFn modifier) => _modifiers.Add(modifier);

    public void ClearCache() => _values.Clear();

    public void Unload() {
        ClearCache();
        _providers.Clear();
        _modifiers.Clear();
    }

    public ReadOnlyCollection<IEndpoint<TArg, TValue>.GetValueFn> Providers => _providers.AsReadOnly();
    public ReadOnlyCollection<IEndpoint<TArg, TValue>.ModifyValueFn> Modifiers => _modifiers.AsReadOnly();

    private readonly List<IEndpoint<TArg, TValue>.GetValueFn> _providers = [];
    private readonly List<IEndpoint<TArg, TValue>.ModifyValueFn> _modifiers = [];
    private readonly IndexerFn _indexer;

    private readonly Dictionary<TIndex, TValue> _values = [];

    public delegate TIndex IndexerFn(TArg args);
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using SpikysLib.Collections;

namespace SPIC;

public interface IEndpoint {
    object? GetValue(object? arg);

    void Unload();
}

public interface IEndpoint<TArg, TValue> : IEndpoint {
    TValue GetValue(TArg args);

    void AddProvider(ProviderFn provider);
    void AddModifier(ModifierFn modifier);

    ReadOnlyCollection<ProviderFn> Providers { get; }
    ReadOnlyCollection<ModifierFn> Modifiers { get; }

    object? IEndpoint.GetValue(object? arg) => GetValue((TArg)arg!);

    delegate Optional<TValue> ProviderFn(TArg args);
    delegate void ModifierFn(TArg args, ref TValue value);
}

public sealed class SimpleEndpoint<TArg, TValue> : IEndpoint<TArg, TValue> {

    public TValue GetValue(TArg args) => Provider(args).Value;

    public void AddProvider(IEndpoint<TArg, TValue>.ProviderFn provider) => Provider ??= provider;
    public void Unload() => Provider = null!;

    public IEndpoint<TArg, TValue>.ProviderFn Provider { get; private set; } = default!;

    ReadOnlyCollection<IEndpoint<TArg, TValue>.ProviderFn> IEndpoint<TArg, TValue>.Providers => new([Provider]);
    void IEndpoint<TArg, TValue>.AddModifier(IEndpoint<TArg, TValue>.ModifierFn modifier) => throw new NotSupportedException();
    ReadOnlyCollection<IEndpoint<TArg, TValue>.ModifierFn> IEndpoint<TArg, TValue>.Modifiers => throw new NotSupportedException();
}

public sealed class Endpoint<TArg, TValue> : IEndpoint<TArg, TValue> {
    public TValue GetValue(TArg args) {
        Optional<TValue> v = new();
        foreach (var provider in _providers) {
            if ((v = provider(args)).HasValue) break;
        }
        if (!v.HasValue) throw new NullReferenceException("No provider returned a value");

        TValue value = v.Value;
        foreach (var modifier in _modifiers) modifier(args, ref value);
        return value;
    }

    public void AddProvider(IEndpoint<TArg, TValue>.ProviderFn provider) => _providers.Add(provider);
    public void AddModifier(IEndpoint<TArg, TValue>.ModifierFn modifier) => _modifiers.Add(modifier);

    public void Unload() {
        _providers.Clear();
        _modifiers.Clear();
    }

    public ReadOnlyCollection<IEndpoint<TArg, TValue>.ProviderFn> Providers => _providers.AsReadOnly();
    public ReadOnlyCollection<IEndpoint<TArg, TValue>.ModifierFn> Modifiers => _modifiers.AsReadOnly();

    private readonly List<IEndpoint<TArg, TValue>.ProviderFn> _providers = [];
    private readonly List<IEndpoint<TArg, TValue>.ModifierFn> _modifiers = [];
}

public interface ICachedEndpoint: IEndpoint {
    void Clear();
}

public sealed class CachedEndpoint<TArg, TValue, TIndex>: IEndpoint<TArg, TValue>, ICachedEndpoint where TIndex : notnull {
    public CachedEndpoint(IndexerFn indexer): this(new Endpoint<TArg, TValue>(), indexer) {}
    public CachedEndpoint(IEndpoint<TArg, TValue> endpoint, IndexerFn indexer) {
        Endpoint = endpoint;
        _indexer = indexer;
    }

    public bool TryGetValue(TIndex index, [MaybeNullWhen(false)] out TValue value) => _values.TryGetValue(index, out value);
    public TValue GetValue(TArg args) => _values.GetOrAdd(_indexer(args), () => Endpoint.GetValue(args));
    public void Clear() => _values.Clear();
    public void Unload() {
        Endpoint.Unload();
        Clear();
        Endpoint = null!;
        _indexer = null!;
    }
    
    public void AddProvider(IEndpoint<TArg, TValue>.ProviderFn provider) => Endpoint.AddProvider(provider);
    public void AddModifier(IEndpoint<TArg, TValue>.ModifierFn modifier) => Endpoint.AddModifier(modifier);
    public ReadOnlyCollection<IEndpoint<TArg, TValue>.ProviderFn> Providers => Endpoint.Providers;
    public ReadOnlyCollection<IEndpoint<TArg, TValue>.ModifierFn> Modifiers => Endpoint.Modifiers;
    public IEndpoint<TArg, TValue> Endpoint { get; private set; }

    private IndexerFn _indexer;
    private readonly Dictionary<TIndex, TValue> _values = [];

    public delegate TIndex IndexerFn(TArg args);
}
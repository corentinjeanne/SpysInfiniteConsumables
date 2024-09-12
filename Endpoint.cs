using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using SpikysLib.Collections;

namespace SPIC;

public class FunctionList<T> : IEnumerable<T> where T: Delegate {
    public void Add(T function) => _functions.Add(function);
    public virtual void Unload() => _functions.Clear();

    public ReadOnlyCollection<T> Functions => _functions.AsReadOnly();
    protected readonly List<T> _functions = [];

    public IEnumerator<T> GetEnumerator() => _functions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_functions).GetEnumerator();
}

public interface IProvider {
    object? GetValue(object? arg);
    void Unload();
}

public interface IEndpoint<TArg, TValue> : IProvider {
    TValue GetValue(TArg arg);
    object? IProvider.GetValue(object? arg) => GetValue((TArg)arg!);
}


public sealed class ProviderList<TArg, TValue> : FunctionList<ProviderList<TArg, TValue>.ProviderFn>, IEndpoint<TArg, TValue> {
    public ProviderList(FallbackFn? fallback = null) => Fallback = fallback;

    public TValue GetValue(TArg args) {
        foreach (var provider in _functions) {
            Optional<TValue> value = provider(args);
            if (value.HasValue) return value.Value;
        }
        return Fallback is not null ? Fallback(args) : throw new NullReferenceException("No provider returned a value");
    }

    public sealed override void Unload() {
        base.Unload();
        Fallback = null!;
    }

    public FallbackFn? Fallback { get; private set; }

    public delegate Optional<TValue> ProviderFn(TArg args);
    public delegate TValue FallbackFn(TArg args);
}

public interface IModifier {
    void ModifyValue(object? arg, ref object? value);
    void Unload();
}

public sealed class ModifierList<TArg, TValue> : FunctionList<ModifierList<TArg, TValue>.ModifierFn>, IModifier {
    public void ModifyValue(TArg args, ref TValue value) {
        foreach (var modifier in _functions) modifier(args, ref value);
    }

    void IModifier.ModifyValue(object? arg, ref object? value) {
        TValue v = (TValue)value!;
        ModifyValue((TArg)arg!, ref v);
        value = v;
    }

    public delegate void ModifierFn(TArg args, ref TValue value);
}

public sealed class Endpoint<TArg, TValue> : IEndpoint<TArg, TValue> {
    public Endpoint(ProviderList<TArg, TValue>.FallbackFn? fallback = null) => Providers = new(fallback);

    public TValue GetValue(TArg args) {
        TValue value = Providers.GetValue(args);
        Modifiers.ModifyValue(args, ref value);
        return value;
    }

    public void Unload() {
        Providers.Unload();
        Modifiers.Unload();
    }

    public ProviderList<TArg, TValue> Providers { get; }
    public ModifierList<TArg, TValue> Modifiers { get; } = [];
}

public interface ICachedEndpoint : IProvider {
    void Clear();
}

public sealed class CachedEndpoint<TArg, TValue, TIndex> : IEndpoint<TArg, TValue>, ICachedEndpoint where TIndex : notnull {
    public CachedEndpoint(IndexerFn indexer) => Indexer = indexer;

    public bool TryGetValue(TIndex index, [MaybeNullWhen(false)] out TValue value) => _values.TryGetValue(index, out value);
    public TValue GetValue(TArg args) => _values.GetOrAdd(Indexer(args), () => _endpoint.GetValue(args));

    public void Clear() => _values.Clear();
    public void Unload() {
        _endpoint.Unload();
        Clear();
        Indexer = null!;
    }

    public ProviderList<TArg, TValue> Providers => _endpoint.Providers;
    public ModifierList<TArg, TValue> Modifiers => _endpoint.Modifiers;
    public IndexerFn Indexer { get; private set; }

    private readonly Endpoint<TArg, TValue> _endpoint = new();
    private readonly Dictionary<TIndex, TValue> _values = [];

    public delegate TIndex IndexerFn(TArg args);
}
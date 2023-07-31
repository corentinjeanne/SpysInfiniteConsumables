using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SPIC;

sealed internal class Cache<TItem, TKey, TValue> where TKey : notnull {

    public Cache(Func<TItem, TKey> indexer, Func<TItem, TValue> builder) {
        Indexer = indexer;
        Builder = builder;
        _cache = new();
        _building = new();
    }

    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value) {
        // Calls++;
        bool res = _cache.TryGetValue(key, out value);
        // if(res) Hits++;
        return res;
    }
    public bool TryGet(TItem item, [MaybeNullWhen(false)] out TValue value) => TryGet(Indexer(item), out value);

    public bool TryGetOrCache(TItem item, [MaybeNullWhen(false)] out TValue value) {
        TKey key = Indexer(item);
        if (TryGet(key, out value)) return true;
        if (!_building.Add(key)) return false;
        value = _cache[key] = Builder(item);
        _building.Remove(key);
        return true;
    }

    public void Clear() => _cache.Clear();
    public void Clear(TKey key) => _cache.Remove(key);
    public void Clear(TItem key) => Clear(Indexer(key));

    public Func<TItem, TKey> Indexer { get; }
    public Func<TItem, TValue> Builder { get; }

    public void ResetStats(){
        Calls = 0;
        Hits = 0;
    }

    public long Calls { get; private set; }
    public long Hits { get; private set; }
    public long Entries => _cache.Count;

    private readonly Dictionary<TKey, TValue> _cache;
    private readonly HashSet<TKey> _building;

    public override string ToString() => $"{Hits}/{Calls} hits ({(Calls == 0 ? 100 : Hits * 100 / Calls)}%), {Entries} entries";
}
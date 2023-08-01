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
        Calls++;
        bool res = _cache.TryGetValue(key, out value);
        if(res) Hits++;
        return res;
    }
    public bool TryGet(TItem item, [MaybeNullWhen(false)] out TValue value) => TryGet(Indexer(item), out value);

    public bool TryGetOrCache(TItem item, [MaybeNullWhen(false)] out TValue value) {
        TKey key = Indexer(item);
        if (TryGet(key, out value)) return true;
        if (Configs.InfinityDisplay.Instance.DisableCache || !_building.Add(key)) return false;
        value = _cache[key] = Builder(item);
        _building.Remove(key);
        if(ValueSizeEstimate is not null) EstimatedSize += ValueSizeEstimate(value);
        return true;
    }

    public void Clear() {
        _cache.Clear();
        EstimatedSize = 0;
        ClearStats();
    }
    public void ClearStats() {
        LastClearTime = Terraria.Main.GlobalTimeWrappedHourly;
        Calls = 0;
        Hits = 0;
    }

    public void Clear(TKey key) => _cache.Remove(key);
    public void Clear(TItem key) => Clear(Indexer(key));

    public Func<TItem, TKey> Indexer { get; }
    public Func<TItem, TValue> Builder { get; }
    public Func<TValue, long>? ValueSizeEstimate { get; init; }

    public float LastClearTime { get; private set; }
    public long? EstimatedSize { get; private set; }
    public long Calls { get; private set; }
    public long Hits { get; private set; }
    public long Count => _cache.Count;

    private readonly Dictionary<TKey, TValue> _cache;
    private readonly HashSet<TKey> _building;

    public string Stats() => $"{Hits} hits ({(Calls == 0 ? 100 : Hits * 100f / Calls):F2}%), {Count} keys{(EstimatedSize.HasValue ? $" ({EstimatedSize}B)" : string.Empty)} in {Terraria.Main.GlobalTimeWrappedHourly - LastClearTime:F0}s";
}
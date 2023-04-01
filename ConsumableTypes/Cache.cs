using System.Collections.Generic;

namespace SPIC.ConsumableGroup;

internal interface ICountCache {
    void ClearAll();

    void ClearRequirement(int id);
    void ClearInfinity(int id);
}
internal interface ICountCache<TCount> : ICountCache where TCount : struct, ICount<TCount>{
    Requirement<TCount> GetOrAddRequirement(int id, System.Func<Requirement<TCount>> getter);
    Infinity<TCount> GetOrAddInfinity(int id, System.Func<Infinity<TCount>> getter);
}

internal interface ICategoryCache {
    void ClearAll();

    void ClearCategory(int id);
}
internal interface ICategoryCache<TCategory> : ICategoryCache where TCategory : System.Enum {
    TCategory GetOrAddCategory(int id, System.Func<TCategory> getter);
}


internal class ConsumableCache<TCount> : ICountCache<TCount> where TCount: struct, ICount<TCount>{

    public static T GetOrAdd<T>(Dictionary<int, T> cache, int id, System.Func<T> getter) where T : notnull => cache.TryGetValue(id, out T? value) ? value : (cache[id] = getter());
    public Requirement<TCount> GetOrAddRequirement(int id, System.Func<Requirement<TCount>> getter) => GetOrAdd(_requirements, id, getter);
    public Infinity<TCount> GetOrAddInfinity(int id, System.Func<Infinity<TCount>> getter) => GetOrAdd(_infinities, id, getter);

    public virtual void ClearAll() {
        _requirements.Clear();
        _infinities.Clear();
    }
    public virtual void ClearRequirement(int id) =>_requirements.Remove(id);
    public virtual void ClearInfinity(int id) => _infinities.Remove(id);

    private readonly Dictionary<int, Requirement<TCount>> _requirements = new();
    private readonly Dictionary<int, Infinity<TCount>> _infinities = new();
}

internal sealed class ConsumableCache<TCount, TCategory> : ConsumableCache<TCount>, ICategoryCache<TCategory> where TCount : struct, ICount<TCount> where TCategory : System.Enum{

    public TCategory GetOrAddCategory(int id, System.Func<TCategory> getter) => GetOrAdd(_categories, id, getter);

    public void ClearCategory(int type) => _categories.Remove(type);

    public sealed override void ClearAll() {
        base.ClearAll();
        _categories.Clear();
    }
    
    private readonly Dictionary<int, TCategory> _categories = new();
}

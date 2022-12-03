using System.Collections.Generic;

namespace SPIC.ConsumableGroup;

internal interface ICache {
    void ClearType(int type);
    public void ClearAll();

}
internal interface ICategoryCache<TCategory> : ICache where TCategory : System.Enum {
    TCategory GetOrAddCategory(int id, System.Func<TCategory> getter);
}
internal interface ICountCache<TCount> : ICache where TCount : ICount<TCount>{
    Requirement<TCount> GetOrAddRequirement(int id, System.Func<Requirement<TCount>> getter);
    Infinity<TCount> GetOrAddInfinity(int id, System.Func<Infinity<TCount>> getter);
}


internal class ConsumableCache<TCount> : ICache, ICountCache<TCount> where TCount: ICount<TCount>{

    public static T GetOrAdd<T>(Dictionary<int, T> cache, int id, System.Func<T> getter) where T : notnull => cache.TryGetValue(id, out T? value) ? value : (cache[id] = getter());
    public Requirement<TCount> GetOrAddRequirement(int id, System.Func<Requirement<TCount>> getter) => GetOrAdd(_requirements, id, getter);
    public Infinity<TCount> GetOrAddInfinity(int id, System.Func<Infinity<TCount>> getter) => GetOrAdd(_infinities, id, getter);

    public virtual void ClearType(int type) {
        _requirements.Remove(type);
        _infinities.Remove(type);
    }
    public virtual void ClearAll() {
        _requirements.Clear();
        _infinities.Clear();
    }

    // ? reduce the amount of requirement instances
    private readonly Dictionary<int, Requirement<TCount>> _requirements = new();
    private readonly Dictionary<int, Infinity<TCount>> _infinities = new();
}

internal sealed class ConsumableCache<TCount, TCategory> : ConsumableCache<TCount>, ICategoryCache<TCategory> where TCount : ICount<TCount> where TCategory : System.Enum{

    public TCategory GetOrAddCategory(int id, System.Func<TCategory> getter) => GetOrAdd(_categories, id, getter);

    public sealed override void ClearType(int type) {
        base.ClearType(type);
        _categories.Remove(type);
    }
    public sealed override void ClearAll() {
        base.ClearAll();
        _categories.Clear();
    }
    
    private readonly Dictionary<int, TCategory> _categories = new();
}
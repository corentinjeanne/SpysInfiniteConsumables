using System;

namespace SPIC;

public interface ICategoryAccessor<TConsumable, TCategory> where TCategory : struct, Enum {
    Infinity<TConsumable> Infinity { get; }
}
